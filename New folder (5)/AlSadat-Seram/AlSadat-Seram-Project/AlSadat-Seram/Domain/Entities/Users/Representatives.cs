using Domain.Entities.Commonitems;
using Domain.Entities.HR;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Users
{
    public class Representatives : BaseStaff
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? RepresentativesCode { get; set; }
        public string SNO { get; set; } = string.Empty;
        public int PointsWallet { get; set; }

        [Column(TypeName = "money")]
        public decimal MoneyDeposit { get; set; }
        public RepresentiveType RepresentiveType { get; set; } = RepresentiveType.Mixed;
        public decimal OvertimeRatePerHour { get; set; }

        // Navigation property to Attendance
        public virtual ICollection<RepresentativeAttendance> RepresentativeAttendance { get; set; } = new List<RepresentativeAttendance>();
        //----------- ICollection From SpecialRepresentiveCity  ---------------------------------
        public virtual ICollection<SpecialRepresentiveCity> SpecialRepresentiveCities { get; set; } = new List<SpecialRepresentiveCity>();
    }
}
