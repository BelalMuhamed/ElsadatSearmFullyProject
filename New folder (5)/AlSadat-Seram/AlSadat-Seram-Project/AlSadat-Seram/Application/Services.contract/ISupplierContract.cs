//using AlSadatSeram.Services.contract;
//using Application.DTOs.SupplierDtosF;
//using Domain.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Application.DTOs.SupplierDtosF.SupplierDtos;

//namespace Application.Services.contract
//{
//        public interface ISupplierContract
//        {
//            Task<ApiResponse<List<SupplierDto>>> GetAllSuppliers(SupplierFilteration req);
//            Task<Result<string>> AddNewSupllier(SupplierDto dto);
//            Task<Result<string>> EditSupplier(SupplierDto dto);
//            Task<Result<SupplierDto>> GetById(int id);
//            Task<Result<SupplierImportResultDto>> ImportFromExcelAsync(Stream fileStream, string createdBy, CancellationToken ct = default);
//            Task<Result<byte[]>> GenerateImportTemplateAsync(CancellationToken ct = default);
//        }
//}
using AlSadatSeram.Services.contract;
using Domain.Common;
using static Application.DTOs.SupplierDtos;

namespace Application.Services.contract
{
    /// <summary>
    /// Application-layer contract for supplier management.
    /// All methods return <see cref="Result{T}"/> for uniform error/success semantics.
    /// Paginated lists return <c>Result&lt;ApiResponse&lt;List&lt;T&gt;&gt;&gt;</c>.
    /// </summary>
    public interface ISupplierContract
    {
        /// <summary>Paginated, filterable list of suppliers.</summary>
        Task<Result<ApiResponse<List<SupplierDto>>>> GetAllSuppliers(SupplierFilteration req);

        /// <summary>Single supplier with city name. Rejects soft-deleted.</summary>
        Task<Result<SupplierDto>> GetById(int id);

        /// <summary>Lightweight {id,name} list for select-boxes. Only active (IsDeleted = false).</summary>
        Task<Result<List<SupplierLookupDto>>> GetSupplierLookups(SupplierLookupFilter filter);

        /// <summary>Creates a new supplier. Blocks duplicates on (Name + PhoneNumbers).</summary>
        Task<Result<string>> AddNewSupplier(SupplierDto dto);

        /// <summary>Updates core supplier fields. Rejects edits on soft-deleted suppliers. Does NOT touch IsDeleted.</summary>
        Task<Result<string>> EditSupplier(SupplierDto dto);

        /// <summary>Flips the supplier's IsDeleted flag. Dedicated method — safer than a full PUT.</summary>
        Task<Result<string>> ToggleSupplierStatus(int id);

        /// <summary>Imports suppliers from an Excel stream. Supports partial success with per-row error reporting.</summary>
        Task<Result<SupplierImportResultDto>> ImportFromExcelAsync(Stream fileStream, CancellationToken ct);

        /// <summary>Generates the downloadable Excel import template (bytes of an .xlsx).</summary>
        Task<Result<byte[]>> GenerateImportTemplateAsync(CancellationToken ct);
    }
}