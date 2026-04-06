document.addEventListener("DOMContentLoaded", function () {
    const photoInput = document.getElementById('PhotoUrlInput');
    const photoPreview = document.getElementById('photoPreview');

    if (photoInput && photoPreview) {
        photoInput.addEventListener('input', function (e) {
            photoPreview.src = e.target.value || "/images/default-director.jpg";
        });

        photoPreview.onerror = function () {
            this.src = "/images/default-director.jpg";
        };
    }
});

async function removeMovieFromDirector(directorId, movieId, confirmMessage) {
    if (!confirm(confirmMessage)) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const formData = new FormData();
    formData.append('directorId', directorId);
    formData.append('movieId', movieId);
    formData.append('__RequestVerificationToken', token);

    try {
        const response = await fetch('/Admin/AdminDirector/RemoveMovie', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            window.location.reload();
        } else {
            alert('Silme başarısız: ' + response.statusText);
        }
    } catch (err) {
        alert('Bağlantı hatası!');
    }
}