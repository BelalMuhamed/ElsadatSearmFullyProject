using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Invoices
{
    public class Billdiscounts
    {
        public int Id { get; set; }
        public float? FirstDiscount { get; set; }
        public float? SecondDiscount { get; set; }
        public float? ThirdDiscount { get; set; }
    }
}
