using Application.DTOs;
using DocumentFormat.OpenXml.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helper
{
    public class PurchaseInvoicePdfDocument : IDocument
    {
        private readonly PurchaseInvoiceDtos _invoice;
        private readonly byte[] logo;
        private readonly bool _isSimple;

        public PurchaseInvoicePdfDocument(PurchaseInvoiceDtos invoice, byte[] logo, bool isSimple = false)
        {
            _invoice = invoice;
            this.logo = MakeWatermark(logo, .6f);
            _isSimple = isSimple;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        //public void Compose(IDocumentContainer container)
        //{
        //    container.Page(page =>
        //    {
        //        page.Margin(20);

        //        page.Layers(layers =>
        //        {
        //            // ================= WATERMARK LAYER =================
        //            layers.Layer()
        //                .AlignCenter()
        //                .AlignMiddle()
        //                .Rotate(15)
        //                .Image(logo)
        //                .FitArea()
        //                .Opacity(0.05f); // 👈 هنا الشفافية

        //            // ================= CONTENT LAYER =================
        //            layers.PrimaryLayer().ContentFromRightToLeft().Column(col =>
        //            {
        //                // HEADER
        //                col.Item()
        //                    .AlignCenter()
        //                    .Text($"فاتورة شراء رقم {_invoice.invoiceNumber}")
        //                    .FontSize(18)
        //                    .Bold();

        //                col.Spacing(10);

        //                // INFO
        //                col.Item().AlignRight().Column(info =>
        //                {
        //                    info.Item().Text($"المورد: {_invoice.supplierName}");
        //                    info.Item().Text($"التاريخ: {_invoice.createdAt:yyyy-MM-dd}");
        //                });

        //                col.Spacing(10);

        //                // TABLE
        //                col.Item().Table(table =>
        //                {
        //                    table.ColumnsDefinition(c =>
        //                    {
        //                        c.RelativeColumn();
        //                        c.ConstantColumn(60);

        //                        if (!_isSimple)
        //                        {
        //                            c.ConstantColumn(80);
        //                            c.ConstantColumn(80);
        //                            c.ConstantColumn(100);
        //                            col.Item().EnsureSpace();
        //                        }
        //                    });

        //                    // HEADER
        //                    table.Header(h =>
        //                    {
        //                        h.Cell().AlignRight().Text("الصنف").Bold();
        //                        h.Cell().AlignRight().Text("الكمية").Bold();

        //                        if (!_isSimple)
        //                        {
        //                            h.Cell().AlignRight().Text("سعر الوحدة").Bold();
        //                            h.Cell().AlignRight().Text("الخصم").Bold();
        //                            h.Cell().AlignRight().Text("الإجمالي").Bold();
        //                        }
        //                    });

        //                    // ROWS
        //                    foreach (var item in _invoice.items)
        //                    {
        //                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        //                        table.Cell().AlignRight().Text(item.productName);
        //                        table.Cell().AlignRight().Text(item.quantity.ToString());

        //                        if (!_isSimple)
        //                        {
        //                            table.Cell().AlignRight().Text((item.buyingPricePerUnit ?? 0m).ToString("0.00"));
        //                            table.Cell().AlignRight().Text((item.totalRivalValue ?? 0m).ToString("0.00"));
        //                            table.Cell().AlignRight().Text((item.totalNetAmount ?? 0m).ToString("0.00"));
        //                        }
        //                    }
        //                });

        //                // SUMMARY
        //                if (!_isSimple)
        //                {
        //                    col.Item()
        //                        .AlignRight()
        //                        .PaddingTop(10)
        //                        .Text($"الإجمالي النهائي: {_invoice.totalNetAmount:0.00}")
        //                        .Bold()
        //                        .FontSize(14);
        //                }
        //            });
        //        });
        //    });
        //}
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(25);

                // Fix: Use page.Content() to get IContainer, then call Layers on it
                page.Content().Layers(layers =>
                {
                    layers.Layer()
                        .AlignCenter()
                        .AlignMiddle()
                        .Rotate(15)
                        .Image(logo)
                        .FitArea();


                    // ================= CONTENT =================
                    layers.PrimaryLayer().ContentFromRightToLeft().Column(col =>
                    {
                        // ===== HEADER =====
                        // ===== HEADER TITLE (CENTER) =====
                        col.Item()
                            .AlignCenter()
                            .Text($"فاتورة شراء {_invoice.invoiceNumber}")
                            .FontSize(18)
                            .Bold();

                        col.Spacing(10);

                        // ===== INFO (RIGHT SIDE) =====
                        col.Item()
                            .AlignRight()
                            .Column(info =>
                            {
                                info.Spacing(3);

                                info.Item().Row(r =>
                                {
                                    r.ConstantItem(100).Text("التاريخ:").Bold();
                                    r.RelativeItem().Text($"{_invoice.createdAt:yyyy-MM-dd}");
                                });

                                info.Item().Row(r =>
                                {
                                    r.ConstantItem(100).Text("المورد:").Bold();
                                    r.RelativeItem().Text(_invoice.supplierName);
                                });

                                info.Item().Row(r =>
                                {
                                    r.ConstantItem(100).Text("مخزن التسكين:").Bold();
                                    r.RelativeItem().Text(_invoice.settledStoreName);
                                });
                            });

                        col.Spacing(15);

                        // ===== TABLE =====
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); // الصنف
                                c.RelativeColumn(1); // الكمية

                                if (!_isSimple)
                                {
                                    c.RelativeColumn(1.5f);
                                    c.RelativeColumn(1.5f);
                                    c.RelativeColumn(2);
                                }
                                if (_isSimple)
                                {
                                    c.RelativeColumn(1); // 👈 الكمية المستلمة
                                    c.ConstantColumn(30); // 👈 checkbox
                                }
                            });

                            // ===== HEADER STYLE =====
                            table.Header(h =>
                            {
                                h.Cell().Element(HeaderCell).Text("الصنف");
                                h.Cell().Element(HeaderCell).Text("الكمية");

                                if (!_isSimple)
                                {
                                    h.Cell().Element(HeaderCell).Text("سعر الوحدة");
                                    h.Cell().Element(HeaderCell).Text("الخصم");
                                    h.Cell().Element(HeaderCell).Text("الإجمالي");
                                }
                                if (_isSimple)
                                {
                                    h.Cell().Element(HeaderCell).Text("الكمية المستلمة"); // 👈 جديد

                                    h.Cell().Element(HeaderCell).Text(""); // 👈 checkb
                                }
                            });

                            // ===== ROWS =====
                            int index = 0;

                            foreach (var item in _invoice.items)
                            {
                                bool isEven = index % 2 == 0;

                                table.Cell().Element(c => BodyCell(c, isEven)).Text(item.productName);
                                table.Cell().Element(c => BodyCell(c, isEven)).Text(item.quantity.ToString());

                                if (!_isSimple)
                                {
                                    table.Cell().Element(c => BodyCell(c, isEven)).Text((item.buyingPricePerUnit ?? 0m).ToString("0.00"));
                                    table.Cell().Element(c => BodyCell(c, isEven)).Text((item.totalRivalValue ?? 0m).ToString("0.00"));
                                    table.Cell().Element(c => BodyCell(c, isEven)).Text((item.totalNetAmount ?? 0m).ToString("0.00"));
                                }
                                if (_isSimple)
                                {
                                    // 👇 الكمية المستلمة (فاضية)
                                    table.Cell().Element(c => BodyCell(c, isEven)).Text("");

                                    // 👇 checkbox
                                    table.Cell().Element(c => BodyCell(c, isEven)).Element(DrawCheckBox);
                                }
                                index++;
                            }
                        });

                        // ===== SUMMARY BOXES =====
                        if (!_isSimple)
                        {
                            col.Item().PaddingTop(20).Row(row =>
                            {
                                row.Spacing(10);

                                row.RelativeItem().Background("#eee8dd").Padding(10).Column(c =>
                                {
                                    c.Item().Text("الضريبة:").Bold();
                                    c.Item().AlignRight().Text($"{_invoice.taxValue:0.00}");
                                });

                                row.RelativeItem().Background("#eee8dd").Padding(10).Column(c =>
                                {
                                    c.Item().Text("قيمة الخصم:").Bold();
                                    c.Item().AlignRight().Text($"{_invoice.rivalValue:0.00}");
                                });

                                row.RelativeItem().Background("#eee8dd").Padding(10).Column(c =>
                                {
                                    c.Item().Text("نسبة الخصم:").Bold();
                                    c.Item().AlignRight().Text($"{_invoice.precentageRival:0.##} %");
                                });

                                row.RelativeItem().Background("#eee8dd").Padding(10).Column(c =>
                                {
                                    c.Item().Text("إجمالي قبل الخصم:").Bold();
                                    c.Item().AlignRight().Text($"{_invoice.totalGrowthAmount:0.00}");
                                });
                            });

                            // ===== FINAL TOTAL =====
                            col.Item().AlignRight().PaddingTop(10)
                                .Background("#eee8dd")
                                .Padding(10)
                                .Width(200)
                                .Column(c =>
                                {
                                    c.Item().Text("الإجمالي النهائي:").Bold();
                                    c.Item().AlignRight().Text($"{_invoice.totalNetAmount:0.00}").Bold();
                                });
                        }
                    });
                });
            });
        }

        // ================= CELL STYLES =================


public byte[] MakeWatermark(byte[] imageBytes, float opacity = 0.05f)
    {
        using var input = new MemoryStream(imageBytes);
        using var original = System.Drawing.Image.FromStream(input);

        using var bmp = new Bitmap(original.Width, original.Height);

        using (var g = Graphics.FromImage(bmp))
        {
            var matrix = new ColorMatrix
            {
                Matrix33 = opacity // الشفافية
            };

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.DrawImage(original,
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                0, 0, original.Width, original.Height,
                GraphicsUnit.Pixel,
                attributes);
        }

        using var output = new MemoryStream();
        bmp.Save(output, System.Drawing.Imaging.ImageFormat.Png);

        return output.ToArray();
    }

    IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(5)
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(x => x.Bold().FontSize(11));
        }

        IContainer BodyCell(IContainer container, bool isEven)
        {
            return container
                .Background(isEven ? Colors.White : Colors.Grey.Lighten4)
                .Padding(5)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(x => x.FontSize(10));
        }
        IContainer DrawCheckBox(IContainer container)
        {
            return container
                .Width(15)
                .Height(15)
                .Border(1.5f)
                .BorderColor(Colors.Black)
                .AlignCenter()
                .AlignMiddle();
        }
    }
}
