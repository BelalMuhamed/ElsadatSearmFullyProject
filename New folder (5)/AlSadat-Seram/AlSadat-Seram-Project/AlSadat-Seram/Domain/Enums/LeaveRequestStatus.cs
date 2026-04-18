using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum LeaveRequestStatus
    {
        Pending = 1,    // معلق
        Approved = 2,   // معتمد
        Rejected = 3,   // مرفوض
        Cancelled = 4
    }
}
