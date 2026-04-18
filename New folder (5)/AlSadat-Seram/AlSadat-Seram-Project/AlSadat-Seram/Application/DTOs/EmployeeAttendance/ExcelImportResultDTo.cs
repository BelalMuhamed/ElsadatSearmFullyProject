using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.EmployeeAttendance;
public class ExcelImportResultDTo
{
    public List<string> SuccessMessages { get; set; } = new();
    public List<ExcelErrorRecord> ErrorRecords { get; set; } = new();
}

public class ExcelErrorRecord
{
    public int RowNumber { get; set; }
    public string? EmployeeCode { get; set; }
    public string ErrorMessage { get; set; } = "";
}
