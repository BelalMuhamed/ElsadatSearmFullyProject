using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.DepartmentService;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.HR;
using Domain.UnitOfWork.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.DepartmentServices
{
    internal class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ICurrentUserService _CurrentUserService;
        public DepartmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _UnitOfWork = unitOfWork;
            _CurrentUserService = currentUserService;
        }
        //------------------------------------------------------------------------
        public async Task<PagedList<Department>> GetAllDepartment(PaginationParams paginationParams , string? search)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<Department>(new List<Department>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            var result = _UnitOfWork.GetRepository<Department,int>().GetQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(c => c.Name.Contains(search));
            }
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------
        public async Task<PagedList<Department>> GetAllActiveDepartment(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<Department>(new List<Department>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            var result = _UnitOfWork.GetRepository<Department,int>().GetQueryable().Where(c=>!c.IsDeleted);
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------
        public async Task<Result<Department>> GetDepartmentById(int id)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<Department>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            var result =await _UnitOfWork.GetRepository<Department,int>().GetByIdAsync(id);
            if (result is null)
                return Result<Department>.Failure("Department Not Found.", HttpStatusCode.NotFound);
            return Result<Department>.Success(result, HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------
        public async Task<PagedList<Department>> GetSoftDeleteDepartment(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<Department>(new List<Department>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            var result = _UnitOfWork.GetRepository<Department,int>().GetQueryable().Where(c=>c.IsDeleted);
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------
        public async Task<Result<string>> CreateDepartment(Department model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (model == null)
                return Result<string>.Failure("Invalid Department data.");
            model.CreateBy= userId;
            model.CreatedAt= DateTime.UtcNow;
            await _UnitOfWork.GetRepository<Department,int>().AddAsync(model);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Department Created Successfully.", HttpStatusCode.Created);
        }
        //------------------------------------------------------------------------
        public async Task<Result<string>> UpdateDepartment(Department model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (model == null)
                return Result<string>.Failure("Invalid Department data.");
            var existingDepartment = await _UnitOfWork.GetRepository<Department,int>().GetByIdAsync(model.Id);
            if (existingDepartment == null)
                return Result<string>.Failure("Department Not Found.", HttpStatusCode.NotFound);
            existingDepartment.Name = model.Name;
            existingDepartment.UpdateBy= userId;
            existingDepartment.UpdateAt= DateTime.UtcNow;
            await _UnitOfWork.GetRepository<Department,int>().UpdateAsync(existingDepartment);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Department Updated Successfully.", HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------
        public async Task<Result<string>> SoftDeleteDepartment(Department model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (model == null)
                return Result<string>.Failure("Invalid Department data.");
            var existingDepartment = await _UnitOfWork.GetRepository<Department, int>().GetByIdAsync(model.Id);
            if (existingDepartment == null)
                return Result<string>.Failure("Department Not Found.", HttpStatusCode.NotFound);
            existingDepartment.IsDeleted= true;
            existingDepartment.DeleteBy= userId;
            existingDepartment.DeleteAt= DateTime.UtcNow;
            await _UnitOfWork.GetRepository<Department, int>().UpdateAsync(existingDepartment);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Department Soft Deleted Successfully.", HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------
        public async Task<Result<string>> RestoreDepartment(Department model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (model == null)
                return Result<string>.Failure("Invalid Department data.");
            var existingDepartment = await _UnitOfWork.GetRepository<Department, int>().GetByIdAsync(model.Id);
            if (existingDepartment == null)
                return Result<string>.Failure("Department Not Found.", HttpStatusCode.NotFound);
            existingDepartment.IsDeleted= false;
            existingDepartment.DeleteBy= null;
            existingDepartment.DeleteAt= null;
            existingDepartment.UpdateBy= userId;
            existingDepartment.UpdateAt= DateTime.UtcNow;
            await _UnitOfWork.GetRepository<Department, int>().UpdateAsync(existingDepartment);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Department Restored Successfully.", HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------
        public async Task<Result<string>> HardDeleteDepartment(Department model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (model == null)
                return Result<string>.Failure("Invalid Department data.");
            var existingDepartment = await _UnitOfWork.GetRepository<Department, int>().GetByIdAsync(model.Id);
            if (existingDepartment == null)
                return Result<string>.Failure("Department Not Found.", HttpStatusCode.NotFound);
            await _UnitOfWork.GetRepository<Department, int>().DeleteAsync(existingDepartment);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Department Hard Deleted Successfully.", HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------
    }
}
