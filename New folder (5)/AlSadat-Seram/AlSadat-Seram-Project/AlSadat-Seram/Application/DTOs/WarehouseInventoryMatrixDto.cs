using System.Collections.Generic;

namespace Application.DTOs.Stock
{
    /// <summary>
    /// Product-centric inventory matrix — one row per product, one cell per warehouse,
    /// plus a pre-computed total across all warehouses. This is the shape consumed by:
    ///  - the المخزون page
    ///  - the Excel export
    /// Computing totals on the server keeps the UI a thin renderer (SRP).
    /// </summary>
    public sealed class WarehouseInventoryMatrixDto
    {
        /// <summary>Column metadata for the matrix (active warehouses, in display order).</summary>
        public List<WarehouseColumnDto> warehouses { get; set; } = new();

        /// <summary>One row per product.</summary>
        public List<ProductInventoryRowDto> products { get; set; } = new();

        /// <summary>Footer row — total quantity per warehouse across all products.</summary>
        public List<WarehouseTotalDto> warehouseTotals { get; set; } = new();

        /// <summary>Grand total quantity across all products and all warehouses.</summary>
        public decimal grandTotalQuantity { get; set; }
    }

    public sealed class WarehouseColumnDto
    {
        public int storeId { get; set; }
        public string storeName { get; set; } = string.Empty;
        public bool isDeleted { get; set; }
    }

    public sealed class ProductInventoryRowDto
    {
        public int productId { get; set; }
        public string productName { get; set; } = string.Empty;
        public string productCode { get; set; } = string.Empty;
        public bool isDeleted { get; set; }

        /// <summary>Reorder threshold — drives the conditional coloring.</summary>
        public int reorderThreshold { get; set; }

        /// <summary>quantities[storeId] = quantity in that warehouse (0 if missing).</summary>
        public Dictionary<int, decimal> quantities { get; set; } = new();

        /// <summary>Sum across all warehouses — pre-computed on the server.</summary>
        public decimal totalQuantityAcrossWarehouses { get; set; }

        /// <summary>Server-evaluated health status — UI just reads it. (Open/Closed: rule lives in one place.)</summary>
        public StockHealth health { get; set; }
    }

    public sealed class WarehouseTotalDto
    {
        public int storeId { get; set; }
        public decimal totalQuantity { get; set; }
    }

    /// <summary>Stock health classification, evaluated server-side from the reorder threshold.</summary>
    public enum StockHealth
    {
        OutOfStock = 0,
        Critical = 1,
        Warning = 2,
        Healthy = 3
    }

    /// <summary>Filter parameters for the inventory matrix.</summary>
    public sealed class WarehouseInventoryFilter
    {
        public string? productName { get; set; }
        public string? productCode { get; set; }
        public int? storeId { get; set; }

        /// <summary>If true, only rows where total ≤ reorderThreshold are returned.</summary>
        public bool? lowStockOnly { get; set; }

        /// <summary>If true, soft-deleted products are excluded (default: true).</summary>
        public bool excludeDeletedProducts { get; set; } = true;

        /// <summary>If true, soft-deleted warehouses are excluded (default: true).</summary>
        public bool excludeDeletedWarehouses { get; set; } = true;

        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 50;
    }
}