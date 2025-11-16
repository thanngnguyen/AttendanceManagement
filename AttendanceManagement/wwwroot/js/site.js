// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-hide alerts after 5 seconds
$(document).ready(function() {
    setTimeout(function() {
        $('.alert:not(.alert-permanent)').fadeOut('slow');
    }, 5000);

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Confirm delete actions
    $('.confirm-delete').on('click', function(e) {
        if (!confirm('Bạn có chắc chắn muốn xóa?')) {
            e.preventDefault();
        }
    });

    // Format date inputs
    $('input[type="datetime-local"]').each(function() {
        if (!$(this).val()) {
            var now = new Date();
            now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
            $(this).val(now.toISOString().slice(0, 16));
        }
    });
});

// Geolocation helper functions
var Geolocation = {
    getCurrentPosition: function(successCallback, errorCallback) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                successCallback,
                errorCallback || this.defaultErrorHandler,
                {
                    enableHighAccuracy: true,
                    timeout: 10000,
                    maximumAge: 0
                }
            );
        } else {
            alert('Trình duyệt của bạn không hỗ trợ định vị.');
        }
    },

    defaultErrorHandler: function(error) {
        var errorMsg = '';
        switch(error.code) {
            case error.PERMISSION_DENIED:
                errorMsg = 'Bạn đã từ chối quyền truy cập vị trí.';
                break;
            case error.POSITION_UNAVAILABLE:
                errorMsg = 'Không thể xác định vị trí của bạn.';
                break;
            case error.TIMEOUT:
                errorMsg = 'Hết thời gian chờ lấy vị trí.';
                break;
            default:
                errorMsg = 'Có lỗi xảy ra khi lấy vị trí.';
        }
        alert(errorMsg);
    },

    calculateDistance: function(lat1, lon1, lat2, lon2) {
        const R = 6371000; // Earth's radius in meters
        const dLat = this.toRadians(lat2 - lat1);
        const dLon = this.toRadians(lon2 - lon1);

        const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                  Math.cos(this.toRadians(lat1)) * Math.cos(this.toRadians(lat2)) *
                  Math.sin(dLon / 2) * Math.sin(dLon / 2);

        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        return R * c;
    },

    toRadians: function(degrees) {
        return degrees * Math.PI / 180;
    }
};

// File upload preview
function previewFile(input, previewId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        var file = input.files[0];
        
        reader.onload = function(e) {
            $('#' + previewId).html(
                '<div class="alert alert-info">' +
                '<i class="fas fa-file"></i> ' + file.name + 
                ' (' + formatBytes(file.size) + ')' +
                '</div>'
            );
        };
        
        reader.readAsDataURL(file);
    }
}

function formatBytes(bytes, decimals = 2) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
}

// Copy to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function() {
        showNotification('Đã sao chép!', 'success');
    }, function(err) {
        console.error('Could not copy text: ', err);
    });
}

// Show notification
function showNotification(message, type = 'info') {
    var alertClass = 'alert-' + type;
    var icon = type === 'success' ? 'fa-check-circle' : 
               type === 'danger' ? 'fa-exclamation-circle' : 
               'fa-info-circle';
    
    var alert = $('<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">' +
        '<i class="fas ' + icon + '"></i> ' + message +
        '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
        '</div>');
    
    $('#notificationArea').append(alert);
    
    setTimeout(function() {
        alert.fadeOut('slow', function() {
            $(this).remove();
        });
    }, 3000);
}

// Form validation helpers
//$.validator.methods.range = function(value, element, param) {
//    var globalizedValue = value.replace(",", ".");
//    return this.optional(element) || (globalizedValue >= param[0] && globalizedValue <= param[1]);
//};

$.validator.methods.number = function(value, element) {
    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
};

// Disable button after click to prevent double submission
$('form').on('submit', function() {
    $(this).find('button[type="submit"]').prop('disabled', true);
    setTimeout(function() {
        $('button[type="submit"]').prop('disabled', false);
    }, 3000);
});
