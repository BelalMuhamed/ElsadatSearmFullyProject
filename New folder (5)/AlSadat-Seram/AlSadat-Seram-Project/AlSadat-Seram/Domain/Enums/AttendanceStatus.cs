using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        Excused = 4,
        Vacation = 5,
        SickLeave = 6,
        EarlyLeave = 7
    }
}
