using Application.Services.contract.CurrentUserService;
using Domain.Common;
using Domain.Entities.Audit;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Infrastructure.Data.Interceptors;

/// <summary>
/// Writes the change log (D28–D30). For every <see cref="AuditedAttribute"/> entity in a save it adds
/// one <see cref="AuditEntry"/> — created, fields before → after, soft-deleted/restored, removed —
/// and stamps who/when/IP/trace on every new line, including the events queued by <c>IAuditTrail</c>.
/// <para>
/// Same transaction as the change: lines for changed/removed rows go in the same save. A new row
/// only gets its key from the database, so its line is written by a second save right after; when
/// no transaction is open this interceptor opens one around both saves, so either both are stored
/// or neither.
/// </para>
/// <para>
/// Registered after <see cref="AuditFieldsInterceptor"/>, whose columns are left out of the diff.
/// Bulk <c>ExecuteUpdate</c>/<c>ExecuteDelete</c> bypass it — don't use them on audited entities.
/// </para>
/// </summary>
public sealed class AuditEntriesInterceptor(ICurrentUserService currentUser, TimeProvider clock)
    : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<IEntityType, AuditedType?> AuditedTypes = new();

    private readonly List<(EntityEntry Source, AuditEntry Line)> _linesAwaitingKeys = [];
    private readonly List<AuditEntry> _linesAddedThisSave = [];
    private IDbContextTransaction? _ownTransaction;
    private bool _writingKeyedLines;
    private bool _userNameLoaded;
    private string? _userName;

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // useAsync: false → every await inside completes synchronously; no sync-over-async.
        CaptureAsync(eventData.Context, useAsync: false, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await CaptureAsync(eventData.Context, useAsync: true, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        WriteKeyedLinesAsync(eventData.Context, useAsync: false, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await WriteKeyedLinesAsync(eventData.Context, useAsync: true, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData) =>
        AbandonAsync(eventData.Context, useAsync: false).GetAwaiter().GetResult();

    /// <inheritdoc />
    public override async Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData, CancellationToken cancellationToken = default) =>
        await AbandonAsync(eventData.Context, useAsync: true);

    private async Task CaptureAsync(DbContext? context, bool useAsync, CancellationToken cancellationToken)
    {
        if (context is null || _writingKeyedLines)
            return;

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is AuditEntry)
            {
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                    throw new InvalidOperationException("AuditEntries is append-only (D30): a line can't be changed or removed.");
                continue;
            }

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var type = AuditedTypes.GetOrAdd(entry.Metadata, AuditedType.Find);
            if (type is null)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    var created = type.NewLine(entry, AuditAction.Created, changes: null);
                    if (type.HasKeyValue(entry))
                        Add(context, created);
                    else
                        _linesAwaitingKeys.Add((entry, created));
                    break;

                case EntityState.Modified:
                    await entry.EnsureOriginalValuesAsync(useAsync, cancellationToken);
                    var (action, changes) = type.Diff(entry);
                    if (action != AuditAction.Updated || changes.Count > 0)
                        Add(context, type.NewLine(entry, action, changes));
                    break;

                case EntityState.Deleted:
                    Add(context, type.NewLine(entry, AuditAction.Deleted, changes: null));
                    break;
            }
        }

        var newLines = context.ChangeTracker.Entries<AuditEntry>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .Concat(_linesAwaitingKeys.Select(p => p.Line))
            .ToList();

        if (newLines.Count > 0)
            await StampAsync(context, newLines, useAsync, cancellationToken);

        if (_linesAwaitingKeys.Count > 0 && context.Database.CurrentTransaction is null)
        {
            _ownTransaction = useAsync
                ? await context.Database.BeginTransactionAsync(cancellationToken)
                : context.Database.BeginTransaction();
        }
    }

    private async Task WriteKeyedLinesAsync(DbContext? context, bool useAsync, CancellationToken cancellationToken)
    {
        if (context is null || _writingKeyedLines)
            return;

        _linesAddedThisSave.Clear();
        if (_linesAwaitingKeys.Count == 0)
            return;

        try
        {
            foreach (var (source, line) in _linesAwaitingKeys)
            {
                line.EntityId = AuditedType.KeyOf(source);
                context.Add(line);
            }
            _linesAwaitingKeys.Clear();

            _writingKeyedLines = true;
            if (useAsync)
                await context.SaveChangesAsync(cancellationToken);
            else
                context.SaveChanges();
        }
        catch
        {
            await RollbackOwnTransactionAsync(useAsync);
            throw;
        }
        finally
        {
            _writingKeyedLines = false;
        }

        if (_ownTransaction is not null)
        {
            if (useAsync)
                await _ownTransaction.CommitAsync(cancellationToken);
            else
                _ownTransaction.Commit();
            await DisposeOwnTransactionAsync(useAsync);
        }
    }

    /// <summary>The business save failed: its lines must not be written by a later save.</summary>
    private async Task AbandonAsync(DbContext? context, bool useAsync)
    {
        if (_writingKeyedLines)
            return; // the second save failed; WriteKeyedLinesAsync rolls back and rethrows

        if (context is not null)
        {
            foreach (var line in _linesAddedThisSave)
                context.Entry(line).State = EntityState.Detached;
        }

        _linesAddedThisSave.Clear();
        _linesAwaitingKeys.Clear();
        await RollbackOwnTransactionAsync(useAsync);
    }

    private void Add(DbContext context, AuditEntry line)
    {
        context.Add(line);
        _linesAddedThisSave.Add(line);
    }

    private async Task StampAsync(DbContext context, List<AuditEntry> lines, bool useAsync, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var userName = await GetUserNameAsync(context, userId, useAsync, cancellationToken);
        var now = clock.GetUtcNow().UtcDateTime;
        var ipAddress = AuditFormatting.Truncate(currentUser.IpAddress, AuditEntry.IpAddressMaxLength);
        var traceId = AuditFormatting.Truncate(Activity.Current?.Id, AuditEntry.TraceIdMaxLength);

        foreach (var line in lines)
        {
            line.OccurredAt = now;
            line.UserId = userId;
            line.UserName = userName;
            line.IpAddress = ipAddress;
            line.TraceId = traceId;
        }
    }

    /// <summary>The acting user's full name, read once per request.</summary>
    private async Task<string?> GetUserNameAsync(DbContext context, string? userId, bool useAsync, CancellationToken cancellationToken)
    {
        if (_userNameLoaded || userId is null)
            return _userName;

        var query = context.Set<ApplicationUser>().Where(u => u.Id == userId).Select(u => u.FullName);
        var fullName = useAsync ? await query.FirstOrDefaultAsync(cancellationToken) : query.FirstOrDefault();

        _userName = AuditFormatting.Truncate(fullName, AuditEntry.UserNameMaxLength);
        _userNameLoaded = true;
        return _userName;
    }

    private async Task RollbackOwnTransactionAsync(bool useAsync)
    {
        if (_ownTransaction is null)
            return;

        if (useAsync)
            await _ownTransaction.RollbackAsync();
        else
            _ownTransaction.Rollback();
        await DisposeOwnTransactionAsync(useAsync);
    }

    private async Task DisposeOwnTransactionAsync(bool useAsync)
    {
        if (_ownTransaction is null)
            return;

        if (useAsync)
            await _ownTransaction.DisposeAsync();
        else
            _ownTransaction.Dispose();
        _ownTransaction = null;
    }

    /// <summary>What the log needs to know about one <see cref="AuditedAttribute"/> entity type.</summary>
    private sealed class AuditedType
    {
        private readonly string _area;
        private readonly string _entityType;
        private readonly IProperty? _reference;
        private readonly IProperty? _isDeleted;
        private readonly IReadOnlyList<IProperty> _diffed;

        private AuditedType(IEntityType entityType, AuditedAttribute attribute)
        {
            _area = attribute.Area;
            _entityType = entityType.ClrType.Name;
            _isDeleted = entityType.FindProperty(AuditColumnNames.IsDeleted);

            if (attribute.ReferenceProperty is not null)
            {
                _reference = entityType.FindProperty(attribute.ReferenceProperty)
                    ?? throw new InvalidOperationException(
                        $"[Audited] on {_entityType}: reference property '{attribute.ReferenceProperty}' is not mapped.");
            }

            _diffed = entityType.GetProperties()
                .Where(p => !p.IsPrimaryKey()
                         && !p.IsShadowProperty()
                         && !AuditColumnNames.All.Contains(p.Name)
                         && p.PropertyInfo?.GetCustomAttribute<NotAuditedAttribute>() is null)
                .ToArray();
        }

        /// <summary>Null when the entity isn't marked <see cref="AuditedAttribute"/>.</summary>
        public static AuditedType? Find(IEntityType entityType)
        {
            var attribute = entityType.ClrType.GetCustomAttribute<AuditedAttribute>(inherit: false);
            return attribute is null ? null : new AuditedType(entityType, attribute);
        }

        public static string KeyOf(EntityEntry entry) =>
            string.Join(",", entry.Metadata.FindPrimaryKey()!.Properties
                .Select(p => Convert.ToString(entry.Property(p.Name).CurrentValue, CultureInfo.InvariantCulture)));

        /// <summary>False while the database still has to generate the key.</summary>
        public bool HasKeyValue(EntityEntry entry) =>
            entry.Metadata.FindPrimaryKey()!.Properties.All(p => !entry.Property(p.Name).IsTemporary);

        public AuditEntry NewLine(EntityEntry entry, AuditAction action, IReadOnlyList<FieldChange>? changes)
        {
            var reference = _reference is null
                ? null
                : entry.State == EntityState.Deleted
                    ? entry.Property(_reference.Name).OriginalValue
                    : entry.Property(_reference.Name).CurrentValue;

            return new AuditEntry
            {
                Area = _area,
                Action = action,
                EntityType = _entityType,
                EntityId = HasKeyValue(entry) ? AuditFormatting.Truncate(KeyOf(entry), AuditEntry.EntityIdMaxLength) : null,
                Reference = AuditFormatting.Truncate(
                    Convert.ToString(reference, CultureInfo.InvariantCulture), AuditEntry.ReferenceMaxLength),
                Changes = changes is { Count: > 0 } ? JsonSerializer.Serialize(changes, AuditFormatting.Json) : null
            };
        }

        /// <summary>The soft-delete transition (if any) and the fields whose value really changed.</summary>
        public (AuditAction Action, IReadOnlyList<FieldChange> Changes) Diff(EntityEntry entry)
        {
            var action = AuditAction.Updated;
            if (_isDeleted is not null)
            {
                var flag = entry.Property(_isDeleted.Name);
                var was = flag.OriginalValue is true;
                var now = flag.CurrentValue is true;
                if (!was && now) action = AuditAction.Deleted;
                else if (was && !now) action = AuditAction.Restored;
            }

            var changes = new List<FieldChange>();
            foreach (var property in _diffed)
            {
                var value = entry.Property(property.Name);
                if (value.IsModified && !Equals(value.OriginalValue, value.CurrentValue))
                    changes.Add(new FieldChange(property.Name, value.OriginalValue, value.CurrentValue));
            }

            return (action, changes);
        }
    }

    /// <summary>One field's value before and after; serialized as <c>{ "field", "old", "new" }</c>.</summary>
    private sealed record FieldChange(string Field, object? Old, object? New);
}
