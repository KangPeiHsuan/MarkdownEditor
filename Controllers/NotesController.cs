using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarkdownEditor.Data;
using MarkdownEditor.Models;
using MarkdownEditor.Services;

namespace MarkdownEditor.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMarkdownService _markdownService;
        private readonly IExportService _exportService;

        public NotesController(ApplicationDbContext context, IMarkdownService markdownService, IExportService exportService)
        {
            _context = context;
            _markdownService = markdownService;
            _exportService = exportService;
        }

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.Include(c => c.Notes).ToListAsync();
            return View(categories);
        }

        // GET: Notes/Editor/5
        public async Task<IActionResult> Editor(int? id)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            
            if (id == null)
            {
                // Create new note
                return View(new Note { CategoryId = 1 });
            }

            var note = await _context.Notes.Include(n => n.Category).FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: Notes/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Note note)
        {
            try
            {
                // 檢查標題是否重複
                var duplicateNote = await _context.Notes
                    .Where(n => n.Title == note.Title && n.CategoryId == note.CategoryId && n.Id != note.Id)
                    .FirstOrDefaultAsync();
                
                if (duplicateNote != null)
                {
                    return Json(new { success = false, message = "此分類下已存在相同名稱的筆記，請重新命名" });
                }

                if (note.Id == 0)
                {
                    // Create new note
                    note.CreatedAt = DateTime.Now;
                    note.UpdatedAt = DateTime.Now;
                    _context.Notes.Add(note);
                }
                else
                {
                    // Update existing note
                    var existingNote = await _context.Notes.FindAsync(note.Id);
                    if (existingNote != null)
                    {
                        existingNote.Title = note.Title;
                        existingNote.Content = note.Content;
                        existingNote.CategoryId = note.CategoryId;
                        existingNote.UpdatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, id = note.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Notes/Preview
        [HttpPost]
        public IActionResult Preview([FromBody] string markdown)
        {
            var html = _markdownService.ConvertToHtml(markdown);
            return Json(new { html = html });
        }

        // GET: Notes/ExportPreview/5
        public async Task<IActionResult> ExportPreview(int id)
        {
            var note = await _context.Notes.Include(n => n.Category).FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            var html = _markdownService.ConvertToHtml(note.Content);
            ViewBag.Html = html;
            return View(note);
        }

        // POST: Notes/ExportPdf/5
        public async Task<IActionResult> ExportPdf(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            var html = _markdownService.ConvertToHtml(note.Content);
            var pdfBytes = _exportService.ExportToPdf(html, note.Title);
            
            return File(pdfBytes, "application/pdf", $"{note.Title}.pdf");
        }

        // POST: Notes/ExportWord/5
        public async Task<IActionResult> ExportWord(int id)
        {
            try
            {
                var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
                if (note == null)
                {
                    return NotFound();
                }

                var content = note.Content ?? "";
                var title = note.Title ?? "未命名筆記";
                
                var wordBytes = _exportService.ExportToWord(content, title);
                
                if (wordBytes == null || wordBytes.Length == 0)
                {
                    return BadRequest("Word 文檔生成失敗：無法產生有效的文檔內容。");
                }
                
                return File(wordBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{title}.docx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExportWord 控制器錯誤: {ex.Message}");
                return BadRequest($"Word 匯出過程發生錯誤: {ex.Message}");
            }
        }

        // DELETE: Notes/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return Json(new { success = false, message = "筆記不存在" });
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }
    }
} 