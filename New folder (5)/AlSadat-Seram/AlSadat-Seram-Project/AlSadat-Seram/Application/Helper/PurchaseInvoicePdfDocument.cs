using Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helper
{
    public class PurchaseInvoicePdfDocument : IDocument
    {
        private readonly PurchaseInvoiceDtos _invoice;
        private readonly bool _isSimple;

        public PurchaseInvoicePdfDocument(PurchaseInvoiceDtos invoice, bool isSimple)
        {
            _invoice = invoice;
            _isSimple = isSimple;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Background(Colors.White);

                page.Content().Column(col =>
                {
                    // HEADER
                    col.Item().Text($"فاتورة شراء رقم {_invoice.invoiceNumber}")
                        .FontSize(18).Bold();

                    col.Item().Text($"المورد: {_invoice.supplierName}");
                    col.Item().Text($"التاريخ: {_invoice.createdAt}");

                    col.Spacing(10);

                    // TABLE
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(); // product
                            c.ConstantColumn(60); // qty

                            if (!_isSimple)
                            {
                                c.ConstantColumn(80); // price
                                c.ConstantColumn(80); // discount
                                c.ConstantColumn(100); // total
                            }
                        });

                        // HEADER
                        table.Header(h =>
                        {
                            h.Cell().Text("الصنف").Bold();
                            h.Cell().Text("الكمية").Bold();

                            if (!_isSimple)
                            {
                                h.Cell().Text("سعر الوحدة").Bold();
                                h.Cell().Text("الخصم").Bold();
                                h.Cell().Text("الإجمالي").Bold();
                            }
                        });

                        // ROWS
                        foreach (var item in _invoice.items)
                        {
                            table.Cell().Text(item.productName);
                            table.Cell().Text(item.quantity.ToString());

                            if (!_isSimple)
                            {
                                table.Cell().Text((item.buyingPricePerUnit ?? 0m).ToString("0.00"));
                                table.Cell().Text((item.totalRivalValue ?? 0m).ToString("0.00"));
                                table.Cell().Text((item.totalNetAmount ?? 0m).ToString("0.00"));
                            }
                        }
                    });

                    // SUMMARY (only full)
                    if (!_isSimple)
                    {
                        col.Item().PaddingTop(10).Text($"الإجمالي النهائي: {_invoice.totalNetAmount}")
                            .Bold().FontSize(14);
                    }
                });
            });
        }
    }
}
