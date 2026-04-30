using Domain.Entities.Commonitems;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Finance
{
    public class ChartOfAccounts : BaseEntity
    {
        public string AccountCode { get; set; } = default!;
        public string? UserId { get; set; }
        public string AccountName { get; set; } = default!;
        public AccountTypes Type { get; set; }
        public int? ParentAccountId { get; set; }
        public bool IsLeaf { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// True when this account is seeded by the system and is referenced by
        /// internal workflows (payroll, sales, purchases). Such accounts must
        /// never be edited (name/code/parent/type) or deleted.
        /// </summary>
        public bool IsSystemAccount { get; set; }

        /// <summary>
        /// Stable, code-level identifier used by services to resolve accounts
        /// without depending on AccountCode strings. Null for user-created accounts.
        /// </summary>
        public SystemAccountCode? SystemCode { get; set; }
    }
}
