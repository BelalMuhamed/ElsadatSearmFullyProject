using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Domain.Common;
using Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.CoponCollectionRepresentiveRateService;
public interface ICoponCollectionRepresentiveRateService
{
    Task<PagedList<CoponCollectionRepresentiveRate>> GetAllCoponCollectionRepresentiveRate(PaginationParams paginationParams);
    Task<PagedList<CoponCollectionRepresentiveRate>> GetAllActiveCoponCollectionRepresentiveRate(PaginationParams paginationParams);
    Task<Result<CoponCollectionRepresentiveRate>> GetCoponCollectionRepresentiveRateById(int id);
    Task<PagedList<CoponCollectionRepresentiveRate>> GetSoftDeleteCoponCollectionRepresentiveRate(PaginationParams paginationParams);
    Task<Result<string>> CreateCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate);
    Task<Result<string>> UpdateCoponCollectionRepresentiveRate( CoponCollectionRepresentiveRate coponCollectionRepresentiveRate);
    Task<Result<string>> SoftDeleteCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate);
    Task<Result<string>> RestoreCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate);
    Task<Result<string>> HardDeleteCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate);
}
