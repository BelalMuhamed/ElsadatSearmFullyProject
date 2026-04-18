using Domain.Entities;
using Domain.Entities.Finance;
using Domain.Entities.Invoices;
using Domain.Entities.Users;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DTOs.SalesInvoices
{
    public class SalesInvoicesResponse
    {
        public int id { get; set; }
        public int? totalCopouns { get; set; }
        public string distributorId { get; set; }
        public string? distributorName { get; set; } = string.Empty;
        public float? firstDiscount { get; set; }
        public float? secondDiscount { get; set; }
        public float? thirdDiscount { get; set; }
        public string? invoiceNumber { get; set; }
        public int? totalPoints { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public string createdBy { get; set; }
        public int? salesInvoiceStatus { get; set; }
        public int? deleteStatus { get; set; }
        public string? updateBy { get; set; }
        public DateTime? updateAt { get; set; }
        public decimal? totalGrowthAmount { get; set; }
        public decimal? totalNetAmount { get; set; }
        public decimal? taxPrecentage { get; set; }
        public decimal? taxValue { get; set; }
        public int? reverseJournalEntry { get; set; }
        public List<salesInvoiceItemsResp> items { get; set; } 

    }
    public class salesInvoiceItemsResp
    {
        public int? id { get; set; }
        public decimal sellingPrice { get; set; }
        public int quantity { get; set; }
        public int productID { get; set; }
        public string? productName { get; set; }
        public decimal? precentageRival { get; set; }
        public decimal? rivalValue { get; set; }
        public decimal? totalRivalValue { get; set; }
        public decimal? totalGrowthAmount { get; set; }
        public decimal? totalNetAmount { get; set; }
    }
    public class SalesInvoiceFilters
    {
        public int? page { get; set; }
        public int? pageSize { get; set; }
        public string? invoiceNumber { get; set; }
        public string? customerId { get; set; }
        public DateTime? createAt { get; set; }
      
        public int? deleteStatus { get; set; }
    }
    public class InvoiceChangeStatusReq
    {
        public int id { get; set; }
        public int? salesInvoiceStatus { get; set; }
        public int? deleteStatus { get; set; }
        public string ?  updateBy { get; set; }
    }
    public class invoiceConfirmationProductsStock
    {
        public int? invoiceId { get; set; }
        public string? updateBy { get; set; }
        public List<ProductStockDto> withdrwanItemsQuantities { get; set; }
    }
    public class SalesInvoiceDetails
    {
        public int id { get; set; }
        public int? totalCopouns { get; set; }
        public string distributorId { get; set; }
        public string? distributorName { get; set; } = string.Empty;
        public float? firstDiscount { get; set; }
        public float? secondDiscount { get; set; }
        public float? thirdDiscount { get; set; }
        public string? invoiceNumber { get; set; }
        public int? totalPoints { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public string createdBy { get; set; }
        public int? salesInvoiceStatus { get; set; }
        public int? deleteStatus { get; set; }
        public string? updateBy { get; set; }
        public DateTime? updateAt { get; set; }
        public decimal? totalGrowthAmount { get; set; }
        public decimal? totalNetAmount { get; set; }
        public decimal? taxPrecentage { get; set; }
        public decimal? taxValue { get; set; }

       public List<salesInvoiceItemsDetails> WithdrwanStock { get; set; } = new List<salesInvoiceItemsDetails>();
    }
    public class salesInvoiceItemsDetails
    {
        public int? id { get; set; }
        public decimal sellingPrice { get; set; }
        public int quantity { get; set; }
        public int productID { get; set; }
        public string? productName { get; set; }
        public decimal? precentageRival { get; set; }
        public decimal? rivalValue { get; set; }
        public decimal? totalRivalValue { get; set; }
        public decimal? totalGrowthAmount { get; set; }
        public decimal? totalNetAmount { get; set; }
      public  List<ProductStockPerStoreDto> WithdrwanStock { get; set; }=new List<ProductStockPerStoreDto>();
    }

}
