using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class PublicHoliday : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}
