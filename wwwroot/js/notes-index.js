// 筆記列表頁面功能

// 顯示新增分類模態框
function showAddCategoryModal() {
    $('#categoryName').val('');
    $('#categoryDescription').val('');
    $('#addCategoryModal').modal('show');
}

// 儲存新分類
function saveCategory() {
    const name = $('#categoryName').val().trim();
    const description = $('#categoryDescription').val().trim();
    
    if (!name) {
        alert('請輸入分類名稱');
        return;
    }
    
    const categoryData = {
        name: name,
        description: description
    };
    
    makeRequest('/Categories/Create', 'POST', categoryData, 
        function(response) {
            if (response.success) {
                showNotification('分類新增成功', 'success');
                $('#addCategoryModal').modal('hide');
                location.reload();
            } else {
                showNotification('新增失敗：' + response.message, 'danger');
            }
        },
        function(error) {
            showNotification('新增失敗，請稍後再試', 'danger');
        }
    );
}

// 編輯分類
function editCategory(id, name, description) {
    $('#editCategoryId').val(id);
    $('#editCategoryName').val(name);
    $('#editCategoryDescription').val(description || '');
    $('#editCategoryModal').modal('show');
}

// 更新分類
function updateCategory() {
    const id = $('#editCategoryId').val();
    const name = $('#editCategoryName').val().trim();
    const description = $('#editCategoryDescription').val().trim();
    
    if (!name) {
        alert('請輸入分類名稱');
        return;
    }
    
    const categoryData = {
        id: parseInt(id),
        name: name,
        description: description
    };
    
    makeRequest('/Categories/Edit', 'POST', categoryData, 
        function(response) {
            if (response.success) {
                showNotification('分類更新成功', 'success');
                $('#editCategoryModal').modal('hide');
                location.reload();
            } else {
                showNotification('更新失敗：' + response.message, 'danger');
            }
        },
        function(error) {
            showNotification('更新失敗，請稍後再試', 'danger');
        }
    );
}

// 刪除分類
function deleteCategory(id) {
    if (confirm('確定要刪除此分類嗎？')) {
        $.ajax({
            url: '/Categories/Delete/' + id,
            type: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification('分類刪除成功', 'success');
                    location.reload();
                } else {
                    showNotification('刪除失敗：' + response.message, 'danger');
                }
            },
            error: function() {
                showNotification('刪除失敗，請稍後再試', 'danger');
            }
        });
    }
}

// 刪除筆記
function deleteNote(id) {
    if (confirm('確定要刪除此筆記嗎？')) {
        $.ajax({
            url: '/Notes/Delete/' + id,
            type: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification('筆記刪除成功', 'success');
                    location.reload();
                } else {
                    showNotification('刪除失敗：' + response.message, 'danger');
                }
            },
            error: function() {
                showNotification('刪除失敗，請稍後再試', 'danger');
            }
        });
    }
}

// 分類篩選功能
$(document).ready(function() {
    // 新增一個"全部筆記"選項
    if ($('#categoryList .all-categories').length === 0) {
        $('#categoryList').prepend(`
            <li class="list-group-item category-item all-categories active" data-category-id="">
                <div>
                    <h6 class="mb-0">全部筆記</h6>
                    <small class="text-muted">${$('.note-card').length} 篇筆記</small>
                </div>
            </li>
        `);
    }
    
    // 使用事件委託處理點擊事件，包括動態新增的元素
    $('#categoryList').on('click', '.category-item', function() {
        const categoryId = $(this).data('category-id');
        
        // 移除所有分類的 active 樣式
        $('.category-item').removeClass('active');
        
        // 添加選中樣式
        $(this).addClass('active');
        
        if (categoryId === "" || categoryId === undefined) {
            // 顯示所有筆記
            $('.note-card').show();
        } else {
            // 篩選特定分類的筆記
            $('.note-card').hide();
            $(`.note-card[data-category-id="${categoryId}"]`).show();
        }
        
        // 更新顯示狀態提示
        updateNotesDisplay(categoryId);
    });
});

// 更新筆記顯示狀態
function updateNotesDisplay(categoryId) {
    const visibleNotes = $('.note-card:visible');
    const totalNotes = $('.note-card').length;
    
    if (visibleNotes.length === 0) {
        if ($('#noNotesMessage').length === 0) {
            $('#notesContainer').append(`
                <div id="noNotesMessage" class="text-center text-muted py-5">
                    <i class="fas fa-sticky-note fa-3x mb-3"></i>
                    <p>此分類下尚無筆記</p>
                    <a href="/Notes/Editor" class="btn btn-primary">
                        <i class="fas fa-plus"></i> 新增第一篇筆記
                    </a>
                </div>
            `);
        }
    } else {
        $('#noNotesMessage').remove();
    }
} 