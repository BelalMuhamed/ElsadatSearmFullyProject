using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
   public  enum ReferenceType
    {
        PurchaseInvoice = 1,
        SalesInvoice = 2,
        Payment = 3,
        Receipt = 4,
        Adjustment = 5
    }
}
