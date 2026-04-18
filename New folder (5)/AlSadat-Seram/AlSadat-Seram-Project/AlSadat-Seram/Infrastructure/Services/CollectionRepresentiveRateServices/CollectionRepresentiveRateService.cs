using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.Services.contract.CollectionRepresentiveRateService;
using Application.Services.contract.CurrentUserService;
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

namespace Infrastructure.Services.CollectionRepresentiveRateServices
{
    internal class CollectionRepresentiveRateService:ICollectionRepresentiveRateService
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ICurrentUserService _CurrentUserService;
        public CollectionRepresentiveRateService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _UnitOfWork = unitOfWork;
            _CurrentUserService = currentUserService;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<CollectionRepresentiveRate>> GetAllCollectionRepresentiveRate(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<CollectionRepresentiveRate>(new List<CollectionRepresentiveRate>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            
            var result = _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetQueryable();
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<CollectionRepresentiveRate>> GetAllActiveCollectionRepresentiveRate(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<CollectionRepresentiveRate>(new List<CollectionRepresentiveRate>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            
            var result = _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetQueryable().Where(c => !c.IsDeleted);
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<Result<CollectionRepresentiveRate>> GetCollectionRepresentiveRateById(int id)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<CollectionRepresentiveRate>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);
            
            var result = await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetByIdAsync(id);
            if (result == null)
                return Result<CollectionRepresentiveRate>.Failure("Can't Found This CollectionRepresentiveRate");
            return Result<CollectionRepresentiveRate>.Success(result);
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<CollectionRepresentiveRate>> GetSoftDeleteCollectionRepresentiveRate(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<CollectionRepresentiveRate>(new List<CollectionRepresentiveRate>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            
            var result = _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetQueryable().Where(c => c.IsDeleted);
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> CreateCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);
            
            if(Model==null)
                return Result<string>.Failure("Invalid CollectionRepresentiveRate data .");
            
            Model.CreatedAt = DateTime.UtcNow;
            Model.CreateBy = userId;
            Model.IsDeleted = false;
            
            await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().AddAsync(Model);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("تم الانشاء بنجاح");
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> UpdateCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);
            
            if (Model == null)
                return Result<string>.Failure("Invalid CollectionRepresentiveRate data .");
            
            var existingCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetByIdAsync(Model.Id);
            if (existingCollectionRepresentiveRate == null)
                return Result<string>.Failure("CollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

            existingCollectionRepresentiveRate.UpdateBy=userId;
            existingCollectionRepresentiveRate.UpdateAt=DateTime.UtcNow;
            existingCollectionRepresentiveRate.From=Model.From;
            existingCollectionRepresentiveRate.To=Model.To;
            existingCollectionRepresentiveRate.Precentage=Model.Precentage;

            await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().UpdateAsync(existingCollectionRepresentiveRate);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("تم التحديث بنجاح");
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> SoftDeleteCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);
            
            if (Model == null)
                return Result<string>.Failure("Invalid CollectionRepresentiveRate data .");
            
            var existingCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetByIdAsync(Model.Id);
            if (existingCollectionRepresentiveRate == null)
                return Result<string>.Failure("CollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

            existingCollectionRepresentiveRate.DeleteBy=userId;
            existingCollectionRepresentiveRate.DeleteAt=DateTime.UtcNow;
            existingCollectionRepresentiveRate.IsDeleted=true;

                await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().UpdateAsync(existingCollectionRepresentiveRate);
                await _UnitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم التعطيل بنجاح");           
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> RestoreCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);
            
            if (Model == null)
                return Result<string>.Failure("Invalid CollectionRepresentiveRate data .");
            
            var existingCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetByIdAsync(Model.Id);
            if (existingCollectionRepresentiveRate == null)
                return Result<string>.Failure("CollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

            existingCollectionRepresentiveRate.IsDeleted = false;
            existingCollectionRepresentiveRate.DeleteBy = null;
            existingCollectionRepresentiveRate.DeleteAt = null;
            existingCollectionRepresentiveRate.UpdateAt = DateTime.UtcNow;
            existingCollectionRepresentiveRate.UpdateBy = userId;

            await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().UpdateAsync(existingCollectionRepresentiveRate);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("تمت الاستعادة بنجاح");
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> HardDeleteCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);
            
            if (Model == null)
                return Result<string>.Failure("Invalid CollectionRepresentiveRate data .");
            
            var existingCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().GetByIdAsync(Model.Id);
            if (existingCollectionRepresentiveRate == null)
                return Result<string>.Failure("CollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

                await _UnitOfWork.GetRepository<CollectionRepresentiveRate, int>().DeleteAsync(existingCollectionRepresentiveRate);
                await _UnitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم الحذف النهائي بنجاح");           
        }
        //-------------------------------------------------------------------------

    }
}
