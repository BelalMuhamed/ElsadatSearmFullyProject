using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class LeaveType : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // اسم نوع الإجازة (مثل: إجازة سنوية، إجازة مرضية، إجازة عارضة)
        public bool IsPaid { get; set; } // هل هي مدفوعة الأجر؟
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}
