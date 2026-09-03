namespace Domain.Common;

/// <summary>
/// Fixed, code-defined permission set for the Employees feature area, nested
/// under the HR module (Module.Code = "HR"). Codes are the single source of
/// truth: seeded into Permissions by Code, emitted as "permission" JWT claims,
/// and referenced by [Authorize(Policy = ...)]. Never hand-type the literal
/// string anywhere else.
/// </summary>
public static class EmployeePermissions
{
    public const string ModuleCode = "HR";
    public const string ModuleName = "الموارد البشرية";

    public const string View = "HR.Employees.View";
    public const string Create = "HR.Employees.Create";
    public const string Update = "HR.Employees.Update";
    public const string Delete = "HR.Employees.Delete";
    public const string AssignPermissions = "HR.Employees.AssignPermissions";

    public static readonly string[] All =
    {
        View, Create, Update, Delete, AssignPermissions
    };

    public static readonly ModulePermissions Module = new()
    {
        ModuleCode = ModuleCode,
        ModuleName = ModuleName,
        SortOrder = 10,
        Permissions = new List<PermissionDefinition>
        {
            new(View, "عرض الموظفين", "عرض بيانات الموظفين", IsBaseline: false),
            new(Create, "إضافة موظف", "إنشاء موظف جديد", IsBaseline: false),
            new(Update, "تعديل موظف", "تعديل بيانات موظف", IsBaseline: false),
            new(Delete, "حذف موظف", "حذف/استرجاع موظف", IsBaseline: false),
            new(AssignPermissions, "إدارة صلاحيات الموظفين", "منح وسحب صلاحيات المستخدمين", IsBaseline: false),
        }
    };
}