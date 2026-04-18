using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Domain.Common;
using Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.CollectionRepresentiveRateService
{
    public interface ICollectionRepresentiveRateService
    {
        Task<PagedList<CollectionRepresentiveRate>> GetAllCollectionRepresentiveRate(PaginationParams paginationParams);
        Task<PagedList<CollectionRepresentiveRate>> GetAllActiveCollectionRepresentiveRate(PaginationParams paginationParams);
        Task<Result<CollectionRepresentiveRate>> GetCollectionRepresentiveRateById(int id);
        Task<PagedList<CollectionRepresentiveRate>> GetSoftDeleteCollectionRepresentiveRate(PaginationParams paginationParams);
        Task<Result<string>> CreateCollectionRepresentiveRate(CollectionRepresentiveRate Model);
        Task<Result<string>> UpdateCollectionRepresentiveRate(CollectionRepresentiveRate Model);
        Task<Result<string>> SoftDeleteCollectionRepresentiveRate(CollectionRepresentiveRate Model);
        Task<Result<string>> RestoreCollectionRepresentiveRate(CollectionRepresentiveRate Model);
        Task<Result<string>> HardDeleteCollectionRepresentiveRate(CollectionRepresentiveRate Model);
    }
}
