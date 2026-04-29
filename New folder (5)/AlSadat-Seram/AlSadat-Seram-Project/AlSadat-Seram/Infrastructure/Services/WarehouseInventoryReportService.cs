using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Stock;
using Application.Services.contract;
using ClosedXML.Excel;
using Domain.Common;
using Domain.Entities;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    /// <summary>
    /// Read-only reporting service for warehouse inventory.
    /// Builds the matrix in a single round-trip and pre-computes totals + health.
    /// </summary>
    public sealed class WarehouseInventoryReportService : IWarehouseInventoryReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseInventoryReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // 1) GET MATRIX
        // =====================================================================
        public async Task<Result<WarehouseInventoryMatrixDto>> GetInventoryMatrixAsync(
            WarehouseInventoryFilter filter,
            CancellationToken ct = default)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                filter ??= new WarehouseInventoryFilter();

                var dto = await BuildMatrixAsync(filter, ct);
                return Result<WarehouseInventoryMatrixDto>.Success(dto, "Success", HttpStatusCode.OK);
            }
            catch (OperationCanceledException)
            {
                return Result<WarehouseInventoryMatrixDto>.Failure(
                    "تم إلغاء الطلب", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<WarehouseInventoryMatrixDto>.Failure(
                    "حدث خطأ أثناء تحميل تقرير المخزون", HttpStatusCode.InternalServerError);
            }
        }

        // =====================================================================
        // 2) EXPORT TO EXCEL
        // =====================================================================
        public async Task<Result<byte[]>> ExportInventoryMatrixToExcelAsync(
            WarehouseInventoryFilter filter,
            CancellationToken ct = default)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                filter ??= new WarehouseInventoryFilter();

                // For exports we ignore paging and return the full filtered matrix.
                filter.page = 1;
                filter.pageSize = int.MaxValue;

                var matrix = await BuildMatrixAsync(filter, ct);
                var bytes  = BuildWorkbook(matrix);

                return Result<byte[]>.Success(bytes, "Success", HttpStatusCode.OK);
            }
            catch (OperationCanceledException)
            {
                return Result<byte[]>.Failure("تم إلغاء التصدير", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<byte[]>.Failure(
                    "حدث خطأ أثناء تصدير ملف الإكسل", HttpStatusCode.InternalServerError);
            }
        }

        // =====================================================================
        // INTERNAL — single source of truth for matrix construction
        // =====================================================================
        private async Task<WarehouseInventoryMatrixDto> BuildMatrixAsync(
            WarehouseInventoryFilter filter,
            CancellationToken ct)
        {
            // ---- 1) Warehouses (columns) -----------------------------------
            var storeRepo = _unitOfWork.GetRepository<Store, int>();
            var storesQuery = storeRepo.GetQueryable();

            if (filter.excludeDeletedWarehouses)
                storesQuery = storesQuery.Where(s => !s.isDeleted);

            if (filter.storeId.HasValue)
                storesQuery = storesQuery.Where(s => s.Id == filter.storeId.Value);

            var warehouses = await storesQuery
                .OrderBy(s => s.Id)
                .Select(s => new WarehouseColumnDto
                {
                    storeId   = s.Id,
                    storeName = s.StoreName,
                    isDeleted = s.isDeleted
                })
                .ToListAsync(ct);

            var warehouseIds = warehouses.Select(w => w.storeId).ToList();

            // ---- 2) Products (rows) ----------------------------------------
            var productRepo  = _unitOfWork.GetRepository<Products, int>();
            var productQuery = productRepo.GetQueryable();

            if (filter.excludeDeletedProducts)
                productQuery = productQuery.Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.productName))
                productQuery = productQuery.Where(p => p.Name.Contains(filter.productName));

            if (!string.IsNullOrWhiteSpace(filter.productCode))
                productQuery = productQuery.Where(p => p.productCode.Contains(filter.productCode));

            var products = await productQuery
                .OrderBy(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.productCode,
                    p.IsDeleted,
                    p.TheSmallestPossibleQuantity
                })
                .ToListAsync(ct);

            // ---- 3) Stock cells — single projection ------------------------
            var stockRepo = _unitOfWork.GetRepository<Stock, int>();
            var stockCells = await stockRepo
                .GetQueryable()
                .Where(s => warehouseIds.Contains(s.StoreId))
                .Select(s => new
                {
                    s.StoreId,
                    s.ProductId,
                    s.Quantity
                })
                .ToListAsync(ct);

            // Group once into a fast lookup: (productId -> storeId -> qty)
            var byProduct = stockCells
                .GroupBy(c => c.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.StoreId, x => x.Quantity));

            // ---- 4) Build product rows -------------------------------------
            var rows = new List<ProductInventoryRowDto>(products.Count);

            foreach (var p in products)
            {
                var qtyByStore = new Dictionary<int, decimal>(warehouseIds.Count);
                decimal total = 0m;

                if (byProduct.TryGetValue(p.Id, out var storeMap))
                {
                    foreach (var w in warehouseIds)
                    {
                        var q = storeMap.TryGetValue(w, out var v) ? v : 0m;
                        qtyByStore[w] = q;
                        total += q;
                    }
                }
                else
                {
                    foreach (var w in warehouseIds)
                        qtyByStore[w] = 0m;
                }

                rows.Add(new ProductInventoryRowDto
                {
                    productId        = p.Id,
                    productName      = p.Name,
                    productCode      = p.productCode,
                    isDeleted        = p.IsDeleted,
                    reorderThreshold = p.TheSmallestPossibleQuantity,
                    quantities       = qtyByStore,
                    totalQuantityAcrossWarehouses = total,
                    health           = ClassifyHealth(total, p.TheSmallestPossibleQuantity)
                });
            }

            // ---- 5) Optional low-stock-only filter -------------------------
            if (filter.lowStockOnly == true)
                rows = rows
                    .Where(r => r.totalQuantityAcrossWarehouses <= r.reorderThreshold)
                    .ToList();

            // ---- 6) Pagination (after filtering, keeps totals accurate over the page) -----
            var pagedRows = rows
                .Skip((Math.Max(filter.page, 1) - 1) * Math.Max(filter.pageSize, 1))
                .Take(Math.Max(filter.pageSize, 1))
                .ToList();

            // ---- 7) Footer totals — computed from the FULL filtered set ----
            var warehouseTotals = warehouses
                .Select(w => new WarehouseTotalDto
                {
                    storeId       = w.storeId,
                    totalQuantity = rows.Sum(r => r.quantities.TryGetValue(w.storeId, out var q) ? q : 0m)
                })
                .ToList();

            var grandTotal = rows.Sum(r => r.totalQuantityAcrossWarehouses);

            return new WarehouseInventoryMatrixDto
            {
                warehouses          = warehouses,
                products            = pagedRows,
                warehouseTotals     = warehouseTotals,
                grandTotalQuantity  = grandTotal
            };
        }

        // ---------------------------------------------------------------------
        // Health classification — ONE place. UI must not re-implement this.
        //   total == 0                           → OutOfStock
        //   total <= threshold                   → Critical
        //   threshold < total <= threshold * 2   → Warning
        //   total >  threshold * 2               → Healthy
        // ---------------------------------------------------------------------
        private static StockHealth ClassifyHealth(decimal total, int threshold)
        {
            if (total <= 0)            return StockHealth.OutOfStock;
            if (threshold <= 0)        return StockHealth.Healthy; // no threshold configured
            if (total <= threshold)    return StockHealth.Critical;
            if (total <= threshold * 2)return StockHealth.Warning;
            return StockHealth.Healthy;
        }

        // =====================================================================
        // EXCEL — ClosedXML
        // =====================================================================
        private static byte[] BuildWorkbook(WarehouseInventoryMatrixDto m)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Warehouse Inventory");
            ws.RightToLeft = true;

            // ---- header row ----
            ws.Cell(1, 1).Value = "كود المنتج";
            ws.Cell(1, 2).Value = "اسم المنتج";
            ws.Cell(1, 3).Value = "الحد الأدنى";

            int col = 4;
            var storeCol = new Dictionary<int, int>();
            foreach (var w in m.warehouses)
            {
                storeCol[w.storeId] = col;
                ws.Cell(1, col).Value = w.storeName;
                col++;
            }

            int totalCol = col;
            ws.Cell(1, totalCol).Value = "الإجمالي";
            ws.Cell(1, totalCol + 1).Value = "الحالة";

            // header style: gold (matches the rest of the system)
            var headerRange = ws.Range(1, 1, 1, totalCol + 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.Black;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder  = XLBorderStyleValues.Thin;

            // ---- data rows ----
            int row = 2;
            foreach (var p in m.products)
            {
                ws.Cell(row, 1).Value = p.productCode;
                ws.Cell(row, 2).Value = p.productName;
                ws.Cell(row, 3).Value = p.reorderThreshold;

                foreach (var w in m.warehouses)
                {
                    var qty = p.quantities.TryGetValue(w.storeId, out var v) ? v : 0m;
                    ws.Cell(row, storeCol[w.storeId]).Value = qty;
                }

                ws.Cell(row, totalCol).Value     = p.totalQuantityAcrossWarehouses;
                ws.Cell(row, totalCol + 1).Value = TranslateHealth(p.health);

                // Color the total cell based on health
                var totalCell = ws.Cell(row, totalCol);
                totalCell.Style.Fill.BackgroundColor = HealthFill(p.health);
                totalCell.Style.Font.Bold = true;

                row++;
            }

            // ---- footer / totals row ----
            int footer = row;
            ws.Cell(footer, 1).Value = "الإجمالي";
            ws.Range(footer, 1, footer, 3).Merge();

            foreach (var w in m.warehouses)
            {
                var t = m.warehouseTotals.FirstOrDefault(x => x.storeId == w.storeId);
                ws.Cell(footer, storeCol[w.storeId]).Value = t?.totalQuantity ?? 0m;
            }
            ws.Cell(footer, totalCol).Value = m.grandTotalQuantity;

            var footerRange = ws.Range(footer, 1, footer, totalCol + 1);
            footerRange.Style.Font.Bold = true;
            footerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
            footerRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;

            // ---- finishing touches ----
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.RangeUsed().Style.Border.InsideBorder  = XLBorderStyleValues.Hair;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private static XLColor HealthFill(StockHealth h) => h switch
        {
            StockHealth.OutOfStock => XLColor.FromHtml("#F8D7DA"), // light red
            StockHealth.Critical   => XLColor.FromHtml("#FFE0B2"), // amber
            StockHealth.Warning    => XLColor.FromHtml("#FFF59D"), // yellow
            StockHealth.Healthy    => XLColor.FromHtml("#C8E6C9"), // light green
            _                      => XLColor.NoColor
        };

        private static string TranslateHealth(StockHealth h) => h switch
        {
            StockHealth.OutOfStock => "نفد المخزون",
            StockHealth.Critical   => "حرج",
            StockHealth.Warning    => "منخفض",
            StockHealth.Healthy    => "آمن",
            _                      => string.Empty
        };
    }
}
