using iTextSharp.text;
using iTextSharp.text.pdf;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using DocDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using PdfDocument = iTextSharp.text.Document;
using DocParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using PdfParagraph = iTextSharp.text.Paragraph;

namespace MarkdownEditor.Services
{
    public class ExportService : IExportService
    {
        public byte[] ExportToPdf(string html, string title)
        {
            using var stream = new MemoryStream();
            var document = new PdfDocument(iTextSharp.text.PageSize.A4, 50, 50, 25, 25);
            var writer = PdfWriter.GetInstance(document, stream);
            
            document.Open();
            
            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var titleParagraph = new PdfParagraph(title, titleFont);
            titleParagraph.Alignment = Element.ALIGN_CENTER;
            document.Add(titleParagraph);
            document.Add(new PdfParagraph("\n"));
            
            // Add content as simple text (HTML parsing with iTextSharp is complex)
            var contentParagraph = new PdfParagraph(html.Replace("<p>", "").Replace("</p>", "\n").Replace("<br>", "\n"));
            document.Add(contentParagraph);
            
            document.Close();
            return stream.ToArray();
        }

        public byte[] ExportToWord(string markdown, string title)
        {
            using var stream = new MemoryStream();
            using var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
            
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new DocDocument();
            var body = mainPart.Document.AppendChild(new Body());

            // Add title
            var titleParagraph = body.AppendChild(new DocParagraph());
            var titleRun = titleParagraph.AppendChild(new Run());
            var titleRunProperties = titleRun.AppendChild(new RunProperties());
            titleRunProperties.AppendChild(new Bold());
            titleRunProperties.AppendChild(new FontSize() { Val = "24" });
            titleRun.AppendChild(new Text(title));

            // Add content (simplified - just add as plain text for now)
            var contentParagraph = body.AppendChild(new DocParagraph());
            var contentRun = contentParagraph.AppendChild(new Run());
            contentRun.AppendChild(new Text(markdown));

            mainPart.Document.Save();
            
            return stream.ToArray();
        }
    }
} 