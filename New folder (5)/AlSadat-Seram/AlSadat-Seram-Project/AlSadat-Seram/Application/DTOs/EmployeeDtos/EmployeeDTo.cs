using Domain.Enums;

namespace Application.DTOs.EmployeeSalary;
public class EmployeeDTo
{
    public string? UserId { get; set; }
    public string Email { get; set; }=string.Empty;
    public string Password { get; set; } = string.Empty;  
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public Gender Gender { get; set; }
    public string? CreateBy { get; set; }
    public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
    public string? UpdateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeleteBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public int CityID { get; set; }
    public string? CityName { get; set; } 
    public string? EmployeeCode { get; set; }
    public int? AccountNumber { get; set; }
    public string? AccountName { get; set; } 
    public string? SNO { get; set; } 
    public decimal OvertimeRatePerHour { get; set; }
    public decimal DeductionRatePerHour { get; set; }
    public int? DepartmentID { get; set; }
    public string? DepartmentName { get; set; } 
    public DateOnly BirthDate { get; set; }
    public DateOnly HireDate { get; set; }= DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal Salary { get; set; }
    public TimeOnly TimeIn { get; set; }
    public TimeOnly TimeOut { get; set; }
    public WeekDays WeekHoliday1 { get; set; }
    public WeekDays? WeekHoliday2 { get; set; }
    public string? NameOfCreatedBy { get; set; }
    public string? RoleName { get; set; }
    public string? RoleId { get; set; }
    public List<string> RolesName { get; set; } = new List<string>();
    public List<string> RolesId { get; set; } = new List<string>();
}
