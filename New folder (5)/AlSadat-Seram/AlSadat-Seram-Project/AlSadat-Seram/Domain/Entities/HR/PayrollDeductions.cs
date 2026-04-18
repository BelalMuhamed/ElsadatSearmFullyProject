using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class PayrollDeductions : BaseEntity
    {
        public string? EmployeeCode { get; set; }   // كود الموظف
        public string? RepresentativeCode { get; set; }  // كود المندوب
        public DateTime DeductionDate { get; set; }  // تاريخ الخصم
        //[Column(TypeName = "money")]
        public decimal DeductionAmount { get; set; }  // قيمة الخصم
        [Column(TypeName = "money")]
        public decimal MoneyAmount { get; set; }  // قيمه المبلغ المخصوم 
        public string DeductionReason { get; set; } = string.Empty;  // سبب الخصم
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }

}
