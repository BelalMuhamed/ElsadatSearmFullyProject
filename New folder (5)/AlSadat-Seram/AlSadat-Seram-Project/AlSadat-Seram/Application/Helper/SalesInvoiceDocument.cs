using Application.DTOs.SalesInvoices;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace Application.Helper
{
    public class SalesInvoicePdfDocument
     : BaseInvoicePdfDocument<SalesInvoicesResponse>
    {
        public SalesInvoicePdfDocument(SalesInvoicesResponse invoice, byte[] logo)
            : base(invoice, logo) { }

       

        protected override void ComposeContent(IContainer container)
        {
            container.ContentFromRightToLeft().Column(col =>
            {
                col.Spacing(10);

                // Header
                col.Item().AlignCenter()
                    .Text($"فاتورة مبيعات {_invoice.invoiceNumber}")
                    .FontSize(18).Bold();

                // Info
                col.Item().AlignRight().Text($"التاريخ: {_invoice.createdAt:yyyy-MM-dd}");
                col.Item().AlignRight().Text($"العميل: {_invoice.distributorName}");

                col.Spacing(15);

                // Table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); // الصنف
                        c.RelativeColumn(1); // الكمية
                        c.RelativeColumn(2); // الكمية المجهزة
                        c.RelativeColumn(2); // المخزن المصدر
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("الصنف");
                        h.Cell().Element(HeaderCell).Text("الكمية");
                        h.Cell().Element(HeaderCell).Text("الكمية التي تم تجهيزها");
                        h.Cell().Element(HeaderCell).Text("المخزن المصدر");
                    });

                    int i = 0;

                    foreach (var item in _invoice.items)
                    {
                        bool even = i % 2 == 0;

                        table.Cell().Element(c => BodyCell(c, even)).Text(item.productName);
                        table.Cell().Element(c => BodyCell(c, even)).Text(item.quantity.ToString());

                        // 👇 empty as requested
                        table.Cell().Element(c => BodyCell(c, even)).Text("");
                        table.Cell().Element(c => BodyCell(c, even)).Text("");

                        i++;
                    }
                });
            });
        }

        // reuse same styles
        //private IContainer HeaderCell(IContainer c) => c
        //    .Background(Colors.Grey.Lighten2)
        //    .Padding(5)
        //    .AlignCenter()
        //    .DefaultTextStyle(x => x.Bold());

        //private IContainer BodyCell(IContainer c, bool even) => c
        //    .Background(even ? Colors.White : Colors.Grey.Lighten4)
        //    .Padding(5)
        //    .AlignCenter();
    }
}