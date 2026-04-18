
namespace Application.DTOs.SalesInvoices
{
    public class SalesInvoiceFilterations
    {
        public string invoiceNo { get; set; }
        public List<string>? dates { get; set; }
        public string? customerId { get; set; }
        public string? craetedBy { get; set; }
        public int? salesInvoiceType { get; set; }
        public int pageSize { get; set; }
        public int page { get; set; }
    }
}
