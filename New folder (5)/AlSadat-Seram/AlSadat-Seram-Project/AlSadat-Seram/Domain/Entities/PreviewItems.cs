using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PreviewItems : BaseEntity
    {
        public int Quantity { get; set; }
        //----------- Obj From Products and ForeignKey ProductsId ---------------------------------
        [ForeignKey(nameof(Product))]
        public int? ProductId { get; set; }
        public virtual Products? Product { get; set; }
        //----------- Obj From Previews and ForeignKey PreviewId ---------------------------------
        [ForeignKey(nameof(Preview))]
        public int PreviewId { get; set; }
        public virtual Previews? Preview { get; set; }
    }
}
