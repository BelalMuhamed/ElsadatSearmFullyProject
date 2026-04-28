using Application.DTOs.SalesInvoices;
using Application.Helper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ConfirmedSalesInvoicePdfDocument
    : BaseInvoicePdfDocument<SalesInvoiceDetails>
{
    private readonly bool _isSimple;
    public ConfirmedSalesInvoicePdfDocument(
        SalesInvoiceDetails invoice,
        byte[] logo, bool isSimple )
        : base(invoice, logo)
    {
        _isSimple = isSimple;
    }

    protected override void ComposeContent(IContainer container)
    {
        container.ContentFromRightToLeft().Column(col =>
        {
            col.Spacing(10);

            ComposeHeader(col);
            if(!_isSimple)
                ComposeSummary(col);

            ComposeTable(col, _isSimple);
        });
    }

    private void ComposeTable(ColumnDescriptor col,bool isSimple)
    {
        col.Item().Table(table =>
        {
            // ✅ UPDATED: added new columns
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3); // الصنف
                c.RelativeColumn(1); // الكمية

                if (isSimple)
                {
                    c.RelativeColumn(2); // الكمية المجهزة
                    c.RelativeColumn(2); // المخزن المصدر
                }
                if(!isSimple)
                {
                    c.RelativeColumn(1); // السعر
                    c.RelativeColumn(1); // الخصم
                    c.RelativeColumn(1); // الصافي
                }
                
            });

            table.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("الصنف");
                h.Cell().Element(HeaderCell).Text("الكمية");
                if(isSimple)
                {
                    h.Cell().Element(HeaderCell).Text("الكمية التي تم تجهيزها");
                    h.Cell().Element(HeaderCell).Text("المخزن المصدر");
                }

                if (!isSimple)
                {
                    // ✅ NEW HEADERS
                    h.Cell().Element(HeaderCell).Text("السعر");
                    h.Cell().Element(HeaderCell).Text("إجمالي الخصم");
                    h.Cell().Element(HeaderCell).Text("صافي المنتج");
                }
            });

            int i = 0;

            foreach (var item in _invoice.WithdrwanStock)
            {
                bool even = i % 2 == 0;

                table.Cell().Element(c => BodyCell(c, even)).Text(item.productName);
                table.Cell().Element(c => BodyCell(c, even)).Text(item.quantity.ToString());
                if(isSimple)
                {
                    // 👇 empty as requested
                    table.Cell().Element(c => BodyCell(c, even)).Text(item.quantity.ToString());

                    table.Cell()
     .Element(c => BodyCell(c, even))
     .Text(string.Join(", ", item.WithdrwanStock.Select(x => x.storeName)));

                }
                if (!isSimple)
                {

                    // ✅ NEW DATA CELLS
                    table.Cell().Element(c => BodyCell(c, even))
                    .Text(item.sellingPrice.ToString("0.00"));

                    table.Cell().Element(c => BodyCell(c, even))
                        .Text(item.totalRivalValue?.ToString("0.00") ?? "0.00");

                    table.Cell().Element(c => BodyCell(c, even))
                        .Text(item.totalNetAmount?.ToString("0.00") ?? "0.00");
                }

                i++;
            }
        });
    }

    private void ComposeSummary(ColumnDescriptor col)
    {
        col.Item().PaddingVertical(10).Column(summary =>
        {
            summary.Spacing(5);

            summary.Item().AlignRight().Text($"عدد النقاط: {_invoice.totalPoints}");
            summary.Item().AlignRight().Text($"خصم أول: {_invoice.firstDiscount:0.00}");
            summary.Item().AlignRight().Text($"خصم ثاني: {_invoice.secondDiscount:0.00}");
            summary.Item().AlignRight().Text($"خصم ثالث: {_invoice.thirdDiscount:0.00}");
            summary.Item().AlignRight().Text($"الضريبة: {_invoice.taxValue:0.00}");
            summary.Item().AlignRight().Text($"الإجمالي الصافي: {_invoice.totalNetAmount:0.00}");
        });
    }

    protected void ComposeHeader(ColumnDescriptor col)
    {
        col.Item().AlignCenter()
            .Text($"فاتورة مبيعات {_invoice.invoiceNumber}")
            .FontSize(18).Bold();

        col.Item().AlignRight().Text($"التاريخ: {_invoice.createdAt:yyyy-MM-dd}");
        col.Item().AlignRight().Text($"العميل: {_invoice.distributorName}");
    }

    // ✅ Reuse styles (in case not موجودة في Base)
    private IContainer HeaderCell(IContainer c) => c
        .Background(Colors.Grey.Lighten2)
        .Padding(5)
        .AlignCenter()
        .DefaultTextStyle(x => x.Bold());

    private IContainer BodyCell(IContainer c, bool even) => c
        .Background(even ? Colors.White : Colors.Grey.Lighten4)
        .Padding(5)
        .AlignCenter();
}