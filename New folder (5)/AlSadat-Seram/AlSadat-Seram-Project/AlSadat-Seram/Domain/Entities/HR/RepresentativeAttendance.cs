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
    public class RepresentativeAttendance : BaseAttendance
    {
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public string CheckInLocation { get; set; } = string.Empty;

        //----------- Obj From Representatives and ForeignKey RepresentativeCode ---------------------------------
        [ForeignKey(nameof(Representatives))]
        public string RepresentativeCode { get; set; } = string.Empty;
        public virtual Representatives? Representatives { get; set; }
    }
}
