using System.Collections.Generic;

namespace Infrastructure.Services.LocalizationServices
{
    /// <summary>
    /// key -> (Arabic, English). Add entries here module-by-module as you migrate
    /// controllers/services to use MessageKey instead of literal strings.
    /// Keys are namespaced by module ("Supplier.", "Auth.", "Common.") to avoid collisions.
    /// </summary>
    internal static class LocalizationResources
    {
        public static readonly Dictionary<string, (string Ar, string En)> Messages = new()
        {
            // ---- Common / cross-cutting (used by ExceptionHandlingMiddleware) ----
            ["Common.ServerError"] = ("حدث خطأ في الخادم. حاول مرة أخرى لاحقًا", "An internal server error occurred. Please try again later."),
            ["Common.NotFound"] = ("العنصر غير موجود", "Resource not found"),
            ["Common.Unauthorized"] = ("الرجاء تسجيل الدخول اولا", "Please log in first"),
            ["Common.BadRequest"] = ("طلب غير صالح", "Bad request"),
            ["Common.DataConflict"] = ("تعارض في البيانات", "Data conflict"),
            ["Common.ValidationFailed"] = ("البيانات المدخلة غير صالحة", "The submitted data is invalid"),
            ["Auth.InvalidGoogleToken"] = ("رمز جوجل غير صالح", "Invalid Google token"),
            // add inside the Messages dictionary
            ["Common.PermissionDenied"] = ("ليس لديك الصلاحية للقيام بهذا الإجراء", "You do not have permission to perform this action"),
            ["Common.ModuleNotAssigned"] = ("ليس لديك صلاحية الوصول لهذا القسم", "You do not have access to this module"),
            ["Permission.AssignedSuccessfully"] = ("تم تحديث صلاحيات المستخدم بنجاح", "User permissions updated successfully"),

            // ---- Example: Supplier module (add the rest as you migrate it) ----
            // ["Supplier.DuplicatePhone"] = ("رقم الهاتف مستخدم بالفعل", "Phone number already in use"),
        };
    }
}