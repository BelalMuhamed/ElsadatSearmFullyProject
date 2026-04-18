using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.SalaryAdjustment;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.EmployeeSalaryAdjustment
{
    public interface IEmployeeSalaryAdjustmentService //  التغيرات علي رواتب الموظفين
    {
        Task<Result<string>> AddSalaryAdjustmentAsync(SalaryAdjustmentDto dto);
        Task<Result<string>> UpdateSalaryAdjustmentAsync(SalaryAdjustmentDto dto);
        Task<Result<string>> DeleteSalaryAdjustmentAsync(int id);
        Task<Result<SalaryAdjustmentDto>> GetSalaryAdjustmentByIdAsync(int id);
        Task<PagedList<SalaryAdjustmentDto>> GetSalaryAdjustmentsAsync(PaginationParams paginationParams);
        Task<Result<string>> SoftDeleteEmployeeSalaryAdjustmentAsync(int id);
        Task<Result<string>> RestoreEmployeeSalaryAdjustmentAsync(int id);
    }

}
