using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.FinanceDtos
{
    public class FilterationAccountsDto
    {
        public string? accountCode { get; set; }
        public string? accountName { get; set; }
        public int? type { get; set; }
        public int? parentAccountId { get; set; }
        public bool? isLeaf { get; set; }
        public bool? isActive { get; set; }
        public int? page { get; set; }
        public int? pageSize { get; set; }

    }
}
