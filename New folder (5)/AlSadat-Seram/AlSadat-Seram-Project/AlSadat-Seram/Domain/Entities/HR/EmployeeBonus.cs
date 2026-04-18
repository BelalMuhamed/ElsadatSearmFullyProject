using Domain.Entities.Commonitems;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class EmployeeBonus : BaseEntity
    {
        public string EmployeeCode { get; set; } = string.Empty;  // كود الموظف
        public decimal BonusAmount { get; set; }  // قيمة المكافأة
        public BonusType BonusType { get; set; }  // نوع المكافأة (شهري، سنوي، مبيعات، الخ.)
        public DateTime BonusDate { get; set; }  // تاريخ المكافأة
        public string ApprovedBy { get; set; } = string.Empty;  // من وافق على المكافأة
        public DateTime? ApprovedAt { get; set; }  // تاريخ الموافقة على المكافأة
        public string? Notes { get; set; }  // ملاحظات إضافية حول المكافأة
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }

}
