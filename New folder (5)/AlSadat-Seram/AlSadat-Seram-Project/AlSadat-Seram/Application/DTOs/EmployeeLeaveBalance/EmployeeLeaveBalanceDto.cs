namespace Application.DTOs.EmployeeLeaveBalance
{
    public class EmployeeLeaveBalanceDto
    {
        public string EmployeeCode { get; set; } = string.Empty; // كود الموظف
        public string EmployeeName { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; } // نوع الإجازة
        public string LeaveTypeName { get; set; } = string.Empty;
        public int Year { get; set; } // السنة
        public decimal OpeningBalance { get; set; } // الرصيد الافتتاحي
        public decimal Accrued { get; set; } // الرصيد المكتسب
        public decimal Used { get; set; } // الرصيد المستخدم
        public decimal Remaining { get; set; } // الرصيد المتبقي
        public decimal PendingRequests { get; set; }
        public decimal AvailableBalance => Remaining - PendingRequests;
    }
    //---------------------------------------------------------
    public class LeaveBalanceSummaryDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public List<LeaveTypeBalanceDto> Balances { get; set; } = new();
        public decimal TotalRemaining => Balances.Sum(b => b.Remaining);
        public decimal TotalUsed => Balances.Sum(b => b.Used);
        public decimal TotalPending => Balances.Sum(b => b.Pending);
    }
    //-----------------------------------------------------------
    public class LeaveTypeBalanceDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Accrued { get; set; }
        public decimal Used { get; set; }
        public decimal Remaining { get; set; }
        public decimal Pending { get; set; }
    }
    //------------------------------------------------------------
    public class BulkLeaveBalanceRequestDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public int Year { get; set; }
        public List<LeaveBalanceItemDto> Balances { get; set; } = new();
    }
    //------------------------------------------------------------
    public class LeaveBalanceItemDto
    {
        public int LeaveTypeId { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal? Accrued { get; set; }
    }
    //------------------------------------------------------------
    public class BulkLeaveBalanceResultDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public List<LeaveBalanceDetailDto> CreatedBalances { get; set; } = new();
        public List<FailedBalanceDto> FailedBalances { get; set; } = new();
        public int TotalCreated => CreatedBalances.Count;
        public int TotalFailed => FailedBalances.Count;
    }
    //------------------------------------------------------------
    public class LeaveBalanceDetailDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal Accrued { get; set; }
        public decimal Remaining { get; set; }
    }
    //------------------------------------------------------------
    public class FailedBalanceDto
    {
        public int LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
    //------------------------------------------------------------
    public class SetCustomLeaveBalanceRequest
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public int OpeningBalance { get; set; }
    }
    //------------------------------------------------------------
    public class InitializeLeaveBalanceRequest
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public int Year { get; set; }
    }

}
