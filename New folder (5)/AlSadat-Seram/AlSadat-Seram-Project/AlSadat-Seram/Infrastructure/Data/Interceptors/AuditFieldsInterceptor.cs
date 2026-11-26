using Application.Services.contract.CurrentUserService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;

namespace Infrastructure.Data.Interceptors;

/// <summary>
/// Writes "who and when" on every save (D27, closes C7): the user comes from the token through
/// <see cref="ICurrentUserService"/>, the time from <see cref="TimeProvider"/> in UTC. Whatever a
/// service copied from a DTO into these columns is overwritten or not written.
/// <para>
/// Applies to every entity that maps a "by" column, matched by name so the old spellings
/// (<c>CreateBy</c>, <c>CreateAt</c>, …) and <c>IAuditedEntity</c> both work (<see cref="AuditColumnNames"/>).
/// Outside an HTTP request (seed, background job) the "by" values the code set are kept.
/// </para>
/// <para>
/// Bulk <c>ExecuteUpdate</c>/<c>ExecuteDelete</c> bypass interceptors and are not stamped.
/// </para>
/// </summary>
public sealed class AuditFieldsInterceptor(ICurrentUserService currentUser, TimeProvider clock)
    : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<IEntityType, AuditColumns?> ColumnsByType = new();

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        // useAsync: false → every await inside completes synchronously; no sync-over-async.
        StampAsync(eventData.Context, useAsync: false, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await StampAsync(eventData.Context, useAsync: true, cancellationToken);
        return result;
    }

    private async Task StampAsync(DbContext? context, bool useAsync, CancellationToken cancellationToken)
    {
        if (context is null)
            return;

        var actor = new Actor(
            InRequest: currentUser.UserPrincipal is not null,
            UserId: currentUser.UserId,
            Now: clock.GetUtcNow().UtcDateTime);

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            var columns = ColumnsByType.GetOrAdd(entry.Metadata, AuditColumns.Find);
            if (columns is null)
                continue;

            if (entry.State == EntityState.Added)
            {
                StampAdded(entry, columns, actor);
            }
            else
            {
                await entry.EnsureOriginalValuesAsync(useAsync, cancellationToken);
                StampModified(entry, columns, actor);
            }
        }
    }

    private static void StampAdded(EntityEntry entry, AuditColumns columns, Actor actor)
    {
        SetBy(entry, columns.CreatedBy, actor);
        SetAll(entry, columns.CreatedAt, actor.Now);

        if (actor.InRequest)
        {
            SetAll(entry, columns.UpdatedBy, null);
            SetAll(entry, columns.UpdatedAt, null);
        }

        if (IsDeleted(entry, columns, original: false))
            StampDeleted(entry, columns, actor);
        else if (actor.InRequest)
            ClearDeleted(entry, columns);
    }

    private static void StampModified(EntityEntry entry, AuditColumns columns, Actor actor)
    {
        Freeze(entry, columns.CreatedBy);
        Freeze(entry, columns.CreatedAt);

        SetBy(entry, columns.UpdatedBy, actor);
        SetAll(entry, columns.UpdatedAt, actor.Now);

        var wasDeleted = IsDeleted(entry, columns, original: true);
        var isDeleted = IsDeleted(entry, columns, original: false);

        if (!wasDeleted && isDeleted)
            StampDeleted(entry, columns, actor);
        else if (wasDeleted && !isDeleted)
            ClearDeleted(entry, columns);
        else
        {
            Freeze(entry, columns.DeletedBy);
            Freeze(entry, columns.DeletedAt);
        }
    }

    private static void StampDeleted(EntityEntry entry, AuditColumns columns, Actor actor)
    {
        SetBy(entry, columns.DeletedBy, actor);
        SetAll(entry, columns.DeletedAt, actor.Now);
    }

    private static void ClearDeleted(EntityEntry entry, AuditColumns columns)
    {
        SetAll(entry, columns.DeletedBy, null);
        SetAll(entry, columns.DeletedAt, null);
    }

    private static bool IsDeleted(EntityEntry entry, AuditColumns columns, bool original)
    {
        if (columns.IsDeleted is null)
            return false;

        var property = entry.Property(columns.IsDeleted.Name);
        return (original ? property.OriginalValue : property.CurrentValue) is true;
    }

    /// <summary>In a request the token decides (null when anonymous, unless the column is required);
    /// outside a request the value the code set stays.</summary>
    private static void SetBy(EntityEntry entry, IReadOnlyList<IProperty> properties, Actor actor)
    {
        if (!actor.InRequest)
            return;

        foreach (var property in properties)
        {
            if (actor.UserId is null && !property.IsNullable)
                continue;

            Set(entry, property, actor.UserId);
        }
    }

    private static void SetAll(EntityEntry entry, IReadOnlyList<IProperty> properties, object? value)
    {
        foreach (var property in properties)
        {
            if (value is null && !property.IsNullable)
                continue;

            Set(entry, property, value);
        }
    }

    private static void Set(EntityEntry entry, IProperty property, object? value)
    {
        var propertyEntry = entry.Property(property.Name);
        propertyEntry.CurrentValue = value;
        if (entry.State == EntityState.Modified)
            propertyEntry.IsModified = true;
    }

    /// <summary>The column keeps its database value: the stored value is not overwritten.</summary>
    private static void Freeze(EntityEntry entry, IReadOnlyList<IProperty> properties)
    {
        foreach (var property in properties)
        {
            var propertyEntry = entry.Property(property.Name);
            propertyEntry.CurrentValue = propertyEntry.OriginalValue;
            propertyEntry.IsModified = false;
        }
    }

    private sealed record Actor(bool InRequest, string? UserId, DateTime Now);

    private sealed record AuditColumns(
        IReadOnlyList<IProperty> CreatedBy,
        IReadOnlyList<IProperty> CreatedAt,
        IReadOnlyList<IProperty> UpdatedBy,
        IReadOnlyList<IProperty> UpdatedAt,
        IReadOnlyList<IProperty> DeletedBy,
        IReadOnlyList<IProperty> DeletedAt,
        IProperty? IsDeleted)
    {
        /// <summary>The entity's audit columns, or null when it maps no "by" column.</summary>
        public static AuditColumns? Find(IEntityType entityType)
        {
            var columns = new AuditColumns(
                Map(entityType, AuditColumnNames.CreatedBy),
                Map(entityType, AuditColumnNames.CreatedAt),
                Map(entityType, AuditColumnNames.UpdatedBy),
                Map(entityType, AuditColumnNames.UpdatedAt),
                Map(entityType, AuditColumnNames.DeletedBy),
                Map(entityType, AuditColumnNames.DeletedAt),
                entityType.FindProperty(AuditColumnNames.IsDeleted));

            var hasBy = columns.CreatedBy.Count + columns.UpdatedBy.Count + columns.DeletedBy.Count > 0;
            return hasBy ? columns : null;
        }

        private static IProperty[] Map(IEntityType entityType, string[] names) =>
            names.Select(entityType.FindProperty).OfType<IProperty>().ToArray();
    }
}
