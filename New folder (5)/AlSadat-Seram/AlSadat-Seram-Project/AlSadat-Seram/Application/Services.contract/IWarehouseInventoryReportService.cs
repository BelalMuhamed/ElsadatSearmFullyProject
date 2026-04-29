using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Stock;
using Domain.Common;

namespace Application.Services.contract
{
    /// <summary>
    /// Read-only reporting contract for warehouse inventory.
    /// Kept SEPARATE from <see cref="IStockService"/> so that the existing stock
    /// CRUD/transactional code path is not impacted (Single Responsibility).
    /// </summary>
    public interface IWarehouseInventoryReportService
    {
        /// <summary>Returns the product-centric inventory matrix used by the مخزون page.</summary>
        Task<Result<WarehouseInventoryMatrixDto>> GetInventoryMatrixAsync(
            WarehouseInventoryFilter filter,
            CancellationToken ct = default);

        /// <summary>Generates an .xlsx export of the inventory matrix.</summary>
        Task<Result<byte[]>> ExportInventoryMatrixToExcelAsync(
            WarehouseInventoryFilter filter,
            CancellationToken ct = default);
    }
}