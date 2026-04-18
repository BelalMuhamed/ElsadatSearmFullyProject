using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR;
public class EmployeeLoanDeduction
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public int LoanId { get; set; }
    public string? EmployeeCode { get; set; } 
    public string? RepresentativeCode { get; set; } 
    public DateTime DeductionDate { get; set; }
    public decimal DeductionAmount { get; set; }
    public string CreateBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}