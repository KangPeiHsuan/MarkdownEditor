// Markdown 編輯器功能
let autoSaveTimer;
let isModified = false;
let currentViewMode = 'edit';

$(document).ready(function() {
    const editor = $('#markdownEditor');
    const preview = $('#markdownPreview');
    const noteId = $('#noteId').val();
    
    // 初始化
    initializeEditor();
    setupViewModeHandlers();
    setupAutoSave();
    setupScrollSync();
    
    // 如果有內容，初始化預覽
    if (editor.val().trim()) {
        updatePreview();
    }
    
    // 編輯器輸入事件
    editor.on('input', function() {
        isModified = true;
        if (currentViewMode === 'split' || currentViewMode === 'preview') {
            updatePreview();
        }
        resetAutoSaveTimer();
    });
    
    // 儲存按鈕事件
    $('#saveBtn').click(function() {
        saveNote();
    });
    
    // 匯出按鈕事件
    $('#exportBtn').click(function() {
        const id = $('#noteId').val();
        if (id && id !== '0') {
            window.location.href = '/Notes/ExportPreview/' + id;
        } else {
            showNotification('請先儲存筆記再匯出', 'warning');
        }
    });
});

// 初始化編輯器
function initializeEditor() {
    const editor = $('#markdownEditor');
    
    // 設定編輯器高度
    resizeEditor();
    
    // 監聽視窗大小變化
    $(window).resize(function() {
        resizeEditor();
    });
    
    // 加入自動完成功能
    editor.on('keydown', function(e) {
        handleAutoComplete(e);
    });
}

// 調整編輯器高度
function resizeEditor() {
    const windowHeight = $(window).height();
    const headerHeight = $('header').height() || 0;
    const controlsHeight = $('.mb-3').outerHeight(true) * 2 || 0;
    const newHeight = windowHeight - headerHeight - controlsHeight - 100;
    
    $('.editor-container').css('height', Math.max(newHeight, 400) + 'px');
}

// 設定視圖模式處理器
function setupViewModeHandlers() {
    $('input[name="viewMode"]').change(function() {
        const mode = $(this).val();
        switchViewMode(mode);
    });
}

// 切換視圖模式
function switchViewMode(mode) {
    currentViewMode = mode;
    const editorCol = $('#editorCol');
    const previewCol = $('#previewCol');
    
    switch (mode) {
        case 'edit':
            editorCol.removeClass('col-md-6').addClass('col-md-12');
            previewCol.addClass('d-none');
            break;
            
        case 'preview':
            editorCol.addClass('d-none');
            previewCol.removeClass('d-none col-md-6').addClass('col-md-12');
            updatePreview();
            break;
            
        case 'split':
            editorCol.removeClass('col-md-12').addClass('col-md-6');
            previewCol.removeClass('d-none col-md-12').addClass('col-md-6');
            updatePreview();
            break;
    }
}

// 更新預覽
function updatePreview() {
    const markdown = $('#markdownEditor').val();
    
    if (!markdown.trim()) {
        $('#markdownPreview').html('<p class="text-muted">預覽內容將在此顯示...</p>');
        return;
    }
    
    $.ajax({
        url: '/Notes/Preview',
        type: 'POST',
        data: JSON.stringify(markdown),
        contentType: 'application/json',
        success: function(response) {
            $('#markdownPreview').html(response.html);
        },
        error: function() {
            $('#markdownPreview').html('<p class="text-danger">預覽載入失敗</p>');
        }
    });
}

// 設定自動儲存
function setupAutoSave() {
    // 每 30 秒自動儲存
    setInterval(function() {
        if (isModified) {
            autoSave();
        }
    }, 30000);
    
    // 頁面離開前自動儲存
    window.addEventListener('beforeunload', function(e) {
        if (isModified) {
            saveNote();
        }
    });
}

// 重設自動儲存計時器（修改後 5 秒）
function resetAutoSaveTimer() {
    clearTimeout(autoSaveTimer);
    autoSaveTimer = setTimeout(function() {
        if (isModified) {
            autoSave();
        }
    }, 5000);
}

// 自動儲存
function autoSave() {
    showAutoSaveIndicator('saving');
    
    const noteData = {
        id: parseInt($('#noteId').val()) || 0,
        title: $('#noteTitle').val() || '未命名筆記',
        content: $('#markdownEditor').val(),
        categoryId: parseInt($('#categorySelect').val())
    };
    
    $.ajax({
        url: '/Notes/Save',
        type: 'POST',
        data: JSON.stringify(noteData),
        contentType: 'application/json',
        success: function(response) {
            if (response.success) {
                isModified = false;
                if (response.id && $('#noteId').val() === '0') {
                    $('#noteId').val(response.id);
                    $('#exportBtn').prop('disabled', false);
                    // 更新 URL
                    window.history.replaceState({}, '', '/Notes/Editor/' + response.id);
                }
                showAutoSaveIndicator('saved');
            } else {
                showAutoSaveIndicator('error');
            }
        },
        error: function() {
            showAutoSaveIndicator('error');
        }
    });
}

// 手動儲存
function saveNote() {
    const noteData = {
        id: parseInt($('#noteId').val()) || 0,
        title: $('#noteTitle').val() || '未命名筆記',
        content: $('#markdownEditor').val(),
        categoryId: parseInt($('#categorySelect').val())
    };
    
    if (!noteData.title.trim()) {
        showNotification('請輸入筆記標題', 'warning');
        $('#noteTitle').focus();
        return;
    }
    
    $.ajax({
        url: '/Notes/Save',
        type: 'POST',
        data: JSON.stringify(noteData),
        contentType: 'application/json',
        success: function(response) {
            if (response.success) {
                isModified = false;
                if (response.id && $('#noteId').val() === '0') {
                    $('#noteId').val(response.id);
                    $('#exportBtn').prop('disabled', false);
                    // 更新 URL
                    window.history.replaceState({}, '', '/Notes/Editor/' + response.id);
                }
                showNotification('筆記儲存成功', 'success');
            } else {
                showNotification('儲存失敗：' + response.message, 'danger');
            }
        },
        error: function() {
            showNotification('儲存失敗，請稍後再試', 'danger');
        }
    });
}

// 設定同步捲動
function setupScrollSync() {
    let isEditorScrolling = false;
    let isPreviewScrolling = false;
    
    $('#markdownEditor').on('scroll', function() {
        if (currentViewMode === 'split' && !isPreviewScrolling) {
            isEditorScrolling = true;
            syncScroll(this, $('#markdownPreview')[0]);
            setTimeout(() => isEditorScrolling = false, 100);
        }
    });
    
    $('#markdownPreview').on('scroll', function() {
        if (currentViewMode === 'split' && !isEditorScrolling) {
            isPreviewScrolling = true;
            syncScroll(this, $('#markdownEditor')[0]);
            setTimeout(() => isPreviewScrolling = false, 100);
        }
    });
}

// 同步捲動
function syncScroll(source, target) {
    const sourceScrollTop = source.scrollTop;
    const sourceScrollHeight = source.scrollHeight - source.clientHeight;
    const targetScrollHeight = target.scrollHeight - target.clientHeight;
    
    if (sourceScrollHeight > 0) {
        const scrollRatio = sourceScrollTop / sourceScrollHeight;
        target.scrollTop = scrollRatio * targetScrollHeight;
    }
}

// 顯示自動儲存指示器
function showAutoSaveIndicator(type) {
    let indicator = $('.auto-save-indicator');
    
    if (indicator.length === 0) {
        indicator = $('<div class="auto-save-indicator"></div>');
        $('body').append(indicator);
    }
    
    indicator.removeClass('saving saved error').addClass(type);
    
    switch (type) {
        case 'saving':
            indicator.text('儲存中...');
            break;
        case 'saved':
            indicator.text('已儲存');
            break;
        case 'error':
            indicator.text('儲存失敗');
            break;
    }
    
    indicator.show();
    setTimeout(() => indicator.hide(), 2000);
}

// 自動完成功能
function handleAutoComplete(e) {
    const editor = e.target;
    const cursorPos = editor.selectionStart;
    const text = editor.value;
    const lineStart = text.lastIndexOf('\n', cursorPos - 1) + 1;
    const currentLine = text.substring(lineStart, cursorPos);
    
    // Tab 鍵處理
    if (e.key === 'Tab') {
        e.preventDefault();
        insertText(editor, '    '); // 插入 4 個空格
        return;
    }
    
    // Enter 鍵自動完成列表
    if (e.key === 'Enter') {
        // 無序列表
        const unorderedMatch = currentLine.match(/^(\s*)([-*+]\s)/);
        if (unorderedMatch) {
            e.preventDefault();
            insertText(editor, '\n' + unorderedMatch[1] + unorderedMatch[2]);
            return;
        }
        
        // 有序列表
        const orderedMatch = currentLine.match(/^(\s*)(\d+)\.\s/);
        if (orderedMatch) {
            e.preventDefault();
            const nextNum = parseInt(orderedMatch[2]) + 1;
            insertText(editor, '\n' + orderedMatch[1] + nextNum + '. ');
            return;
        }
        
        // 引用塊
        const quoteMatch = currentLine.match(/^(\s*>+\s?)/);
        if (quoteMatch) {
            e.preventDefault();
            insertText(editor, '\n' + quoteMatch[1]);
            return;
        }
    }
}

// 插入文字到編輯器
function insertText(editor, text) {
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const value = editor.value;
    
    editor.value = value.substring(0, start) + text + value.substring(end);
    editor.selectionStart = editor.selectionEnd = start + text.length;
    
    // 觸發 input 事件
    $(editor).trigger('input');
} 