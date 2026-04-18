using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helper;
public class RepresentativeAttendanceHelper
{
    public string? RepresentativeCode { get; set; }
    public string? RepresentativeId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? InputTime { get; set; }
    public DateOnly? SelectedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? AttendanceStatusID { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeEmail { get; set; }
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public string CheckInLocation { get; set; } = string.Empty;
}

