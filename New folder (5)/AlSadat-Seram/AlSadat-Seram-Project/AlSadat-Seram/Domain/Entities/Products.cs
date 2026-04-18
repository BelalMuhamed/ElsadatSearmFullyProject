using Domain.Entities.Commonitems;
using Domain.Entities.Invoices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Products : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "money")]
        public decimal SellingPrice { get; set; }
        public int PointPerUnit { get; set; }
        public string productCode { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
        public int TheSmallestPossibleQuantity { get; set; }
     

        //----------- Obj From Categories and ForeignKey CategoryID ---------------------------------
    
        //------------- ICollection From PurchaseInvoiceItems ------------------------------
        public virtual ICollection<PurchaseInvoiceItems> PurchaseInvoiceItems { get; set; } = [];
        //------------- ICollection From SalesInvoiceItems ------------------------------
        public virtual ICollection<SalesInvoiceItems> SalesInvoiceItems { get; set; } = [];
    }

}
