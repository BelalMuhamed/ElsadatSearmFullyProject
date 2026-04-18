using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.PublicHolidayService;
using Domain.Common;
using Domain.Entities.HR;
using Domain.UnitOfWork.Contract;
using System.Net;

namespace Infrastructure.Services.PublicHolidayServices
{
    internal class PublicHolidayService : IPublicHolidayService
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ICurrentUserService _CurrentUserService;
        public PublicHolidayService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _UnitOfWork = unitOfWork;
            _CurrentUserService = currentUserService;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<PublicHoliday>> GetAllPublicHoliday(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<PublicHoliday>(new List<PublicHoliday>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            var result = _UnitOfWork.GetRepository<PublicHoliday, int>().GetQueryable().OrderBy(x=>x.CreatedAt);
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<PublicHoliday>> GetAllActivePublicHoliday(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<PublicHoliday>(new List<PublicHoliday>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            var result = _UnitOfWork.GetRepository<PublicHoliday, int>().GetQueryable().Where(c => !c.IsDeleted);
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<Result<PublicHoliday>> GetPublicHolidayByID(int id)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<PublicHoliday>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            var result = await _UnitOfWork.GetRepository<PublicHoliday, int>().GetByIdAsync(id);
            if (result == null)
                return Result<PublicHoliday>.Failure("Can't Found This CollectionRepresentiveRate");
            return Result<PublicHoliday>.Success(result);
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<PublicHoliday>> GetSoftDeletePublicHoliday(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<PublicHoliday>(new List<PublicHoliday>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            var result = _UnitOfWork.GetRepository<PublicHoliday, int>().GetQueryable().Where(c => c.IsDeleted);
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> CreatePublicHoliday(PublicHoliday Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (Model == null)
                return Result<string>.Failure("Invalid PublicHoliday data .");
            Model.CreatedAt = DateTime.UtcNow;
            Model.CreateBy = userId;
            Model.IsDeleted = false;
            await _UnitOfWork.GetRepository<PublicHoliday, int>().AddAsync(Model);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("PublicHoliday created successfully .");
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> UpdatePublicHoliday(PublicHoliday Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (Model == null)
                return Result<string>.Failure("Invalid PublicHoliday data .");
            var existingDepartment = await _UnitOfWork.GetRepository<PublicHoliday,int>().GetByIdAsync(Model.Id);
            if(existingDepartment == null)
                return Result<string>.Failure("PublicHoliday Not Found.",HttpStatusCode.NotFound);
                existingDepartment.Date = Model.Date;
                existingDepartment.Name = Model.Name;
                existingDepartment.UpdateBy = userId;
                existingDepartment.UpdateAt = DateTime.UtcNow;
                await _UnitOfWork.GetRepository<PublicHoliday, int>().UpdateAsync(existingDepartment);
                await _UnitOfWork.SaveChangesAsync();
                return Result<string>.Success("PublicHoliday Updated Successfully .");
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> SoftDeletePublicHoliday(PublicHoliday Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (Model == null)
                return Result<string>.Failure("Invalid PublicHoliday data .");
            var existingDepartment = await _UnitOfWork.GetRepository<PublicHoliday,int>().GetByIdAsync(Model.Id);
            if(existingDepartment == null)
                return Result<string>.Failure("PublicHoliday Not Found.",HttpStatusCode.NotFound);

                existingDepartment.IsDeleted= true;
                existingDepartment.DeleteBy = userId;
                existingDepartment.DeleteAt = DateTime.UtcNow;

                await _UnitOfWork.GetRepository<PublicHoliday, int>().UpdateAsync(existingDepartment);
                await _UnitOfWork.SaveChangesAsync();
                return Result<string>.Success("PublicHoliday Soft Deleted Successfully .");
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> RestorePublicHoliday(PublicHoliday Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (Model == null)
                return Result<string>.Failure("Invalid PublicHoliday data .");
            var existingDepartment = await _UnitOfWork.GetRepository<PublicHoliday,int>().GetByIdAsync(Model.Id);
            if(existingDepartment == null)
                return Result<string>.Failure("PublicHoliday Not Found.",HttpStatusCode.NotFound);

                existingDepartment.IsDeleted = false;
                existingDepartment.UpdateBy = userId;
                existingDepartment.UpdateAt = DateTime.UtcNow;
                existingDepartment.DeleteBy = null;
                existingDepartment.DeleteAt = null;

                await _UnitOfWork.GetRepository<PublicHoliday, int>().UpdateAsync(existingDepartment);
                await _UnitOfWork.SaveChangesAsync();
                return Result<string>.Success("PublicHoliday Restored Successfully .");
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> HardDeletePublicHoliday(PublicHoliday Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
            if (Model == null)
                return Result<string>.Failure("Invalid PublicHoliday data .");
            var existingDepartment = await _UnitOfWork.GetRepository<PublicHoliday,int>().GetByIdAsync(Model.Id);
            if(existingDepartment == null)
                return Result<string>.Failure("PublicHoliday Not Found.",HttpStatusCode.NotFound);

                await _UnitOfWork.GetRepository<PublicHoliday, int>().DeleteAsync(existingDepartment);
                await _UnitOfWork.SaveChangesAsync();
                return Result<string>.Success("PublicHoliday Hard Deleted Successfully .");
        }
        //------------------------------------------------------------------------------
    }
}
