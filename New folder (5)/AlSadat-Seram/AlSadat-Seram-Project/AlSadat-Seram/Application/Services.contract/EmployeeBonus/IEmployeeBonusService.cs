using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeBonus;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.EmployeeBonus
{
    public interface IEmployeeBonusService
    {
        Task<Result<string>> AddEmployeeBonusAsync(EmployeeBonusDto dto);
        Task<Result<string>> UpdateEmployeeBonusAsync(EmployeeBonusDto dto);
        Task<Result<string>> DeleteEmployeeBonusAsync(int id);
        Task<Result<EmployeeBonusDto>> GetEmployeeBonusByIdAsync(int id);
        Task<PagedList<EmployeeBonusDto>> GetEmployeeBonusesAsync(PaginationParams paginationParams);
        Task<Result<string>> SoftDeleteEmployeeBonusAsync(int id);
        Task<Result<string>> RestoreEmployeeBonusAsync(int id);
    }

}
