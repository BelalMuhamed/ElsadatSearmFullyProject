namespace Domain.Common;

/// <summary>
/// Puts an entity in the change log (D29): every insert, field change, soft delete/restore and
/// removal of a row writes one <c>AuditEntry</c> in the same save. Opt-in on purpose — tokens,
/// passwords and salaries never get it; a sensitive field on an audited entity gets
/// <see cref="NotAuditedAttribute"/>.
/// </summary>
/// <param name="area">The screen/module key shown in the log's «الشاشة» column (a constant from <see cref="AuditAreas"/>).</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AuditedAttribute(string area) : Attribute
{
    /// <summary>The screen/module key, from <see cref="AuditAreas"/>.</summary>
    public string Area { get; } = area;

    /// <summary>
    /// The property people search the log by — a document number (<c>InvoiceNumber</c>) or a name.
    /// Null when the row has nothing better than its id.
    /// </summary>
    public string? ReferenceProperty { get; init; }
}
