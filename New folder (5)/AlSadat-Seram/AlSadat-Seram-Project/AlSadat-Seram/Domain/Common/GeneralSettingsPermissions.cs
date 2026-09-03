namespace Domain.Common;

/// <summary>
/// General Settings module — covers configuration/reference-data areas that don't
/// warrant their own top-level module (Coupons, Governorates, Cities). Additional
/// features get added under this same ModuleCode as they're confirmed; the module
/// itself is intentionally open-ended per Belal's direction.
/// </summary>
public static class GeneralSettingsPermissions
{
    public const string ModuleCode = "GeneralSettings";
    public const string ModuleName = "الإعدادات العامة";

    public const string CouponsView = "GeneralSettings.Coupons.View";
    public const string CouponsCreate = "GeneralSettings.Coupons.Create";
    public const string CouponsUpdate = "GeneralSettings.Coupons.Update";

    public const string GovernoratesView = "GeneralSettings.Governorates.View";
    public const string GovernoratesCreate = "GeneralSettings.Governorates.Create";
    public const string GovernoratesUpdate = "GeneralSettings.Governorates.Update";

    public const string CitiesView = "GeneralSettings.Cities.View";
    public const string CitiesCreate = "GeneralSettings.Cities.Create";
    public const string CitiesUpdate = "GeneralSettings.Cities.Update";

    public static readonly ModulePermissions Module = new()
    {
        ModuleCode = ModuleCode,
        ModuleName = ModuleName,
        SortOrder = 20,
        Permissions = new List<PermissionDefinition>
        {
            new(CouponsView, "عرض الكوبونات", "عرض قائمة الكوبونات", IsBaseline: false),
            new(CouponsCreate, "إضافة كوبون", "إنشاء كوبون جديد", IsBaseline: false),
            new(CouponsUpdate, "تعديل كوبون", "تعديل بيانات كوبون قائم", IsBaseline: false),

            new(GovernoratesView, "عرض المحافظات", "عرض قائمة المحافظات", IsBaseline: false),
            new(GovernoratesCreate, "إضافة محافظة", "إنشاء محافظة جديدة", IsBaseline: false),
            new(GovernoratesUpdate, "تعديل محافظة", "تعديل بيانات محافظة", IsBaseline: false),

            new(CitiesView, "عرض المدن", "عرض قائمة المدن", IsBaseline: false),
            new(CitiesCreate, "إضافة مدينة", "إنشاء مدينة جديدة", IsBaseline: false),
            new(CitiesUpdate, "تعديل مدينة", "تعديل بيانات مدينة", IsBaseline: false),
        }
    };
}