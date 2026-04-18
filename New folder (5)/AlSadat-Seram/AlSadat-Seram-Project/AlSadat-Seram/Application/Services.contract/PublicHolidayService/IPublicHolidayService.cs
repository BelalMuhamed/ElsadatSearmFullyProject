using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Domain.Common;
using Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.PublicHolidayService
{
    public interface IPublicHolidayService
    {
        Task<PagedList<PublicHoliday>> GetAllPublicHoliday(PaginationParams paginationParams);
        Task<PagedList<PublicHoliday>> GetAllActivePublicHoliday(PaginationParams paginationParams);
        Task<Result<PublicHoliday>> GetPublicHolidayByID(int id);
        Task<PagedList<PublicHoliday>> GetSoftDeletePublicHoliday(PaginationParams paginationParams);
        Task<Result<string>> CreatePublicHoliday(PublicHoliday Model);
        Task<Result<string>> UpdatePublicHoliday(PublicHoliday Model);
        Task<Result<string>> SoftDeletePublicHoliday(PublicHoliday Model);
        Task<Result<string>> RestorePublicHoliday(PublicHoliday Model);
        Task<Result<string>> HardDeletePublicHoliday(PublicHoliday Model);
    }
}
