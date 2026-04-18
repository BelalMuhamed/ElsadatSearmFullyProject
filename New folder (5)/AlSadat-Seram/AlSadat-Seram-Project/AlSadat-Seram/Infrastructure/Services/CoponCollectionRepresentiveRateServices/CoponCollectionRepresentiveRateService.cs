using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.Services.contract.CoponCollectionRepresentiveRateService;
using Application.Services.contract.CurrentUserService;
using Domain.Common;
using Domain.Entities.HR;
using Domain.UnitOfWork.Contract;
using System.Net;

namespace Infrastructure.Services.CoponCollectionRepresentiveRateServices;
internal class CoponCollectionRepresentiveRateService:ICoponCollectionRepresentiveRateService
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly ICurrentUserService _CurrentUserService;

    public CoponCollectionRepresentiveRateService(IUnitOfWork unitOfWork , ICurrentUserService currentUserService)
    {
        _UnitOfWork = unitOfWork;
        _CurrentUserService = currentUserService;
    }
    //------------------------------------------------------------------------
    public async Task<PagedList<CoponCollectionRepresentiveRate>> GetAllCoponCollectionRepresentiveRate(PaginationParams paginationParams)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return new PagedList<CoponCollectionRepresentiveRate>(new List<CoponCollectionRepresentiveRate>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
        
        var result = _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate, int>().GetQueryable();
        var pagedResult = await result.OrderBy(c => c.NumberOfCopons).ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        return pagedResult;
    }
    //------------------------------------------------------------------------
    public async Task<PagedList<CoponCollectionRepresentiveRate>> GetAllActiveCoponCollectionRepresentiveRate(PaginationParams paginationParams)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return new PagedList<CoponCollectionRepresentiveRate>(new List<CoponCollectionRepresentiveRate>(), 0, paginationParams.PageNumber, paginationParams.PageSize);

        var result = _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().GetQueryable().Where(c=>!c.IsDeleted);
        var pagedResult = await result.OrderBy(c => c.NumberOfCopons).ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        return pagedResult;
    }
    //------------------------------------------------------------------------
    public async Task<Result<CoponCollectionRepresentiveRate>> GetCoponCollectionRepresentiveRateById(int id)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<CoponCollectionRepresentiveRate>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);

        var result =await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().GetByIdAsync(id);
        if(result==null)
            return Result<CoponCollectionRepresentiveRate>.Failure("Can't Found This CoponCollectionRepresentiveRate");
        return Result<CoponCollectionRepresentiveRate>.Success(result);
    }
    //------------------------------------------------------------------------
    public async Task<PagedList<CoponCollectionRepresentiveRate>> GetSoftDeleteCoponCollectionRepresentiveRate(PaginationParams paginationParams)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return new PagedList<CoponCollectionRepresentiveRate>(new List<CoponCollectionRepresentiveRate>(), 0, paginationParams.PageNumber, paginationParams.PageSize);

        var result =  _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().GetQueryable().Where(c => c.IsDeleted);
        var pagedResult = await result.OrderBy(c => c.NumberOfCopons).ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        return pagedResult;
    }
    //------------------------------------------------------------------------
    public async Task<Result<string>> CreateCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);

        if (coponCollectionRepresentiveRate == null)
            return Result<string>.Failure("Invalid CoponCollectionRepresentiveRate data.");
        
        coponCollectionRepresentiveRate.CreatedAt = DateTime.Now;
        coponCollectionRepresentiveRate.CreateBy = userId;

        await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().AddAsync(coponCollectionRepresentiveRate);
        await _UnitOfWork.SaveChangesAsync();
        return Result<string>.Success("تم الانشاء بنجاح");

    }
    //------------------------------------------------------------------------
    public async Task<Result<string>> UpdateCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا",HttpStatusCode.Unauthorized);

        if (coponCollectionRepresentiveRate == null)
            return Result<string>.Failure("Invalid CoponCollectionRepresentiveRate Data.");
        
        var existingCoponCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate, int>().GetByIdAsync(coponCollectionRepresentiveRate.Id);
        if (existingCoponCollectionRepresentiveRate == null)
            return Result<string>.Failure("CoponCollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

        existingCoponCollectionRepresentiveRate.UpdateBy = userId;
        existingCoponCollectionRepresentiveRate.UpdateAt = DateTime.Now;
        existingCoponCollectionRepresentiveRate.Cashed = coponCollectionRepresentiveRate.Cashed;
        existingCoponCollectionRepresentiveRate.NumberOfCopons = coponCollectionRepresentiveRate.NumberOfCopons;

            await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().UpdateAsync(existingCoponCollectionRepresentiveRate);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("تم التحديث بنجاح");
    }
    //------------------------------------------------------------------------

    public async Task<Result<string>> SoftDeleteCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);

        if (coponCollectionRepresentiveRate == null)
            return Result<string>.Failure("Invalid CoponCollectionRepresentiveRate Data.");
        var existingcoponCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate, int>().GetByIdAsync(coponCollectionRepresentiveRate.Id);
        if (existingcoponCollectionRepresentiveRate == null)
            return Result<string>.Failure("CollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

        existingcoponCollectionRepresentiveRate.IsDeleted = true;
        existingcoponCollectionRepresentiveRate.DeleteBy = userId;
        existingcoponCollectionRepresentiveRate.DeleteAt = DateTime.UtcNow;
            await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().UpdateAsync(existingcoponCollectionRepresentiveRate);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("تم التعطيل بنجاح");
    }
    //------------------------------------------------------------------------
    public async Task<Result<string>> RestoreCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);

        if (coponCollectionRepresentiveRate == null)
            return Result<string>.Failure("Invalid CoponCollectionRepresentiveRate Data.");
        
        var existingcoponCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate, int>().GetByIdAsync(coponCollectionRepresentiveRate.Id);
        if (existingcoponCollectionRepresentiveRate == null)
            return Result<string>.Failure("CollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

        existingcoponCollectionRepresentiveRate.IsDeleted = false;
        existingcoponCollectionRepresentiveRate.DeleteBy = null;
        existingcoponCollectionRepresentiveRate.DeleteAt = null;
        existingcoponCollectionRepresentiveRate.UpdateBy = userId;
        existingcoponCollectionRepresentiveRate.UpdateAt= DateTime.UtcNow;
            await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().UpdateAsync(existingcoponCollectionRepresentiveRate);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("تمت الاستعادة بنجاح");
    }
    //------------------------------------------------------------------------
    public async Task<Result<string>> HardDeleteCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا", HttpStatusCode.Unauthorized);

        if (coponCollectionRepresentiveRate == null)
            return Result<string>.Failure("Invalid CoponCollectionRepresentiveRate Data.");
        
        var existingcoponCollectionRepresentiveRate = await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate, int>().GetByIdAsync(coponCollectionRepresentiveRate.Id);
        if (existingcoponCollectionRepresentiveRate == null)
            return Result<string>.Failure("CollectionRepresentiveRate Not Found.", HttpStatusCode.NotFound);

            await _UnitOfWork.GetRepository<CoponCollectionRepresentiveRate,int>().DeleteAsync(existingcoponCollectionRepresentiveRate);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("تم الحذف النهائي بنجاح");
    }
    //-------------------------------------------------------------------------
}
