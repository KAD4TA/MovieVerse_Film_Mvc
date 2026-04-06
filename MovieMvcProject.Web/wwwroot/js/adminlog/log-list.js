function openDetailedModal(btn) {
    const content = btn.getAttribute('data-error-content');
    const modal = document.getElementById('errorDetailModal');
    const container = document.getElementById('modalContainer');
    const body = document.getElementById('errorModalBody');

    body.textContent = content;
    modal.classList.remove('hidden');

    setTimeout(() => {
        container.classList.remove('scale-95');
        container.classList.add('scale-100');
    }, 10);

    document.body.style.overflow = 'hidden';
}

function closeDetailedModal() {
    const modal = document.getElementById('errorDetailModal');
    const container = document.getElementById('modalContainer');

    container.classList.remove('scale-100');
    container.classList.add('scale-95');

    setTimeout(() => {
        modal.classList.add('hidden');
    }, 200);

    document.body.style.overflow = 'auto';
}

window.onclick = function (e) {
    if (e.target.id === 'errorDetailModal') {
        closeDetailedModal();
    }
};

document.onkeydown = function (e) {
    if (e.key === "Escape") {
        closeDetailedModal();
    }
};