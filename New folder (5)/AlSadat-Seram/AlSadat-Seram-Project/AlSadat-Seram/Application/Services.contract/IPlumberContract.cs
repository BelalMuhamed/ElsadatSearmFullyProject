using AlSadatSeram.Services.contract;
using Domain.Common;
using static Application.DTOs.PlumberDtos;

namespace Application.Services.contract
{
    /// <summary>
    /// Application-layer contract for plumber management.
    /// Mirrors <see cref="ISupplierContract"/> in shape and conventions.
    /// All methods return <see cref="Result{T}"/> for uniform error/success semantics.
    /// </summary>
    public interface IPlumberContract
    {
        /// <summary>Paginated, filterable list of plumbers.</summary>
        Task<Result<ApiResponse<List<PlumberDto>>>> GetAllPlumbers(PlumberFilteration req);

        /// <summary>Single plumber with city name. Rejects soft-deleted.</summary>
        Task<Result<PlumberDto>> GetById(int id);

        /// <summary>Lightweight {id,name,specialty} list for select-boxes. Only active.</summary>
        Task<Result<List<PlumberLookupDto>>> GetPlumberLookups(PlumberLookupFilter filter);

        /// <summary>
        /// Creates a new plumber. Blocks duplicates on (Name + PhoneNumbers).
        /// Also blocks duplicate <see cref="PlumberDto.licenseNumber"/> when one is supplied.
        /// </summary>
        Task<Result<string>> AddNewPlumber(PlumberDto dto);

        /// <summary>Updates core fields. Rejects edits on soft-deleted. Does NOT touch IsDeleted.</summary>
        Task<Result<string>> EditPlumber(PlumberDto dto);

        /// <summary>Flips the plumber's IsDeleted flag.</summary>
        Task<Result<string>> TogglePlumberStatus(int id);

        /// <summary>Imports plumbers from an Excel stream. Supports partial success with per-row error reporting.</summary>
        Task<Result<PlumberImportResultDto>> ImportFromExcelAsync(Stream fileStream, CancellationToken ct);

        /// <summary>Generates the downloadable Excel import template.</summary>
        Task<Result<byte[]>> GenerateImportTemplateAsync(CancellationToken ct);

        /// <summary>Exports the current filtered list of plumbers as an Excel workbook.</summary>
        Task<Result<byte[]>> ExportToExcelAsync(PlumberFilteration filter, CancellationToken ct);
    }
}
