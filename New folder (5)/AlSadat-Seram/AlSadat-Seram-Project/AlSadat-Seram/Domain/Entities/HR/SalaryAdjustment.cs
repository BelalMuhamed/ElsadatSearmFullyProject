using Domain.Entities.Commonitems;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class SalaryAdjustment : BaseEntity
    {
        public string EmployeeCode { get; set; } = string.Empty;  // كود الموظف
        [Column(TypeName = "money")]
        public decimal AdjustmentAmount { get; set; }  // قيمة التعديل على الراتب
        public AdjustmentType AdjustmentType { get; set; }  // نوع التعديل (زيادة سنوية، زيادة أداء، إلخ)
        public DateTime AdjustmentDate { get; set; }  // تاريخ التعديل
        public string ApprovedBy { get; set; } = string.Empty;  // من وافق على التعديل
        public DateTime? ApprovedAt { get; set; }  // تاريخ الموافقة
        public string? Notes { get; set; }  // ملاحظات إضافية
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }

    }
}
