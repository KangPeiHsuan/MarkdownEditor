# Bug 修正說明

本次修正解決了以下五個主要問題：

## ✅ 修正 1：編輯模式切換後無法繼續編輯

### 問題描述
從編輯模式切換至預覽模式後，無法再切回編輯或分割模式進行輸入，編輯器會卡住。

### 解決方案
修改 `wwwroot/js/markdown-editor.js` 中的 `switchViewMode` 函數：
- 在切換回編輯模式時，重新聚焦編輯器
- 確保編輯器在所有模式下都不被禁用
- 添加 `setTimeout` 確保DOM更新完成後再設定焦點

### 修正檔案
- `wwwroot/js/markdown-editor.js`

---

## ✅ 修正 2：分類/筆記名稱可重複

### 問題描述
目前允許建立相同名稱的分類或筆記，導致難以辨識與管理。

### 解決方案
在控制器層面添加重複名稱檢查：
- **筆記**：同一分類下不允許重複標題
- **分類**：全域不允許重複分類名稱
- 儲存前進行資料庫查詢檢查
- 返回明確的錯誤訊息

### 修正檔案
- `Controllers/NotesController.cs` - 添加筆記標題重複檢查
- `Controllers/CategoriesController.cs` - 添加分類名稱重複檢查

---

## ✅ 修正 3：Word 匯出內容為空白

### 問題描述
使用匯出 Word 功能時，下載的 Word 檔內容為空白。

### 解決方案
重新實作 Word 匯出功能：
- 添加 `ProcessMarkdownToWord` 方法處理 Markdown 語法
- 支援標題層級（H1, H2, H3）並套用對應字體大小
- 支援粗體、斜體、列表、引用塊等格式
- 添加 `ProcessTextWithFormatting` 方法處理內聯格式

### 修正檔案
- `Services/ExportService.cs`

---

## ✅ 修正 4：PDF 匯出顯示 HTML 原始碼

### 問題描述
使用匯出 PDF 功能時，內容顯示的是 HTML 原始碼，而不是轉換後的樣式內容。

### 解決方案
重新實作 PDF 匯出功能：
- 添加 `ProcessHtmlToPdf` 方法解析 HTML 標籤
- 使用正則表達式清理和轉換 HTML 內容
- 處理標題、粗體、斜體、列表等元素
- 移除 HTML 標籤，保留格式化的純文字

### 修正檔案
- `Services/ExportService.cs`

---

## ✅ 修正 5：「全部筆記」分類無法正常顯示

### 問題描述
點選總覽頁面中的「全部筆記」分類時，畫面無反應或未正確顯示所有筆記。

### 解決方案
改進分類篩選功能：
- 使用事件委託（Event Delegation）處理動態添加的元素
- 正確處理「全部筆記」的空字串 categoryId
- 添加 `updateNotesDisplay` 函數提供更好的用戶體驗
- 當分類下無筆記時顯示友善提示訊息

### 修正檔案
- `wwwroot/js/notes-index.js`

---

## 🔧 技術改進

### 新增功能
1. **名稱重複檢查**：防止建立重複的分類或筆記名稱
2. **改進的匯出功能**：Word 和 PDF 匯出現在支援 Markdown 格式
3. **更好的用戶體驗**：編輯器模式切換更流暢，分類篩選更可靠

### 錯誤處理
1. 添加明確的錯誤訊息
2. 改進前端錯誤處理
3. 更好的用戶反饋

### 代碼品質
1. 使用事件委託改進 JavaScript 事件處理
2. 添加必要的 using 語句
3. 改進方法結構和可讀性

---

## 🧪 測試建議

建議測試以下場景：

1. **編輯器模式切換**
   - 在編輯、預覽、分割模式間切換
   - 確認編輯器始終可正常輸入

2. **名稱重複檢查**
   - 嘗試建立重複名稱的分類
   - 嘗試在同一分類下建立重複名稱的筆記

3. **匯出功能**
   - 匯出包含各種 Markdown 語法的筆記
   - 檢查 Word 和 PDF 檔案內容格式

4. **分類篩選**
   - 點選「全部筆記」查看所有筆記
   - 點選特定分類查看對應筆記
   - 測試空分類的提示訊息

---

## 📋 檔案清單

修正涉及的檔案：
- `Controllers/NotesController.cs`
- `Controllers/CategoriesController.cs`
- `Services/ExportService.cs`
- `wwwroot/js/markdown-editor.js`
- `wwwroot/js/notes-index.js` 