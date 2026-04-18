using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PurchaseInvoiceDtos
    {
        public int? id { get; set; }
        public DateTime? createdAt { get; set; }
        public string? createdBy { get; set; }
        public string? updatedBy { get; set; }
        public DateTime? updatedAt { get; set; }
        public decimal? totalGrowthAmount { get; set; }
        public decimal? totalNetAmount { get; set; }
        public string? invoiceNumber { get; set; }
        public int? supplierId { get; set; }
        public string? supplierName { get; set; }
        public int? settledStatus { get; set; }
        public int? deleteStatus { get; set; }
        public decimal? precentageRival { get; set; }
        public decimal? rivalValue { get; set; }
        public decimal? totalRivalValue { get; set; }
        public decimal? taxPrecentage { get; set; }
        public decimal? taxValue { get; set; }
        public int? settledStoreId { get; set; }
        public string? settledStoreName { get; set; }

        public List<PurchaseInvoiceItemsDtos> items { get; set; } = new List<PurchaseInvoiceItemsDtos>();

    }
    public class PurchaseInvoiceItemsDtos
    {
        public int? id { get; set; }
        public string? itemCode { get; set; }
        public decimal? buyingPricePerUnit { get; set; }
        public int? productId { get; set; }
        public string? productName { get; set; }
        public decimal quantity { get; set; }
        public int? purchaseInvoiceId { get; set; }
        public string? PurchaseInvoiceNumber { get; set; }
        public decimal? precentageRival { get; set; }
        public decimal? rivalValue { get; set; }
        public decimal? totalRivalValue { get; set; }

        public decimal? totalGrowthAmount { get; set; }
        public decimal? totalNetAmount { get; set; }
    }
    public class PurchaseInvoiceFilters
    {
        public int? page { get; set; }
        public int? pageSize { get; set; }
        public string? invoiceNumber { get; set; }
        public int? supplierId { get; set; }
        public int? settledStatus { get; set; }
        public int? deleteStatus { get; set; }
    }

    public class InvoicePdfRequest
    {
        public int Id { get; set; }
        public bool IsSimple { get; set; }
    }
}
