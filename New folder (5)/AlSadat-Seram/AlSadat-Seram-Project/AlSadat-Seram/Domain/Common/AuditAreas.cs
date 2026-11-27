namespace Domain.Common;

/// <summary>
/// Stable keys for the change log's «الشاشة» column (stored in <c>AuditEntry.Area</c>; the UI
/// translates them). Each module adds its own when it marks its entities <see cref="AuditedAttribute"/>.
/// Never rename a value: old lines keep it.
/// </summary>
public static class AuditAreas
{
    public const string Products = "Products";
    public const string Suppliers = "Suppliers";
    public const string Accounting = "Accounting";
    public const string Approvals = "Approvals";
}
