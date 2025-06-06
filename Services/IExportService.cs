namespace MarkdownEditor.Services
{
    public interface IExportService
    {
        byte[] ExportToPdf(string html, string title);
        byte[] ExportToWord(string markdown, string title);
    }
} 