namespace Domain.Common;

/// <summary>
/// Audit columns for entities written from F3-S1 on (D27). <c>AuditFieldsInterceptor</c> fills them
/// on every save from the token and the clock (UTC); services and DTOs never set them.
/// Older entities keep their <c>CreateBy</c>/<c>UpdateBy</c>/<c>DeleteBy</c> names until their module
/// is rewritten — the interceptor fills both spellings.
/// </summary>
public interface IAuditedEntity
{
    /// <summary><c>ApplicationUser.Id</c> of the user who created the row; null when created by the system.</summary>
    string? CreatedBy { get; set; }

    /// <summary>When the row was created (UTC).</summary>
    DateTime CreatedAt { get; set; }

    /// <summary><c>ApplicationUser.Id</c> of the last user who changed the row.</summary>
    string? UpdatedBy { get; set; }

    /// <summary>When the row was last changed (UTC); null until the first change.</summary>
    DateTime? UpdatedAt { get; set; }
}
