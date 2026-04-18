using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Users
{
    public class Distributor_Merchant_Agent
    {
        public int Balance { get; set; }
        [Column(TypeName = "money")]
        public decimal? CashBalance { get; set; }
        [Column(TypeName = "money")]
        public decimal? Indebtedness { get; set; }
        public decimal? FirstSpecialDiscount { get; set; }
        public decimal? SecondSpecialDiscount { get; set; }
        public decimal? ThirdSpecialDiscount { get; set; }

        public DistributorOrMerchantOrAgent Type { get; set; }

        //----------- Obj From User and ForeignKey UserID ---------------------------------
        [Key]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }

    }
}
