using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SalaryAdjustment
{
    public class SalaryAdjustmentDto
    {
        public string EmployeeCode { get; set; } = string.Empty;  // كود الموظف
        [Column(TypeName = "money")]
        public decimal AdjustmentAmount { get; set; }  // قيمة التعديل على الراتب
        public AdjustmentType AdjustmentType { get; set; }  // نوع التعديل
        public DateTime AdjustmentDate { get; set; }  // تاريخ التعديل
        public string ApprovedBy { get; set; } = string.Empty;  // من وافق على التعديل
        public DateTime? ApprovedAt { get; set; }  // تاريخ الموافقة
        public string? Notes { get; set; }  // ملاحظات إضافية
    }
}
