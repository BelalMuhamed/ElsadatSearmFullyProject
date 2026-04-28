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
    public abstract class BaseInvoicePdfDocument<T> : IDocument
    {
        protected readonly T _invoice;
        protected readonly byte[] _watermark;

        protected BaseInvoicePdfDocument(T invoice, byte[] logo)
        {
            _invoice = invoice;
            _watermark = MakeWatermark(logo, 0.6f);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        protected byte[] MakeWatermark(byte[] imageBytes, float opacity)
        {
            using var input = new MemoryStream(imageBytes);
            using var original = System.Drawing.Image.FromStream(input);

            using var bmp = new Bitmap(original.Width, original.Height);

            using (var g = Graphics.FromImage(bmp))
            {
                var matrix = new ColorMatrix { Matrix33 = opacity };

                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix);

                g.DrawImage(original,
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    0, 0, original.Width, original.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            using var output = new MemoryStream();
            bmp.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            return output.ToArray();
        }

        protected void ApplyWatermark(PageDescriptor page)
        {
            page.Content().Layers(layers =>
            {
                layers.Layer()
                    .AlignCenter()
                    .AlignMiddle()
                    .Rotate(15)
                    .Image(_watermark)
                    .FitArea();

                ComposeContent(layers.PrimaryLayer());
            });
        }

        protected abstract void ComposeContent(IContainer container);

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(25);
                ApplyWatermark(page); // 👈 this calls ComposeContent
            });
        }

        public IContainer HeaderCell(IContainer c) => c
           .Background(Colors.Grey.Lighten2)
           .Padding(5)
           .AlignCenter()
           .DefaultTextStyle(x => x.Bold());

        public IContainer BodyCell(IContainer c, bool even) => c
            .Background(even ? Colors.White : Colors.Grey.Lighten4)
            .Padding(5)
            .AlignCenter();
    }
}
