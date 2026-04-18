using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums;
public enum LoanStatus
{
    PendingApproval = 0,  // قيد الموافقة
    Active = 1,           // نشط (تمت الموافقة)
    PaidOff = 2,          // مسدد بالكامل
    Rejected = 3,         // مرفوض
    Overdue = 4,          // متأخر في السداد
    Defaulted = 5         // متعثر
}
