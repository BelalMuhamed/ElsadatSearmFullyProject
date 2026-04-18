using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class EmployeeAttendance : BaseAttendance
    {
        [ForeignKey(nameof(Employee))]
        public string EmployeeCode { get; set; } = string.Empty;
        public virtual Employee Employee { get; set; }
    }
}
