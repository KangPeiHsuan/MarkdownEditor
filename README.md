# Markdown 筆記編輯器
> [!NOTE]
> - PROMPT LOG: [[Cursor] Markdown 編輯器](https://hackmd.io/@kangpei/SkwvNygXxl)
> - 這份專案主要會使用 Cursor AI 功能完成，練習並記錄如何使用 prompt 去實作出需求功能。

一個功能完整的 Markdown 筆記系統，支援即時預覽、分類管理、自動儲存和匯出功能。

## 功能特色

### 📝 內容編輯
- **Markdown 語法支援**：完整支援標準 Markdown 語法
- **三種視圖模式**：
  - 編輯模式：專注於內容編寫
  - 預覽模式：即時查看渲染結果
  - 分割模式：編輯與預覽同時顯示
- **智慧自動完成**：支援列表、引用等語法的自動完成
- **即時預覽**：輸入內容時即時更新預覽

### 🎨 視窗功能
- **同步捲動**：分割模式下編輯區與預覽區同步捲動
- **深色/淺色模式**：可在深色與淺色主題間切換
- **響應式設計**：適配不同螢幕尺寸

### 💾 儲存與匯出
- **自動儲存**：每 30 秒自動儲存 + 修改後 5 秒延遲儲存
- **即時儲存指示**：顯示儲存狀態（儲存中/已儲存/儲存失敗）
- **匯出功能**：
  - 支援匯出為 PDF 檔案
  - 支援匯出為 Word 檔案
  - 匯出前預覽確認

### 📁 分類管理
- **分類系統**：支援建立、編輯、刪除分類
- **筆記組織**：每篇筆記可指定分類
- **分類篩選**：可按分類篩選顯示筆記

## 技術規格

- **後端框架**：ASP.NET Core 8.0 MVC
- **前端技術**：Bootstrap 5.3、jQuery 3.7、Font Awesome 6.0
- **資料庫**：SQLite (Entity Framework Core)
- **Markdown 處理**：Markdig
- **匯出功能**：iTextSharp (PDF)、DocumentFormat.OpenXml (Word)

## 系統需求

- .NET 8.0 SDK
- 支援的作業系統：Windows 10/11、macOS、Linux

## 安裝與執行

### 1. 克隆專案
```bash
git clone <repository-url>
cd MarkdownEditor
```

### 2. 還原套件
```bash
dotnet restore
```

### 3. 建立資料庫
```bash
dotnet ef database update
```
如果沒有安裝 EF Core Tools，請先執行：
```bash
dotnet tool install --global dotnet-ef
```

### 4. 執行專案
```bash
dotnet run
```

### 5. 開啟瀏覽器
訪問 `https://localhost:5001` 或 `http://localhost:5000`

## 使用說明

### 建立第一篇筆記
1. 點擊「新增筆記」按鈕
2. 輸入筆記標題
3. 選擇分類（預設為「一般筆記」）
4. 在編輯器中輸入 Markdown 內容
5. 系統會自動儲存，或點擊「儲存」按鈕手動儲存

### 切換視圖模式
- **編輯模式**：適合專注寫作
- **預覽模式**：查看最終渲染效果
- **分割模式**：同時編輯與預覽，支援同步捲動

### 主題切換
點擊導航列右上角的主題切換按鈕，可在深色與淺色模式間切換。系統會記住您的偏好設定。

### 分類管理
1. 在筆記列表頁面，點擊分類區塊的「+」按鈕新增分類
2. 點擊分類旁的編輯按鈕修改分類資訊
3. 點擊分類旁的刪除按鈕刪除分類（需先移除該分類下的所有筆記）

### 匯出筆記
1. 在編輯頁面點擊「匯出」按鈕
2. 在預覽頁面確認內容無誤
3. 選擇匯出為 PDF 或 Word 格式
4. 檔案將自動下載

## 專案結構

```
MarkdownEditor/
├── Controllers/           # MVC 控制器
│   ├── NotesController.cs
│   └── CategoriesController.cs
├── Models/               # 資料模型
│   ├── Note.cs
│   └── Category.cs
├── Views/                # Razor 視圖
│   ├── Notes/
│   └── Shared/
├── wwwroot/             # 靜態檔案
│   ├── css/
│   └── js/
├── Data/                # 資料訪問
│   └── ApplicationDbContext.cs
├── Services/            # 業務邏輯服務
│   ├── MarkdownService.cs
│   └── ExportService.cs
└── Program.cs           # 程式進入點
```

## 開發說明

### 新增功能
1. 建立相應的 Controller Action
2. 新增或修改 Model（如需要）
3. 建立對應的 View
4. 加入必要的 JavaScript 功能

### 資料庫變更
1. 修改 Model 類別
2. 新增 Migration：`dotnet ef migrations add <MigrationName>`
3. 更新資料庫：`dotnet ef database update`

### 自訂主題
修改 `wwwroot/css/site.css` 檔案中的 CSS 變數和樣式。

## 故障排除

### 資料庫問題
- 確認 SQLite 檔案權限
- 嘗試刪除 `notes.db` 檔案並重新執行 `dotnet ef database update`

### 匯出功能問題
- 確認有足夠的磁碟空間
- 檢查瀏覽器的下載設定

### 效能問題
- 大型筆記可能影響即時預覽效能
- 建議將長篇內容分割為多篇筆記

## 授權資訊

本專案採用 MIT 授權條款。

## 貢獻指南

歡迎提交 Issue 和 Pull Request 來改進這個專案！

1. Fork 專案
2. 建立功能分支
3. 提交變更
4. 建立 Pull Request

## 聯絡資訊

如有問題或建議，請透過 GitHub Issues 聯絡。 