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
    public class RepresentativeCashTransactions : BaseEntity
    {
        [Column(TypeName = "money")]
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool CompanyAccepted { get; set; }=true;

        //------------- Obj From User and ForeignKey RepresentativeID ----------------
        [ForeignKey(nameof(Representative))]
        public string RepresentativeID { get; set; } = string.Empty;
        public virtual ApplicationUser? Representative { get; set; }
        //----------- Obj From Previews and ForeignKey PreviewId ---------------------------------
        [ForeignKey(nameof(Preview))]
        public int PreviewId { get; set; }
        public virtual Previews Preview { get; set; }
    }
}
