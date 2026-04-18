using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.EmployeeLeaveRequest
{
    public class EmployeeLeaveRequestDto
    {
        public int Id { get; set; }

        //[Required(ErrorMessage = "كود الموظف مطلوب")]
        public string? EmployeeCode { get; set; } 
        public string? EmployeeName { get; set; } 
        public string? RepresentativeCode { get; set; }
        public string? RepresentativeName { get; set; } 

        [Required(ErrorMessage = "نوع الإجازة مطلوب")]
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        public DateTime FromDate { get; set; } // من تاريخ
        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        public DateTime ToDate { get; set; } // إلى تاريخ

        public decimal DaysRequested { get; set; } // عدد الأيام المطلوبة
        public LeaveRequestStatus Status { get; set; } // حالة الطلب
        public string StatusName => Status.ToString();

        public string ApprovedBy { get; set; } = string.Empty; // من قام بالموافقة
        public DateTime? ApprovedAt { get; set; } // تاريخ الموافقة
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        [StringLength(500,ErrorMessage = "الملاحظات يجب ألا تتعدى 500 حرف")]
        public string? Notes { get; set; } // ملاحظات إضافية

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
    //------------------------------------------------------------------------------------
    public class ApproveRejectLeaveDto
    {
        [Required(ErrorMessage = "معرف طلب الإجازة مطلوب")]
        public int LeaveRequestId { get; set; }

        //[Required(ErrorMessage = "سبب الإجراء مطلوب")]
        [StringLength(500,ErrorMessage = "السبب يجب ألا يتعدى 500 حرف")]
        public string? Reason { get; set; }
    }
    //----------------------------------------------------------------------
    public class LeaveRequestFilterDto
    {
        public string? EmployeeCode { get; set; }
        public  string? RepresentativeCode { get; set; }
        public int? LeaveTypeId { get; set; }
        public LeaveRequestStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool SortDescending { get; set; } = true;
    }
    //----------------------------------------------------------------------
    public class CreateLeaveRequestDto
    {
        //[Required(ErrorMessage = "كود الموظف مطلوب")]
        public string? EmployeeCode { get; set; } 
        public string? RepresentativeCode { get; set; }

        [Required(ErrorMessage = "نوع الإجازة مطلوب")]
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        public DateTime ToDate { get; set; }

        [StringLength(500,ErrorMessage = "الملاحظات يجب ألا تتعدى 500 حرف")]
        public string? Notes { get; set; }

        public bool IsEmergency { get; set; }
        public string? ContactDuringLeave { get; set; }
        public string EmployeeEmail { get; set; } = string.Empty;
    }

    //---------------------------------------------------------------------------------
    public class LeaveReportDto
    {

        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int TotalLeaveDays { get; set; }
        public int UsedLeaveDays { get; set; }
        public int RemainingLeaveDays { get; set; }
        public List<LeaveDetailDto> LeaveDetails { get; set; } = new();
    }

    //---------------------------------------------------------------------
    public class LeaveDetailDto
    {
        public string LeaveType { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
        public DateTime LastLeaveDate { get; set; }
    }
  
    //-------------------------------------------------------
    public class LeaveReportFilterDto
    {
        public string? Department { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Year { get; set; }
        public int? LeaveTypeId { get; set; }
    }

}
