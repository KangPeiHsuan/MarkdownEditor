// 主題切換功能
document.addEventListener('DOMContentLoaded', function () {
    const themeToggle = document.getElementById('themeToggle');
    const themeText = document.getElementById('themeText');
    const themeIcon = themeToggle.querySelector('i');
    
    // 檢查本地儲存的主題設定
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        document.body.classList.add('dark-mode');
        themeIcon.className = 'fas fa-sun';
        themeText.textContent = '淺色模式';
    }
    
    // 主題切換事件
    themeToggle.addEventListener('click', function () {
        document.body.classList.toggle('dark-mode');
        
        if (document.body.classList.contains('dark-mode')) {
            themeIcon.className = 'fas fa-sun';
            themeText.textContent = '淺色模式';
            localStorage.setItem('theme', 'dark');
        } else {
            themeIcon.className = 'fas fa-moon';
            themeText.textContent = '深色模式';
            localStorage.setItem('theme', 'light');
        }
    });
});

// 通用 AJAX 請求函數
function makeRequest(url, method, data, successCallback, errorCallback) {
    $.ajax({
        url: url,
        type: method,
        data: JSON.stringify(data),
        contentType: 'application/json',
        success: function (response) {
            if (successCallback) successCallback(response);
        },
        error: function (xhr, status, error) {
            console.error('請求失敗:', error);
            if (errorCallback) {
                errorCallback(error);
            } else {
                alert('操作失敗，請稍後再試');
            }
        }
    });
}

// 顯示通知
function showNotification(message, type = 'info', duration = 3000) {
    const notification = $(`
        <div class="alert alert-${type} alert-dismissible fade show position-fixed" 
             style="top: 80px; right: 20px; z-index: 1050; min-width: 300px;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    $('body').append(notification);
    
    setTimeout(function () {
        notification.alert('close');
    }, duration);
} 