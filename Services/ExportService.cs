using iTextSharp.text;
using iTextSharp.text.pdf;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using System.Text.RegularExpressions;
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
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.Black);
            var titleParagraph = new PdfParagraph(title, titleFont);
            titleParagraph.Alignment = Element.ALIGN_CENTER;
            titleParagraph.SpacingAfter = 20;
            document.Add(titleParagraph);
            
            // Process HTML content
            ProcessHtmlToPdf(document, html);
            
            document.Close();
            return stream.ToArray();
        }

        private void ProcessHtmlToPdf(PdfDocument document, string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return;

            // 簡單的HTML解析和轉換
            var cleanText = html
                .Replace("<p>", "")
                .Replace("</p>", "\n\n")
                .Replace("<br>", "\n")
                .Replace("<br/>", "\n")
                .Replace("<br />", "\n")
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"");

            // 處理標題
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<h1[^>]*>(.*?)</h1>", 
                m => "\n" + m.Groups[1].Value + "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<h2[^>]*>(.*?)</h2>", 
                m => "\n" + m.Groups[1].Value + "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<h3[^>]*>(.*?)</h3>", 
                m => "\n" + m.Groups[1].Value + "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // 處理粗體
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<strong[^>]*>(.*?)</strong>", 
                "$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<b[^>]*>(.*?)</b>", 
                "$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // 處理斜體
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<em[^>]*>(.*?)</em>", 
                "$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<i[^>]*>(.*?)</i>", 
                "$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // 處理列表
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<ul[^>]*>", 
                "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"</ul>", 
                "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<li[^>]*>(.*?)</li>", 
                "• $1\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // 移除其他HTML標籤
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<[^>]+>", "");

            // 分段處理
            var paragraphs = cleanText.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.Black);
            
            foreach (var para in paragraphs)
            {
                var trimmedPara = para.Trim();
                if (!string.IsNullOrEmpty(trimmedPara))
                {
                    var paragraph = new PdfParagraph(trimmedPara, normalFont);
                    paragraph.SpacingAfter = 10;
                    document.Add(paragraph);
                }
            }
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
            titleRunProperties.AppendChild(new FontSize() { Val = "32" });
            titleRun.AppendChild(new Text(title));

            // Add empty line
            body.AppendChild(new DocParagraph());

            // Process markdown content
            ProcessMarkdownToWord(body, markdown);

            mainPart.Document.Save();
            
            return stream.ToArray();
        }

        private void ProcessMarkdownToWord(Body body, string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return;

            var lines = markdown.Split('\n');
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    // 空行
                    body.AppendChild(new DocParagraph());
                    continue;
                }
                
                var paragraph = body.AppendChild(new DocParagraph());
                
                if (trimmedLine.StartsWith("# "))
                {
                    // H1 標題
                    var run = paragraph.AppendChild(new Run());
                    var runProperties = run.AppendChild(new RunProperties());
                    runProperties.AppendChild(new Bold());
                    runProperties.AppendChild(new FontSize() { Val = "28" });
                    run.AppendChild(new Text(trimmedLine.Substring(2)));
                }
                else if (trimmedLine.StartsWith("## "))
                {
                    // H2 標題
                    var run = paragraph.AppendChild(new Run());
                    var runProperties = run.AppendChild(new RunProperties());
                    runProperties.AppendChild(new Bold());
                    runProperties.AppendChild(new FontSize() { Val = "24" });
                    run.AppendChild(new Text(trimmedLine.Substring(3)));
                }
                else if (trimmedLine.StartsWith("### "))
                {
                    // H3 標題
                    var run = paragraph.AppendChild(new Run());
                    var runProperties = run.AppendChild(new RunProperties());
                    runProperties.AppendChild(new Bold());
                    runProperties.AppendChild(new FontSize() { Val = "20" });
                    run.AppendChild(new Text(trimmedLine.Substring(4)));
                }
                else if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                {
                    // 無序列表
                    var run = paragraph.AppendChild(new Run());
                    run.AppendChild(new Text("• " + trimmedLine.Substring(2)));
                }
                else if (trimmedLine.StartsWith("> "))
                {
                    // 引用塊
                    var run = paragraph.AppendChild(new Run());
                    var runProperties = run.AppendChild(new RunProperties());
                    runProperties.AppendChild(new Italic());
                    run.AppendChild(new Text(trimmedLine.Substring(2)));
                }
                else
                {
                    // 普通段落
                    ProcessTextWithFormatting(paragraph, trimmedLine);
                }
            }
        }

        private void ProcessTextWithFormatting(DocParagraph paragraph, string text)
        {
            // 處理粗體 **text** 和斜體 *text*
            var parts = System.Text.RegularExpressions.Regex.Split(text, @"(\*\*.*?\*\*|\*.*?\*)");
            
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                
                var run = paragraph.AppendChild(new Run());
                
                if (part.StartsWith("**") && part.EndsWith("**") && part.Length > 4)
                {
                    // 粗體
                    var runProperties = run.AppendChild(new RunProperties());
                    runProperties.AppendChild(new Bold());
                    run.AppendChild(new Text(part.Substring(2, part.Length - 4)));
                }
                else if (part.StartsWith("*") && part.EndsWith("*") && part.Length > 2)
                {
                    // 斜體
                    var runProperties = run.AppendChild(new RunProperties());
                    runProperties.AppendChild(new Italic());
                    run.AppendChild(new Text(part.Substring(1, part.Length - 2)));
                }
                else
                {
                    // 普通文字
                    run.AppendChild(new Text(part));
                }
            }
        }
    }
} 