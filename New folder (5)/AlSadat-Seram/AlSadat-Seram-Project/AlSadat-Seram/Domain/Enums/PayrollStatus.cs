using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums;
public enum PayrollStatus
{
    Created = 1,        // تم الإنشاء (محاسب يراجع)
    Approved = 2,       // تمت الموافقة
    Paid = 3,           // تم الدفع
    Rejected = 4,       // مرفوض
    Cancelled = 5
}
