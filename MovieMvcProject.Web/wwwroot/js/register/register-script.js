// Şifre göster/gizle fonksiyonu
function setupPasswordToggle(inputId, toggleId) {
    const input = document.getElementById(inputId);
    const toggle = document.getElementById(toggleId);
    if (!input || !toggle) return;

    toggle.addEventListener('click', () => {
        if (input.type === 'password') {
            input.type = 'text';
            toggle.textContent = 'visibility_off';
        } else {
            input.type = 'password';
            toggle.textContent = 'visibility';
        }
    });
}

// Dosya seçildiğinde isim gösterimi
function setupFileDisplay(inputId, displayId, defaultText) {
    const fileInput = document.getElementById(inputId);
    const fileNameDisplay = document.getElementById(displayId);
    if (fileInput && fileNameDisplay) {
        fileInput.addEventListener('change', function () {
            const fileName = this.files[0] ? this.files[0].name : defaultText;
            fileNameDisplay.textContent = fileName;
        });
    }
}

// Başlatıcı
document.addEventListener('DOMContentLoaded', () => {
    setupPasswordToggle('PasswordInput', 'togglePassword');
    setupPasswordToggle('ConfirmPasswordInput', 'toggleConfirmPassword');
});