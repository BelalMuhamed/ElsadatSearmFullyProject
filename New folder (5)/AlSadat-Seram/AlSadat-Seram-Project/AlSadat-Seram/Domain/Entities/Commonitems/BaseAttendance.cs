
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Commonitems
{
    public class BaseAttendance : BaseEntity
    {
        public DateOnly? AttendanceDate { get; set; }
        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; } 
        public CheckInMethod CheckInMethod { get; set; } = CheckInMethod.FingerPrint;
        public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Present;
        public bool IsDeleted { get; set; }     
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}
