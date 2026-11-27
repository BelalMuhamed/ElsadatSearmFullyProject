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
            ["Common.ConcurrencyConflict"] = ("حد تاني عدّل البيانات دي قبلك. حدّث الصفحة وحاول تاني", "Someone else changed this data first. Refresh the page and try again."),
            ["Common.ValidationFailed"] = ("البيانات المدخلة غير صالحة", "The submitted data is invalid"),
            ["Common.InvalidBody"] = ("صيغة البيانات المرسلة غير صحيحة", "The request data is malformed"),
            ["Common.MethodNotAllowed"] = ("طريقة الطلب غير مسموحة", "This request method is not allowed"),
            ["Common.UnsupportedMediaType"] = ("نوع البيانات المرسلة غير مدعوم", "This content type is not supported"),
            ["Common.TooManyRequests"] = ("عدد الطلبات كبير. حاول مرة أخرى بعد قليل", "Too many requests. Please try again shortly."),
            // add inside the Messages dictionary
            ["Common.PermissionDenied"] = ("ليس لديك الصلاحية للقيام بهذا الإجراء", "You do not have permission to perform this action"),
            ["Common.ModuleNotAssigned"] = ("ليس لديك صلاحية الوصول لهذا القسم", "You do not have access to this module"),
            ["Permission.AssignedSuccessfully"] = ("تم تحديث صلاحيات المستخدم بنجاح", "User permissions updated successfully"),
            ["Auth.UserNotFound"] = ("المستخدم غير موجود", "User not found"),
            ["Permission.TargetMustBeEmployee"] = ("الصلاحيات تُمنح للموظفين فقط", "Permissions can only be granted to employees"),
            ["Permission.UnknownPermissionId"] = ("توجد صلاحية غير معروفة ضمن الطلب", "The request contains an unknown permission"),
            ["Permission.NotDelegable"] = ("لا يمكن تفويض هذه الصلاحية", "This permission cannot be delegated"),
            ["Auth.InvalidRefreshToken"] = ("رمز التحديث غير صالح أو منتهي", "Refresh token is invalid or expired"),
            ["Auth.InvalidCredentials"] =
    ("بيانات الدخول غير صحيحة", "Invalid credentials"),

            ["Auth.AccountLocked"] =
    ("تم قفل الحساب مؤقتًا بسبب محاولات دخول متكررة",
     "Account temporarily locked"),

            ["Auth.AccountDeactivated"] =
    ("تم إيقاف هذا الحساب",
     "This account has been deactivated"),

            ["Auth.InvalidRefreshToken"] =
    ("رمز التحديث غير صالح أو منتهي",
     "Refresh token is invalid or expired"),

            ["Auth.LoggedOut"] =
    ("تم تسجيل الخروج بنجاح",
     "Logged out successfully"),

            ["Auth.LoggedOutAllDevices"] =
    ("تم تسجيل الخروج من جميع الأجهزة",
     "Logged out from all devices"),

            ["Auth.RoleNotAssignable"] =
    ("لا يمكن إسناد هذا الدور",
     "This role cannot be assigned"),

            ["Permission.TargetMustBeEmployee"] =
    ("الصلاحيات تُمنح للموظفين فقط",
     "Permissions can only be granted to employees"),

            ["Permission.NotDelegable"] =
    ("لا يمكن تفويض هذه الصلاحية",
     "This permission cannot be delegated"),
            ["Auth.PasswordChangeRequired"] = ("يجب تغيير كلمة المرور الأولى قبل استخدام النظام", "You must change your initial password before using the system"),
            ["Auth.InitialPasswordSetupFailed"] = ("تعذر تجهيز كلمة المرور الأولى. لم يتم إنشاء الحساب", "Could not set up the initial password. The account was not created"),
            ["Role.AdminAssignmentForbidden"] = ("إنشاء مدير نظام أو تعديل بياناته متاح لمدير النظام فقط", "Only a system administrator can create or change an administrator"),
            ["Permission.SessionsRevoked"] = ("تم إنهاء جميع جلسات المستخدم", "All user sessions have been revoked"),
            ["Auth.RevokeSessionsFailed"] = ("تعذر إنهاء جلسات المستخدم. لم يتم تغيير أي شيء", "Could not revoke the user's sessions. Nothing was changed"),
            ["Journal.AccountNotPostable"] = ("فيه حساب في القيد مقفول أو حساب أب، ومينفعش يتقيد عليه", "A line hits an inactive or parent account, which can't take entries"),
            ["Journal.EntryNotFound"] = ("القيد ده مش موجود", "Journal entry not found"),
            ["Journal.NotPosted"] = ("القيد لسه مترحّلش، فمينفعش يتعكس", "The entry isn't posted yet, so it can't be reversed"),
            ["Journal.AlreadyReversed"] = ("القيد ده اتعكس قبل كده", "This entry has already been reversed"),
            ["Journal.CannotReverseReversal"] = ("ده قيد عكسي، ومينفعش يتعكس تاني", "This is a reversal entry and can't be reversed"),

            // Approvals (F7)
            ["Approval.NotFound"] = ("الطلب ده مش موجود", "Approval request not found"),
            ["Approval.AlreadyDecided"] = ("الطلب ده اتاخد فيه قرار خلاص", "This request has already been decided"),
            ["Approval.OwnRequest"] = ("مينفعش توافق أو ترفض طلب انت اللي طلبته", "You can't decide your own request"),
            ["Approval.NotAllowed"] = ("معندكش صلاحية تقرر في النوع ده من الطلبات", "You aren't allowed to decide this type of request"),
            ["Approval.ReasonRequired"] = ("لازم تكتب السبب، ولحد 500 حرف", "A reason is required, up to 500 characters"),
            ["Approval.AlreadyPending"] = ("فيه طلب لنفس الحاجة لسه مستني قرار", "A request for this is already waiting for a decision"),
            ["Approval.NotRequester"] = ("اللي طلب الطلب بس هو اللي يقدر يسحبه", "Only the requester can withdraw this request"),
            ["Approval.TypeMismatch"] = ("الطلب ده من نوع تاني", "This request is of a different type"),
            ["Approval.Submitted"] = ("الطلب اتبعت لمركز الموافقات", "The request was sent to the approvals center"),
            ["Approval.Approved"] = ("اتوافق على الطلب", "The request was approved"),
            ["Approval.Rejected"] = ("الطلب اترفض", "The request was rejected"),
            ["Approval.Withdrawn"] = ("الطلب اتسحب", "The request was withdrawn"),

            // Approval type labels (F7), used inside notifications
            ["ApprovalType.SalesInvoiceCancel"] = ("إلغاء فاتورة بيع", "Sales invoice cancellation"),
            ["ApprovalType.PurchaseInvoiceCancel"] = ("إلغاء فاتورة شراء", "Purchase invoice cancellation"),
            ["ApprovalType.PointsToPlumber"] = ("تحويل نقاط لسبّاك", "Points transfer to a plumber"),
            ["ApprovalType.TreasuryConfirmation"] = ("تأكيد خزنة", "Treasury confirmation"),
            ["ApprovalType.Leave"] = ("إجازة", "Leave"),
            ["ApprovalType.Loan"] = ("قرض", "Loan"),
            ["ApprovalType.PermissionRequest"] = ("طلب صلاحية", "Permission request"),
            ["ApprovalType.PasswordReset"] = ("نسيت كلمة السر", "Password reset"),

            // Notifications (F8)
            ["Notification.NotFound"] = ("الإشعار ده مش موجود", "Notification not found"),
            ["Notification.CannotMute"] = ("طلبات الموافقة وتغيير الصلاحيات مينفعش تتقفل", "Approval requests and permission changes can't be turned off"),
            ["Notification.ApprovalRequested"] = ("{0} بعت طلب: {1}", "{0} sent a request: {1}"),
            ["Notification.ApprovalApproved"] = ("{0} وافق على طلبك: {1}", "{0} approved your request: {1}"),
            ["Notification.ApprovalRejected"] = ("{0} رفض طلبك: {1} · السبب: {2}", "{0} rejected your request: {1} · Reason: {2}"),
            ["Notification.Someone"] = ("حد", "Someone"),
            // ---- Example: Supplier module (add the rest as you migrate it) ----
            // ["Supplier.DuplicatePhone"] = ("رقم الهاتف مستخدم بالفعل", "Phone number already in use"),
        };
    }
}