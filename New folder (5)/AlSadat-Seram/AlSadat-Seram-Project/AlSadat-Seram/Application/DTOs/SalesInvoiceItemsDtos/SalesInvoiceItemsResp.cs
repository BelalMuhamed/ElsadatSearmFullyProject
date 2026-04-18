namespace AlSadatSeram.Services.contract.SalesInvoiceItemsDD.Dtos
{
    public class SalesInvoiceItemsResp
    {
        public int? id { get; set; }
        public decimal sellingPrice { get; set; }
        public int quantity { get; set; }
        public int pointEarned { get; set; }

        public int? productId { get; set; }
        public  string productName { get; set; }

        public decimal discountPerItem { get; set; }

    }
}
