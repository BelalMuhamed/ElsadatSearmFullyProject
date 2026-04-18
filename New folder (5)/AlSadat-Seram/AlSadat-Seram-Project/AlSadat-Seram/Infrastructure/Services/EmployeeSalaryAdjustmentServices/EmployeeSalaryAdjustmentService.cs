using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.SalaryAdjustment;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.EmployeeSalaryAdjustment;
using Domain.Common;
using Domain.Entities.HR;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.EmployeeSalaryAdjustmentServices
{
    public class EmployeeSalaryAdjustmentService : IEmployeeSalaryAdjustmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public EmployeeSalaryAdjustmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> AddSalaryAdjustmentAsync(SalaryAdjustmentDto dto)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Result<string>.Failure("Unauthorized");

            var adjustment = new SalaryAdjustment
            {
                EmployeeCode = dto.EmployeeCode,
                AdjustmentAmount = dto.AdjustmentAmount,
                AdjustmentType = dto.AdjustmentType,
                AdjustmentDate = dto.AdjustmentDate,
                ApprovedBy = dto.ApprovedBy,
                ApprovedAt = dto.ApprovedAt,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<SalaryAdjustment, int>().AddAsync(adjustment);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Salary Adjustment added successfully.");
        }

        public async Task<Result<string>> UpdateSalaryAdjustmentAsync(SalaryAdjustmentDto dto)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Result<string>.Failure("Unauthorized");

            var adjustment = await _unitOfWork.GetRepository<SalaryAdjustment, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(a => a.EmployeeCode == dto.EmployeeCode && a.AdjustmentDate == dto.AdjustmentDate);

            if (adjustment == null)
                return Result<string>.Failure("Adjustment not found.");

            adjustment.AdjustmentAmount = dto.AdjustmentAmount;
            adjustment.AdjustmentType = dto.AdjustmentType;
            adjustment.ApprovedBy = dto.ApprovedBy;
            adjustment.ApprovedAt = dto.ApprovedAt;
            adjustment.Notes = dto.Notes;
            adjustment.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.GetRepository<SalaryAdjustment, int>().UpdateAsync(adjustment);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Salary Adjustment updated successfully.");
        }

        public async Task<Result<string>> DeleteSalaryAdjustmentAsync(int id)
        {
            var adjustment = await _unitOfWork.GetRepository<SalaryAdjustment, int>().GetByIdAsync(id);
            if (adjustment == null)
                return Result<string>.Failure("Adjustment not found.");

            await _unitOfWork.GetRepository<SalaryAdjustment, int>().DeleteAsync(adjustment);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Salary Adjustment deleted successfully.");
        }

        public async Task<Result<SalaryAdjustmentDto>> GetSalaryAdjustmentByIdAsync(int id)
        {
            var adjustment = await _unitOfWork.GetRepository<SalaryAdjustment, int>().GetByIdAsync(id);
            if (adjustment == null)
                return Result<SalaryAdjustmentDto>.Failure("Adjustment not found.");

            var dto = new SalaryAdjustmentDto
            {
                EmployeeCode = adjustment.EmployeeCode,
                AdjustmentAmount = adjustment.AdjustmentAmount,
                AdjustmentType = adjustment.AdjustmentType,
                AdjustmentDate = adjustment.AdjustmentDate,
                ApprovedBy = adjustment.ApprovedBy,
                ApprovedAt = adjustment.ApprovedAt,
                Notes = adjustment.Notes
            };

            return Result<SalaryAdjustmentDto>.Success(dto);
        }

        public async Task<PagedList<SalaryAdjustmentDto>> GetSalaryAdjustmentsAsync(PaginationParams paginationParams)
        {
            var query = _unitOfWork.GetRepository<SalaryAdjustment, int>().GetQueryable().AsNoTracking();
            var result = query.Select(a => new SalaryAdjustmentDto
            {
                EmployeeCode = a.EmployeeCode,
                AdjustmentAmount = a.AdjustmentAmount,
                AdjustmentType = a.AdjustmentType,
                AdjustmentDate = a.AdjustmentDate,
                ApprovedBy = a.ApprovedBy,
                ApprovedAt = a.ApprovedAt,
                Notes = a.Notes
            });

            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        public async Task<Result<string>> SoftDeleteEmployeeSalaryAdjustmentAsync(int id)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized user.", HttpStatusCode.Unauthorized);

            var record = await _unitOfWork.GetRepository<SalaryAdjustment, int>()
                .GetByIdAsync(id);
            if (record == null)
                return Result<string>.Failure("Salary adjustment not found.", HttpStatusCode.NotFound);

            record.IsDeleted = true;
            record.DeleteBy = userId;
            record.DeleteAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return Result<string>.Success("Salary adjustment deleted successfully.");
        }

        public async Task<Result<string>> RestoreEmployeeSalaryAdjustmentAsync(int id)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized user.", HttpStatusCode.Unauthorized);

            var record = await _unitOfWork.GetRepository<SalaryAdjustment, int>()
                .GetByIdAsync(id);
            if (record == null)
                return Result<string>.Failure("Salary adjustment not found.", HttpStatusCode.NotFound);

            record.IsDeleted = false;
            record.UpdateBy = userId;
            record.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return Result<string>.Success("Salary adjustment restored successfully.");
        }
    }
}
