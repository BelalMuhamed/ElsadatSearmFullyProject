using Domain.Entities.Commonitems;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Finance
{
    public class ChartOfAccounts:BaseEntity
    {
        public string AccountCode { get; set; }
        public string? UserId { get; set; }
        public string AccountName { get; set; }
        public AccountTypes Type { get; set; }
        public int? ParentAccountId { get; set; }
        public bool IsLeaf { get; set; }
    
        public bool IsActive { get; set; }

    }
}
