namespace Domain.Common;

/// <summary>
/// Fixed, code-defined permission catalog for the Employees module (Decision 5 —
/// no dynamic Module/Permission CRUD). These strings are the single source of truth:
/// they're what gets seeded into the Permissions table, what JwtService emits as
/// "permission" claims, and what [Authorize(Policy = ...)] references — never hand-type
/// the literal string anywhere else.
/// </summary>
public static class EmployeePermissions
{
    public const string ModuleName = "Employees";

    public const string View = "Employees.View";
    public const string Create = "Employees.Create";
    public const string Update = "Employees.Update";
    public const string Delete = "Employees.Delete";
    public const string AssignPermissions = "Employees.AssignPermissions";

    public static readonly string[] All =
    {
        View, Create, Update, Delete, AssignPermissions
    };
}