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
using PdfFont = iTextSharp.text.Font;
using DocFont = DocumentFormat.OpenXml.Wordprocessing.Font;

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
            
            // 建立支援中文的字體
            var chineseFont = GetChineseFont();
            var titleFont = GetChineseTitleFont();
            
            // Add title
            var titleParagraph = new PdfParagraph(title, titleFont);
            titleParagraph.Alignment = Element.ALIGN_CENTER;
            titleParagraph.SpacingAfter = 20;
            document.Add(titleParagraph);
            
            // Process HTML content
            ProcessHtmlToPdf(document, html, chineseFont);
            
            document.Close();
            return stream.ToArray();
        }

        private BaseFont GetChineseBaseFont()
        {
            try
            {
                return BaseFont.CreateFont("STSong-Light", "UniGB-UCS2-H", BaseFont.NOT_EMBEDDED);
            }
            catch
            {
                try
                {
                    return BaseFont.CreateFont("c:/windows/fonts/simsun.ttc,0", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                }
                catch
                {
                    return BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                }
            }
        }

        private PdfFont GetChineseFont()
        {
            var baseFont = GetChineseBaseFont();
            return new PdfFont(baseFont, 12, PdfFont.NORMAL, BaseColor.Black);
        }

        private PdfFont GetChineseTitleFont()
        {
            var baseFont = GetChineseBaseFont();
            return new PdfFont(baseFont, 18, PdfFont.BOLD, BaseColor.Black);
        }

        private PdfFont GetChineseBoldFont()
        {
            var baseFont = GetChineseBaseFont();
            return new PdfFont(baseFont, 12, PdfFont.BOLD, BaseColor.Black);
        }

        private PdfFont GetChineseHeaderFont(int level)
        {
            var baseFont = GetChineseBaseFont();
            int size = level switch
            {
                1 => 16,
                2 => 14,
                3 => 13,
                _ => 12
            };
            return new PdfFont(baseFont, size, PdfFont.BOLD, BaseColor.Black);
        }

        private void ProcessHtmlToPdf(PdfDocument document, string html, PdfFont normalFont)
        {
            if (string.IsNullOrWhiteSpace(html))
                return;

            // 轉換換行符為統一格式
            html = html.Replace("\r\n", "\n").Replace("\r", "\n");

            // 處理標題
            var h1Pattern = @"<h1[^>]*>(.*?)</h1>";
            var h2Pattern = @"<h2[^>]*>(.*?)</h2>";
            var h3Pattern = @"<h3[^>]*>(.*?)</h3>";
            
            html = Regex.Replace(html, h1Pattern, m => 
                $"\n[H1]{System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}[/H1]\n", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, h2Pattern, m => 
                $"\n[H2]{System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}[/H2]\n", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, h3Pattern, m => 
                $"\n[H3]{System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}[/H3]\n", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 處理粗體
            var boldPattern = @"<(strong|b)[^>]*>(.*?)</(strong|b)>";
            html = Regex.Replace(html, boldPattern, m => 
                $"[BOLD]{System.Web.HttpUtility.HtmlDecode(m.Groups[2].Value)}[/BOLD]", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 處理斜體
            var italicPattern = @"<(em|i)[^>]*>(.*?)</(em|i)>";
            html = Regex.Replace(html, italicPattern, m => 
                $"[ITALIC]{System.Web.HttpUtility.HtmlDecode(m.Groups[2].Value)}[/ITALIC]", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 處理列表項
            var listItemPattern = @"<li[^>]*>(.*?)</li>";
            html = Regex.Replace(html, listItemPattern, m => 
                $"• {System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}\n", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 清理 HTML 標籤
            html = Regex.Replace(html, @"<[^>]+>", "");

            // 解碼 HTML 實體
            html = System.Web.HttpUtility.HtmlDecode(html);

            // 處理段落
            var paragraphs = html.Split(new string[] { "\n\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var para in paragraphs)
            {
                var trimmedPara = para.Trim();
                if (string.IsNullOrEmpty(trimmedPara)) continue;

                // 處理標題
                if (trimmedPara.StartsWith("[H1]") && trimmedPara.EndsWith("[/H1]"))
                {
                    var headerText = trimmedPara.Substring(4, trimmedPara.Length - 9);
                    var headerParagraph = new PdfParagraph(headerText, GetChineseHeaderFont(1));
                    headerParagraph.SpacingAfter = 12;
                    headerParagraph.SpacingBefore = 16;
                    document.Add(headerParagraph);
                }
                else if (trimmedPara.StartsWith("[H2]") && trimmedPara.EndsWith("[/H2]"))
                {
                    var headerText = trimmedPara.Substring(4, trimmedPara.Length - 9);
                    var headerParagraph = new PdfParagraph(headerText, GetChineseHeaderFont(2));
                    headerParagraph.SpacingAfter = 10;
                    headerParagraph.SpacingBefore = 14;
                    document.Add(headerParagraph);
                }
                else if (trimmedPara.StartsWith("[H3]") && trimmedPara.EndsWith("[/H3]"))
                {
                    var headerText = trimmedPara.Substring(4, trimmedPara.Length - 9);
                    var headerParagraph = new PdfParagraph(headerText, GetChineseHeaderFont(3));
                    headerParagraph.SpacingAfter = 8;
                    headerParagraph.SpacingBefore = 12;
                    document.Add(headerParagraph);
                }
                else
                {
                    // 處理帶格式的文字段落
                    var paragraph = new PdfParagraph();
                    ProcessFormattedText(paragraph, trimmedPara);
                    paragraph.SpacingAfter = 8;
                    document.Add(paragraph);
                }
            }
        }

        private void ProcessFormattedText(PdfParagraph paragraph, string text)
        {
            var parts = Regex.Split(text, @"(\[BOLD\].*?\[/BOLD\]|\[ITALIC\].*?\[/ITALIC\])");
            
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                if (part.StartsWith("[BOLD]") && part.EndsWith("[/BOLD]"))
                {
                    var boldText = part.Substring(6, part.Length - 13);
                    var chunk = new Chunk(boldText, GetChineseBoldFont());
                    paragraph.Add(chunk);
                }
                else if (part.StartsWith("[ITALIC]") && part.EndsWith("[/ITALIC]"))
                {
                    var italicText = part.Substring(8, part.Length - 17);
                    var baseFont = GetChineseBaseFont();
                    var italicFont = new PdfFont(baseFont, 12, PdfFont.ITALIC, BaseColor.Black);
                    var chunk = new Chunk(italicText, italicFont);
                    paragraph.Add(chunk);
                }
                else
                {
                    var chunk = new Chunk(part, GetChineseFont());
                    paragraph.Add(chunk);
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

            // 將 \r\n 轉換為 \n
            markdown = markdown.Replace("\r\n", "\n").Replace("\r", "\n");

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
                    // 普通段落，處理內聯格式
                    ProcessTextWithFormatting(paragraph, trimmedLine);
                }
            }
        }

        private void ProcessTextWithFormatting(DocParagraph paragraph, string text)
        {
            // 處理粗體 **text** 和斜體 *text*
            var parts = Regex.Split(text, @"(\*\*.*?\*\*|\*.*?\*)");
            
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
                else if (part.StartsWith("*") && part.EndsWith("*") && part.Length > 2 && !part.StartsWith("**"))
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