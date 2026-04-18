using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.EmployeeBonus
{
    public class EmployeeBonusDto
    {
        public string EmployeeCode { get; set; } = string.Empty; // كود الموظف
        public decimal BonusAmount { get; set; } // قيمة المكافأة
        public BonusType BonusType { get; set; } // نوع المكافأة
        public DateTime BonusDate { get; set; } // تاريخ المكافأة
        public string ApprovedBy { get; set; } = string.Empty; // من وافق على المكافأة
        public DateTime? ApprovedAt { get; set; } // تاريخ الموافقة
        public string? Notes { get; set; } // ملاحظات إضافية
    }


}
