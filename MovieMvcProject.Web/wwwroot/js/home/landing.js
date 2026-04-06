
document.addEventListener('DOMContentLoaded', function () {
    
    const langSelect = document.getElementById('cultureSelect');
    if (langSelect) {
        langSelect.addEventListener('change', function () {
            this.form.submit();
        });
    }

    console.log("MovieVerse Landing Page Hazır.");
});