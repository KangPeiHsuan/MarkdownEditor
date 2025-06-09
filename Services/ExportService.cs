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
                4 => 12,
                _ => 11
            };
            return new PdfFont(baseFont, size, PdfFont.BOLD, BaseColor.Black);
        }

        private void ProcessHtmlToPdf(PdfDocument document, string html, PdfFont normalFont)
        {
            if (string.IsNullOrWhiteSpace(html))
                return;

            // 轉換換行符為統一格式
            html = html.Replace("\r\n", "\n").Replace("\r", "\n");

            // 處理標題（按層級順序，避免錯誤匹配）
            var h4Pattern = @"<h4[^>]*>(.*?)</h4>";
            var h3Pattern = @"<h3[^>]*>(.*?)</h3>";
            var h2Pattern = @"<h2[^>]*>(.*?)</h2>";
            var h1Pattern = @"<h1[^>]*>(.*?)</h1>";
            
            html = Regex.Replace(html, h4Pattern, m => 
                $"\n[H4]{System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}[/H4]\n", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, h3Pattern, m => 
                $"\n[H3]{System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}[/H3]\n", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, h2Pattern, m => 
                $"\n[H2]{System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}[/H2]\n", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, h1Pattern, m => 
                $"\n[H1]{System.Web.HttpUtility.HtmlDecode(m.Groups[1].Value)}[/H1]\n", 
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

            // 處理 Markdown 粗體語法 **text**
            var markdownBoldPattern = @"\*\*([^*]+)\*\*";
            html = Regex.Replace(html, markdownBoldPattern, m => 
                $"[BOLD]{m.Groups[1].Value}[/BOLD]", 
                RegexOptions.IgnoreCase);

            // 處理 Markdown 斜體語法 *text*
            var markdownItalicPattern = @"(?<!\*)\*([^*]+)\*(?!\*)";
            html = Regex.Replace(html, markdownItalicPattern, m => 
                $"[ITALIC]{m.Groups[1].Value}[/ITALIC]", 
                RegexOptions.IgnoreCase);

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
                else if (trimmedPara.StartsWith("[H4]") && trimmedPara.EndsWith("[/H4]"))
                {
                    var headerText = trimmedPara.Substring(4, trimmedPara.Length - 9);
                    var headerParagraph = new PdfParagraph(headerText, GetChineseHeaderFont(4));
                    headerParagraph.SpacingAfter = 6;
                    headerParagraph.SpacingBefore = 10;
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
            try
            {
                using var stream = new MemoryStream();
                
                // 創建 WordprocessingDocument，需要手動控制關閉時機
                var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
                
                // 創建主文檔部分
                var mainPart = wordDocument.AddMainDocumentPart();
                
                // 創建文檔結構
                var document = new DocDocument();
                var body = new Body();
                
                // 添加標題
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var titleParagraph = new DocParagraph();
                    var titleRun = new Run();
                    var titleRunProperties = new RunProperties();
                    titleRunProperties.Append(new Bold());
                    titleRunProperties.Append(new FontSize() { Val = "56" }); // 28pt = 56 半點
                    titleRun.Append(titleRunProperties);
                    titleRun.Append(new Text(title) { Space = SpaceProcessingModeValues.Preserve });
                    titleParagraph.Append(titleRun);
                    body.Append(titleParagraph);
                    
                    // 標題後添加空行
                    body.Append(new DocParagraph());
                }
                
                // 處理 Markdown 內容
                if (!string.IsNullOrWhiteSpace(markdown))
                {
                    ProcessMarkdownToWord(body, markdown);
                }
                else
                {
                    // 如果內容為空，添加提示
                    var emptyParagraph = new DocParagraph();
                    var emptyRun = new Run();
                    emptyRun.Append(new Text("（此筆記內容為空）") { Space = SpaceProcessingModeValues.Preserve });
                    emptyParagraph.Append(emptyRun);
                    body.Append(emptyParagraph);
                }
                
                // 設定文檔結構
                document.Append(body);
                mainPart.Document = document;
                
                // 保存並關閉文檔
                mainPart.Document.Save();
                wordDocument.Dispose();
                
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Word 匯出錯誤: {ex.Message}");
                Console.WriteLine($"錯誤堆疊: {ex.StackTrace}");
                return null;
            }
        }

        private void ProcessMarkdownToWord(Body body, string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return;

            // 將換行符統一處理
            markdown = markdown.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = markdown.Split('\n');
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    // 空行
                    body.Append(new DocParagraph());
                    continue;
                }
                
                var paragraph = new DocParagraph();
                
                if (trimmedLine.StartsWith("#### "))
                {
                    // H4 標題 - 16pt 粗體
                    var text = trimmedLine.Substring(5).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var run = new Run();
                        var runProperties = new RunProperties();
                        runProperties.Append(new Bold());
                        runProperties.Append(new FontSize() { Val = "32" }); // 16pt = 32 半點
                        run.Append(runProperties);
                        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
                        paragraph.Append(run);
                    }
                }
                else if (trimmedLine.StartsWith("### "))
                {
                    // H3 標題 - 18pt 粗體
                    var text = trimmedLine.Substring(4).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var run = new Run();
                        var runProperties = new RunProperties();
                        runProperties.Append(new Bold());
                        runProperties.Append(new FontSize() { Val = "36" }); // 18pt = 36 半點
                        run.Append(runProperties);
                        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
                        paragraph.Append(run);
                    }
                }
                else if (trimmedLine.StartsWith("## "))
                {
                    // H2 標題 - 20pt 粗體
                    var text = trimmedLine.Substring(3).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var run = new Run();
                        var runProperties = new RunProperties();
                        runProperties.Append(new Bold());
                        runProperties.Append(new FontSize() { Val = "40" }); // 20pt = 40 半點
                        run.Append(runProperties);
                        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
                        paragraph.Append(run);
                    }
                }
                else if (trimmedLine.StartsWith("# "))
                {
                    // H1 標題 - 24pt 粗體
                    var text = trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var run = new Run();
                        var runProperties = new RunProperties();
                        runProperties.Append(new Bold());
                        runProperties.Append(new FontSize() { Val = "48" }); // 24pt = 48 半點
                        run.Append(runProperties);
                        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
                        paragraph.Append(run);
                    }
                }
                else if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                {
                    // 無序列表
                    var text = trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        // 處理列表項目的內聯格式
                        ProcessTextWithFormatting(paragraph, "• " + text);
                    }
                }
                else if (trimmedLine.StartsWith("> "))
                {
                    // 引用塊
                    var text = trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var run = new Run();
                        var runProperties = new RunProperties();
                        runProperties.Append(new Italic());
                        run.Append(runProperties);
                        ProcessTextWithFormattingInRun(run, text, runProperties);
                    }
                }
                else
                {
                    // 普通段落，處理內聯格式
                    ProcessTextWithFormatting(paragraph, trimmedLine);
                }
                
                body.Append(paragraph);
            }
        }

        private void ProcessTextWithFormatting(DocParagraph paragraph, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                var emptyRun = new Run();
                emptyRun.Append(new Text("") { Space = SpaceProcessingModeValues.Preserve });
                paragraph.Append(emptyRun);
                return;
            }

            // 使用更複雜的處理來支援多種格式
            ProcessComplexFormatting(paragraph, text);
        }

        private void ProcessComplexFormatting(DocParagraph paragraph, string text)
        {
            // 定義格式模式（按優先級排序）
            var patterns = new List<(string pattern, string type)>
            {
                (@"(\*\*[^*]+\*\*)", "bold"),           // **粗體**
                (@"(\+\+[^+]+\+\+)", "underline"),      // ++底線++
                (@"(?<!\*)(\*[^*]+\*)(?!\*)", "italic") // *斜體*
            };

            var segments = new List<(string text, string format)>();
            var currentText = text;
            var currentIndex = 0;

            while (currentIndex < currentText.Length)
            {
                var nextMatch = -1;
                string matchType = "";
                string matchText = "";
                int matchStart = currentText.Length;

                // 找到最早出現的格式
                foreach (var (pattern, type) in patterns)
                {
                    var match = Regex.Match(currentText.Substring(currentIndex), pattern);
                    if (match.Success && (currentIndex + match.Index) < matchStart)
                    {
                        matchStart = currentIndex + match.Index;
                        nextMatch = matchStart;
                        matchType = type;
                        matchText = match.Value;
                    }
                }

                if (nextMatch == -1)
                {
                    // 沒有更多格式，添加剩餘文字
                    if (currentIndex < currentText.Length)
                    {
                        segments.Add((currentText.Substring(currentIndex), "normal"));
                    }
                    break;
                }

                // 添加格式前的普通文字
                if (nextMatch > currentIndex)
                {
                    segments.Add((currentText.Substring(currentIndex, nextMatch - currentIndex), "normal"));
                }

                // 添加格式化文字
                segments.Add((matchText, matchType));
                currentIndex = nextMatch + matchText.Length;
            }

            // 將所有段落轉換為 Word 元素
            foreach (var (segmentText, format) in segments)
            {
                if (string.IsNullOrEmpty(segmentText)) continue;

                var run = new Run();
                var runProperties = new RunProperties();

                switch (format)
                {
                    case "bold":
                        runProperties.Append(new Bold());
                        runProperties.Append(new FontSize() { Val = "24" }); // 12pt = 24 半點
                        var boldText = segmentText.Substring(2, segmentText.Length - 4);
                        run.Append(runProperties);
                        run.Append(new Text(boldText) { Space = SpaceProcessingModeValues.Preserve });
                        break;

                    case "underline":
                        runProperties.Append(new Underline() { Val = UnderlineValues.Single });
                        runProperties.Append(new FontSize() { Val = "24" }); // 12pt = 24 半點
                        var underlineText = segmentText.Substring(2, segmentText.Length - 4);
                        run.Append(runProperties);
                        run.Append(new Text(underlineText) { Space = SpaceProcessingModeValues.Preserve });
                        break;

                    case "italic":
                        runProperties.Append(new Italic());
                        runProperties.Append(new FontSize() { Val = "24" }); // 12pt = 24 半點
                        var italicText = segmentText.Substring(1, segmentText.Length - 2);
                        run.Append(runProperties);
                        run.Append(new Text(italicText) { Space = SpaceProcessingModeValues.Preserve });
                        break;

                    default: // normal
                        runProperties.Append(new FontSize() { Val = "24" }); // 12pt = 24 半點
                        run.Append(runProperties);
                        run.Append(new Text(segmentText) { Space = SpaceProcessingModeValues.Preserve });
                        break;
                }

                paragraph.Append(run);
            }
        }

        private void ProcessTextWithFormattingInRun(Run baseRun, string text, RunProperties baseProperties)
        {
            // 簡化版本，用於已有基本格式的文字（如引用）
            baseRun.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }

        // 移除舊的方法
        private void ProcessItalicText(DocParagraph paragraph, string text)
        {
            // 這個方法已被 ProcessComplexFormatting 替代
            var run = new Run();
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            paragraph.Append(run);
        }
    }
} 