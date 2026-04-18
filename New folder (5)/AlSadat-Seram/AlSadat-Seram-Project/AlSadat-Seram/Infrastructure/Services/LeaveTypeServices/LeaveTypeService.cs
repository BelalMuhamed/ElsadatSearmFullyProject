using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.LeaveType;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LeaveType;
using Domain.Common;
using Domain.Entities.HR;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.LeaveTypeServices
{
    public class LeaveTypeService:ILeaveTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public LeaveTypeService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> AddLeaveTypeAsync(LeaveTypeDto leaveTypeDto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                // التحقق من عدم تكرار الاسم
                var existingType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .FindAsync(lt => lt.Name == leaveTypeDto.Name && !lt.IsDeleted);

                if(existingType != null)
                    return Result<string>.Failure("هذا النوع موجود بالفعل");

                var leaveType = new LeaveType
                {
                    Name = leaveTypeDto.Name,
                    IsPaid = leaveTypeDto.IsPaid,
                    CreatedAt = DateTime.UtcNow,
                    CreateBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.GetRepository<LeaveType,int>().AddAsync(leaveType);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم إضافة نوع الإجازة بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<Result<string>> UpdateLeaveTypeAsync(LeaveTypeDto leaveTypeDto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var leaveType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .GetByIdAsync(leaveTypeDto.Id);

                if(leaveType == null || leaveType.IsDeleted)
                    return Result<string>.Failure("نوع الإجازة غير موجود");

                // التحقق من عدم تكرار الاسم (استثناء النوع الحالي)
                var existingType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .FindAsync(lt => lt.Name == leaveTypeDto.Name &&
                                   lt.Id != leaveTypeDto.Id &&
                                   !lt.IsDeleted);

                if(existingType != null)
                    return Result<string>.Failure("هذا الاسم مستخدم بالفعل لنوع إجازة آخر");

                leaveType.Name = leaveTypeDto.Name;
                leaveType.IsPaid = leaveTypeDto.IsPaid;
                leaveType.UpdateBy = userId;
                leaveType.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم تحديث نوع الإجازة بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<Result<string>> SoftDeleteLeaveTypeAsync(int id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var leaveType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .GetByIdAsync(id);

                if(leaveType == null)
                    return Result<string>.Failure("نوع الإجازة غير موجود");

                if(leaveType.IsDeleted)
                    return Result<string>.Failure("نوع الإجازة محذوف بالفعل");

                // التحقق من استخدام النوع
                var isUsed = await CheckLeaveTypeUsageAsync(id);
                if(!isUsed.IsSuccess)
                    return Result<string>.Failure("لا يمكن حذف نوع الإجازة المستخدم في طلبات أو أرصدة");

                leaveType.IsDeleted = true;
                leaveType.DeleteBy = userId;
                leaveType.DeleteAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم حذف نوع الإجازة بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<Result<string>> RestoreLeaveTypeAsync(int id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var leaveType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .GetByIdAsync(id);

                if(leaveType == null)
                    return Result<string>.Failure("نوع الإجازة غير موجود");

                if(!leaveType.IsDeleted)
                    return Result<string>.Failure("نوع الإجازة غير محذوف");

                leaveType.IsDeleted = false;
                leaveType.UpdateBy = userId;
                leaveType.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم استعادة نوع الإجازة بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<Result<LeaveTypeDto>> GetLeaveTypeByIdAsync(int id)
        {
            try
            {
                var leaveType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .GetByIdAsync(id);

                if(leaveType == null || leaveType.IsDeleted)
                    return Result<LeaveTypeDto>.Failure("نوع الإجازة غير موجود");

                var dto = new LeaveTypeDto
                {
                    Id = leaveType.Id,
                    Name = leaveType.Name,
                    IsPaid = leaveType.IsPaid,
                    CreatedAt = leaveType.CreatedAt,
                    CreatedBy = leaveType.CreateBy ?? "",
                    UpdatedAt = leaveType.UpdateAt,
                    UpdatedBy = leaveType.UpdateBy
                };

                return Result<LeaveTypeDto>.Success(dto);
            }
            catch(Exception ex)
            {
                return Result<LeaveTypeDto>.Failure($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<PagedList<LeaveTypeDto>> GetAllLeaveTypesAsync(PaginationParams paginationParams)
        {
            var query = _unitOfWork.GetRepository<LeaveType,int>()
                .GetQueryable()
                .Where(lt => !lt.IsDeleted)
                .OrderBy(lt => lt.Name);

            var result = query.Select(lt => new LeaveTypeDto
            {
                Id = lt.Id,
                Name = lt.Name,
                IsPaid = lt.IsPaid,
                CreatedAt = lt.CreatedAt,
                CreatedBy = lt.CreateBy ?? "",
                UpdatedAt = lt.UpdateAt,
                UpdatedBy = lt.UpdateBy
            });

            return await result.ToPagedListAsync(paginationParams.PageNumber,paginationParams.PageSize);
        }

        public async Task<Result<List<LeaveTypeDto>>> GetActiveLeaveTypesAsync()
        {
            try
            {
                var leaveTypes = await _unitOfWork.GetRepository<LeaveType,int>()
                    .GetQueryable()
                    .Where(lt => !lt.IsDeleted)
                    .OrderBy(lt => lt.Name)
                    .Select(lt => new LeaveTypeDto
                    {
                        Id = lt.Id,
                        Name = lt.Name,
                        IsPaid = lt.IsPaid
                    })
                    .ToListAsync();

                return Result<List<LeaveTypeDto>>.Success(leaveTypes);
            }
            catch(Exception ex)
            {
                return Result<List<LeaveTypeDto>>.Failure($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<Result<bool>> CheckLeaveTypeUsageAsync(int leaveTypeId)
        {
            try
            {
                // التحقق من استخدام النوع في طلبات الإجازة
                var hasRequests = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetQueryable()
                    .AnyAsync(lr => lr.LeaveTypeId == leaveTypeId && !lr.IsDeleted);

                if(hasRequests)
                    return Result<bool>.Success(true);

                // التحقق من استخدام النوع في أرصدة الإجازة
                var hasBalances = await _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                    .GetQueryable()
                    .AnyAsync(b => b.LeaveTypeId == leaveTypeId && !b.IsDeleted);

                return Result<bool>.Success(hasBalances);
            }
            catch(Exception ex)
            {
                return Result<bool>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
    }
}
