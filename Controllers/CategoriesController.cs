using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarkdownEditor.Data;
using MarkdownEditor.Models;

namespace MarkdownEditor.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Categories/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            try
            {
                // 檢查分類名稱是否重複
                var duplicateCategory = await _context.Categories
                    .Where(c => c.Name == category.Name)
                    .FirstOrDefaultAsync();
                
                if (duplicateCategory != null)
                {
                    return Json(new { success = false, message = "分類名稱已存在，請重新命名" });
                }

                if (ModelState.IsValid)
                {
                    category.CreatedAt = DateTime.Now;
                    category.UpdatedAt = DateTime.Now;
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, category = category });
                }
                return Json(new { success = false, message = "資料驗證失敗" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Categories/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] Category category)
        {
            try
            {
                // 檢查分類名稱是否重複（排除自己）
                var duplicateCategory = await _context.Categories
                    .Where(c => c.Name == category.Name && c.Id != category.Id)
                    .FirstOrDefaultAsync();
                
                if (duplicateCategory != null)
                {
                    return Json(new { success = false, message = "分類名稱已存在，請重新命名" });
                }

                var existingCategory = await _context.Categories.FindAsync(category.Id);
                if (existingCategory == null)
                {
                    return Json(new { success = false, message = "分類不存在" });
                }

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Categories/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories.Include(c => c.Notes).FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return Json(new { success = false, message = "分類不存在" });
                }

                if (category.Notes.Any())
                {
                    return Json(new { success = false, message = "此分類下還有筆記，無法刪除" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 