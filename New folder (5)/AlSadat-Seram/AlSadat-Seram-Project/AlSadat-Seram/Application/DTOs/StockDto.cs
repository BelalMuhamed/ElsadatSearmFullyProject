using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class StockDto
    {
        public int storeID { get; set; }
        public string storeName { get; set; }
        public bool isDeleted { get; set; }
        public List<StockProducts> storeStocks { get; set; }
    }
    public class StockProducts
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public decimal quantity { get; set; }
        public bool isDeleted { get; set; }
        public int lowQuantity { get; set; }
     
    }
    public class StockFilterations
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public string? storeName { get; set; }
    }
    public class ProductStockPerStoreDto
    {

        public string? storeName { get; set; }
        public int storeId { get; set; }
        public bool? isStoreDeleted { get; set; }
        public decimal? avaliableQuantity { get; set; }
        public int? withdrawnQuantity { get; set; }
    }
    public class ProductStockDto
    {
        public int productId { get; set; }
        public string?productName { get; set; }
        public bool? isProductDeleted { get; set; }
        public bool? isCategoryDeleted { get; set; }
        public List<ProductStockPerStoreDto> stocks { get; set; } = null;

    }
    /// <summary>
    /// Lightweight projection used by the Stock-Transfer page to populate
    /// the source-warehouse product picker.
    /// <para>
    /// Only includes products with on-hand quantity &gt; 0 — there is no
    /// reason to offer a zero-stock product as a transfer candidate.
    /// </para>
    /// <para>
    /// <see cref="avgCost"/> is exposed so the UI can display a read-only
    /// indicator (it is informational; the authoritative cost stays on the server).
    /// </para>
    /// </summary>
    public sealed class StoreStockProductDto
    {
        public int productId { get; set; }
        public string productName { get; set; } = string.Empty;
        public string productCode { get; set; } = string.Empty;
        public decimal availableQuantity { get; set; }
        public decimal avgCost { get; set; }
    }

}
