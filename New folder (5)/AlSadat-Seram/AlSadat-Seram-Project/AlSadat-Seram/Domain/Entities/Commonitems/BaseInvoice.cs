using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Commonitems
{
    public class BaseInvoice : BaseEntity
    {
        public PurchaseInvoivceDeleteStatus? DeleteStatus { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateAt { get; set; }= DateTime.UtcNow;
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }

        [Column(TypeName = "money")]
        public decimal? TotalGrowthAmount { get; set; }
        public decimal? TotalNetAmount { get; set; }

    }
}