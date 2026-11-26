namespace Domain.Entities.Audit;

/// <summary>What an <see cref="AuditEntry"/> records.</summary>
public enum AuditAction : byte
{
    /// <summary>A row was inserted.</summary>
    Created = 1,

    /// <summary>One or more fields changed.</summary>
    Updated = 2,

    /// <summary>A row was soft-deleted (<c>IsDeleted</c> false → true) or removed.</summary>
    Deleted = 3,

    /// <summary>A soft-deleted row was brought back (<c>IsDeleted</c> true → false).</summary>
    Restored = 4,

    /// <summary>A business event written by a service through <c>IAuditTrail</c> (e.g. "account deactivated").</summary>
    Event = 5
}

/// <summary>
/// One line of the change log (screen 78, D28): who did what, when, on which document, with the
/// fields before → after. Append-only (D30): never updated or deleted, enforced by the interceptor
/// and by the database trigger <see cref="AppendOnlyTrigger"/>.
/// </summary>
public class AuditEntry
{
    /// <summary>Name of the SQL trigger that rejects <c>UPDATE</c> and <c>DELETE</c> on the table.</summary>
    public const string AppendOnlyTrigger = "TR_AuditEntries_AppendOnly";

    public const int UserNameMaxLength = 256;
    public const int AreaMaxLength = 64;
    public const int EventKeyMaxLength = 128;
    public const int EntityTypeMaxLength = 128;
    public const int EntityIdMaxLength = 64;
    public const int ReferenceMaxLength = 200;
    public const int NoteMaxLength = 500;
    public const int IpAddressMaxLength = 45;
    public const int TraceIdMaxLength = 64;

    public long Id { get; set; }

    /// <summary>When it happened (UTC).</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary><c>ApplicationUser.Id</c> from the token; null for the system (seed, background job).</summary>
    public string? UserId { get; set; }

    /// <summary>The user's full name at that moment, so the line still reads right after a rename.</summary>
    public string? UserName { get; set; }

    /// <summary>The screen/module the change belongs to (a stable key, e.g. <c>Products</c>); the UI translates it.</summary>
    public string Area { get; set; } = string.Empty;

    public AuditAction Action { get; set; }

    /// <summary>Localization key of the sentence for <see cref="AuditAction.Event"/> lines (e.g. <c>Audit.UserDeactivated</c>).</summary>
    public string? EventKey { get; set; }

    /// <summary>CLR name of the changed entity (e.g. <c>Products</c>), used with <see cref="EntityId"/> for the «افتح» link.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the changed row, as text.</summary>
    public string? EntityId { get; set; }

    /// <summary>What a person searches by: the document number (<c>INV-1042</c>) or the name (<c>نايل</c>).</summary>
    public string? Reference { get; set; }

    /// <summary>JSON array of <c>{ "field", "old", "new" }</c>; null when nothing field-level is recorded.</summary>
    public string? Changes { get; set; }

    /// <summary>Free text the user gave, e.g. the reason for a deactivation.</summary>
    public string? Note { get; set; }

    public string? IpAddress { get; set; }

    /// <summary>The request's trace id, to find the matching log lines.</summary>
    public string? TraceId { get; set; }
}
