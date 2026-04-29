using System;

namespace Application.DTOs.ProductsDtos
{
    /// <summary>
    /// DTO used by Application/Presentation layers for product CRUD.
    /// NOTE: <c>theHighestPossibleQuantity</c> has been removed — it is no longer
    /// part of the business model. Only the minimum threshold
    /// (<see cref="theSmallestPossibleQuantity"/>) is retained for low-stock alerts.
    /// </summary>
    public class ProductDto
    {
        public int? id { get; set; }
        public string name { get; set; } = string.Empty;
        public string productCode { get; set; } = string.Empty;
        public decimal sellingPrice { get; set; }
        public int pointPerUnit { get; set; }

        public string? createBy { get; set; }
        public DateTime? createAt { get; set; } = DateTime.UtcNow;
        public string? updateBy { get; set; }
        public DateTime? updateAt { get; set; }
        public bool isDeleted { get; set; }
        public string? deleteBy { get; set; }
        public DateTime? deleteAt { get; set; }

        /// <summary>Reorder threshold — quantities at or below this trigger a low-stock alert.</summary>
        public int theSmallestPossibleQuantity { get; set; }

        public int categoryId { get; set; }
        public string? categoryName { get; set; }
    }
}