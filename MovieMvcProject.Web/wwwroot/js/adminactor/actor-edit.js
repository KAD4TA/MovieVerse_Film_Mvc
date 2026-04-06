document.addEventListener("DOMContentLoaded", function () {
    const avatarInput = document.getElementById('AvatarUrlInput');
    const avatarPreview = document.getElementById('avatarPreview');

    if (avatarInput && avatarPreview) {
        avatarInput.addEventListener('input', function (e) {
            avatarPreview.src = e.target.value || "/profile-images/default-profile.png";
        });

        avatarPreview.onerror = function () {
            this.src = "/profile-images/default-profile.png";
        };
    }
});

function copyAvatarUrl() {
    const val = document.getElementById('AvatarUrlInput').value;
    navigator.clipboard.writeText(val);
    alert('URL Copied to Clipboard!');
}

async function removeMovieFromActor(actorId, movieId, confirmMessage) {
    if (!confirm(confirmMessage)) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const formData = new FormData();
    formData.append('actorId', actorId);
    formData.append('movieId', movieId);
    formData.append('__RequestVerificationToken', token);

    try {
        const response = await fetch('/Admin/AdminActor/RemoveMovie', {
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