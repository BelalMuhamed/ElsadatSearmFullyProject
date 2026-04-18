using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Invoices
{
    public class CompanyExpensesInvoices : BaseInvoice
    {
        public string Notes { get; set; } = string.Empty;
        
    }
}
