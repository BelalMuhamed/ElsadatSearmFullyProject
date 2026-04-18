using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeBonus;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.EmployeeBonus;
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

namespace Infrastructure.Services.EmployeeBonusServices
{
    public class EmployeeBonusService : IEmployeeBonusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public EmployeeBonusService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> AddEmployeeBonusAsync(EmployeeBonusDto dto)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Result<string>.Failure("Unauthorized");

            var bonus = new EmployeeBonus
            {
                EmployeeCode = dto.EmployeeCode,
                BonusAmount = dto.BonusAmount,
                BonusType = dto.BonusType,
                BonusDate = dto.BonusDate,
                ApprovedBy = dto.ApprovedBy,
                ApprovedAt = dto.ApprovedAt,
                Notes = dto.Notes,      
            };

            await _unitOfWork.GetRepository<EmployeeBonus,int>().AddAsync(bonus);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Employee Bonus added successfully.");
        }

        public async Task<Result<string>> UpdateEmployeeBonusAsync(EmployeeBonusDto dto)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Result<string>.Failure("Unauthorized");

            var bonus = await _unitOfWork.GetRepository<EmployeeBonus, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(b => b.EmployeeCode == dto.EmployeeCode && b.BonusDate == dto.BonusDate);

            if (bonus == null)
                return Result<string>.Failure("Bonus not found.");

            bonus.BonusAmount = dto.BonusAmount;
            bonus.BonusType = dto.BonusType;
            bonus.ApprovedBy = dto.ApprovedBy;
            bonus.ApprovedAt = dto.ApprovedAt;
            bonus.Notes = dto.Notes;
            bonus.UpdateAt = DateTime.UtcNow;
            bonus.UpdateBy = userId;

            await _unitOfWork.GetRepository<EmployeeBonus, int>().UpdateAsync(bonus);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Employee Bonus updated successfully.");
        }

        public async Task<Result<string>> DeleteEmployeeBonusAsync(int id)
        {
            var bonus = await _unitOfWork.GetRepository<EmployeeBonus, int>().GetByIdAsync(id);
            if (bonus == null)
                return Result<string>.Failure("Bonus not found.");

            await _unitOfWork.GetRepository<EmployeeBonus, int>().DeleteAsync(bonus);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Employee Bonus deleted successfully.");
        }

        public async Task<Result<EmployeeBonusDto>> GetEmployeeBonusByIdAsync(int id)
        {
            var bonus = await _unitOfWork.GetRepository<EmployeeBonus, int>().GetByIdAsync(id);
            if (bonus == null)
                return Result<EmployeeBonusDto>.Failure("Bonus not found.");

            var dto = new EmployeeBonusDto
            {
                EmployeeCode = bonus.EmployeeCode,
                BonusAmount = bonus.BonusAmount,
                BonusType = bonus.BonusType,
                BonusDate = bonus.BonusDate,
                ApprovedBy = bonus.ApprovedBy,
                ApprovedAt = bonus.ApprovedAt,
                Notes = bonus.Notes
            };

            return Result<EmployeeBonusDto>.Success(dto);
        }

        public async Task<PagedList<EmployeeBonusDto>> GetEmployeeBonusesAsync(PaginationParams paginationParams)
        {
            var query = _unitOfWork.GetRepository<EmployeeBonus,int>().GetQueryable().AsNoTracking();
            var result = query.Select(b => new EmployeeBonusDto
            {
                EmployeeCode = b.EmployeeCode,
                BonusAmount = b.BonusAmount,
                BonusType = b.BonusType,
                BonusDate = b.BonusDate,
                ApprovedBy = b.ApprovedBy,
                ApprovedAt = b.ApprovedAt,
                Notes = b.Notes
            });

            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        public async Task<Result<string>> SoftDeleteEmployeeBonusAsync(int id)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized user.", HttpStatusCode.Unauthorized);

            var bonus = await _unitOfWork.GetRepository<EmployeeBonus, int>()
                .GetByIdAsync(id);
            if (bonus == null)
                return Result<string>.Failure("Bonus not found.", HttpStatusCode.NotFound);

            bonus.IsDeleted = true;
            bonus.DeleteBy = userId;
            bonus.DeleteAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return Result<string>.Success("Bonus deleted successfully.");
        }

        public async Task<Result<string>> RestoreEmployeeBonusAsync(int id)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized user.", HttpStatusCode.Unauthorized);

            var bonus = await _unitOfWork.GetRepository<EmployeeBonus, int>()
                .GetByIdAsync(id);
            if (bonus == null)
                return Result<string>.Failure("Bonus not found.", HttpStatusCode.NotFound);

            bonus.IsDeleted = false;
            bonus.UpdateBy = userId;
            bonus.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return Result<string>.Success("Bonus restored successfully.");
        }
    }
}
