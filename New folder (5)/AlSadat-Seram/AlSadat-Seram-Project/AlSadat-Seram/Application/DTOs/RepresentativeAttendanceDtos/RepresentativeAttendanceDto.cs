using Domain.Enums;

namespace Application.DTOs.RepresentativeAttendanceDtos;
public class RepresentativeAttendanceDto
{
    public int Id { get; set; }
    public string RepresentativeCode { get; set; } = string.Empty;
    public string RepresentativeName { get; set; } = string.Empty;
    public string RepresentativeId { get; set; } = string.Empty;
    public DateOnly AttendanceDate { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public CheckInMethod CheckInMethod { get; set; } = CheckInMethod.MobileApp;
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Present;
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public string CheckInLocation { get; set; } = string.Empty;
}
