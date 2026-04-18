using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums;
public enum PaymentMethod
{
    SalaryDeduction,  // خصم من الراتب
    Cash,             // نقدي
    BankTransfer,     // تحويل بنكي
    Check
}
