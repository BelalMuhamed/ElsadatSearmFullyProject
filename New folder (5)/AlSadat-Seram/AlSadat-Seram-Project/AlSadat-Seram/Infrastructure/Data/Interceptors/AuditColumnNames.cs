using Domain.Common;

namespace Infrastructure.Data.Interceptors;

/// <summary>
/// Property names of the "who and when" columns, in both spellings: <see cref="IAuditedEntity"/>
/// and the older <c>CreateBy</c>/<c>CreateAt</c>/… still on entities not rewritten yet (D27).
/// </summary>
internal static class AuditColumnNames
{
    public static readonly string[] CreatedBy = [nameof(IAuditedEntity.CreatedBy), "CreateBy"];
    public static readonly string[] CreatedAt = [nameof(IAuditedEntity.CreatedAt), "CreateAt"];
    public static readonly string[] UpdatedBy = [nameof(IAuditedEntity.UpdatedBy), "UpdateBy"];
    public static readonly string[] UpdatedAt = [nameof(IAuditedEntity.UpdatedAt), "UpdateAt"];
    public static readonly string[] DeletedBy = ["DeletedBy", "DeleteBy"];
    public static readonly string[] DeletedAt = ["DeletedAt", "DeleteAt"];

    /// <summary>The soft-delete flag; its change is recorded as the action, not as a field.</summary>
    public const string IsDeleted = "IsDeleted";

    /// <summary>Every name above: these columns never appear in an audit line's field list.</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [.. CreatedBy, .. CreatedAt, .. UpdatedBy, .. UpdatedAt, .. DeletedBy, .. DeletedAt, IsDeleted],
        StringComparer.Ordinal);
}
