using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.DepartmentService
{
    public interface IDepartmentService
    {
        Task<PagedList<Department>> GetAllDepartment(PaginationParams paginationParams,string? search);
        Task<PagedList<Department>> GetAllActiveDepartment(PaginationParams paginationParams);
        Task<Result<Department>> GetDepartmentById(int id);
        Task<PagedList<Department>> GetSoftDeleteDepartment(PaginationParams paginationParams);
        Task<Result<string>> CreateDepartment(Department model);
        Task<Result<string>> UpdateDepartment(Department model);
        Task<Result<string>> SoftDeleteDepartment(Department model);
        Task<Result<string>> RestoreDepartment(Department model);
        Task<Result<string>> HardDeleteDepartment(Department model);
    }
}
