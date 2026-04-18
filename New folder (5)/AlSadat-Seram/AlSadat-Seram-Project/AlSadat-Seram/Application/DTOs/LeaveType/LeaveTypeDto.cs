using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.LeaveType
{
    public class LeaveTypeDto
    {
        public int Id { get; set; }  // معرّف نوع الإجازة
        [Required(ErrorMessage = "اسم نوع الإجازة مطلوب")]
        [StringLength(150,ErrorMessage = "الاسم يجب ألا يتعدى 150 حرف")]
        public string Name { get; set; } = string.Empty;  // اسم نوع الإجازة (مثل: إجازة سنوية، إجازة مرضية)
        public bool IsPaid { get; set; }  // هل هي مدفوعة الأجر؟
        public DateTime CreatedAt { get; set; }  // تاريخ الإنشاء
        public string CreatedBy { get; set; } = string.Empty;  // الشخص الذي أنشأ هذا النوع
        public string? UpdatedBy { get; set; }  // الشخص الذي قام بالتحديث الأخير
        public DateTime? UpdatedAt { get; set; }  // تاريخ آخر تحديث
    }

}
