using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Transactions
{
    public class PointTransactions : BaseEntity
    {
        public int TotalPoints { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //------------- Obj From User and ForeignKey SenderId ----------------
        [ForeignKey(nameof(Sender))]

        public string SenderId { get; set; } = string.Empty;
        public virtual ApplicationUser? Sender { get; set; }

        //------------- Obj From User and ForeignKey ReceverId ----------------
        [ForeignKey(nameof(Recever))]
        public string ReceverId { get; set; } = string.Empty;
        [InverseProperty("ReceverTransactions")]
        public virtual ApplicationUser? Recever { get; set; }
        //----------- Obj From Previews and ForeignKey PreviewId ---------------------------------
        [ForeignKey(nameof(Preview))]
        public int? PreviewId { get; set; }
        public virtual Previews? Preview { get; set; }
    }
}
