
let MOVIE_ID = '';
let TRAILER_URL = '';
let movieLocalizer = {};
let lockedRating = 0;

window.initMovieDetails = function (config) {
    MOVIE_ID = config.movieId;
    TRAILER_URL = config.trailerUrl;
    movieLocalizer = config.localizer;

    loadComments(1);
    setupStarEvents();
};

function refreshStars(containerSelector, starSelector, inputId, count) {
    const stars = document.querySelectorAll(`${containerSelector} ${starSelector}`);
    stars.forEach((s, idx) => {
        const isActive = idx < count;
        s.classList.toggle('text-yellow-500', isActive);
        s.classList.toggle('text-gray-600', !isActive);
        s.style.fontVariationSettings = `'FILL' ${isActive ? 1 : 0}`;
    });
}

// Edit Modal Star Events
document.addEventListener('click', (e) => {
    const star = e.target.closest('.edit-star-btn');
    if (star) {
        const val = parseInt(star.dataset.val);
        document.getElementById('editRatingInput').value = val;
        refreshStars('#edit-star-selector', '.edit-star-btn', 'editRatingInput', val);
    }
});

document.addEventListener('mouseover', (e) => {
    const star = e.target.closest('.edit-star-btn');
    if (star) {
        const val = parseInt(star.dataset.val);
        refreshStars('#edit-star-selector', '.edit-star-btn', 'editRatingInput', val);
    }
});

document.addEventListener('mouseout', (e) => {
    if (e.target.closest('.edit-star-btn')) {
        const currentVal = parseInt(document.getElementById('editRatingInput').value) || 0;
        refreshStars('#edit-star-selector', '.edit-star-btn', 'editRatingInput', currentVal);
    }
});

async function loadComments(pageNumber = 1) {
    const container = document.getElementById('commentsListArea');
    try {
        const res = await fetch(`/Comment/List?movieId=${MOVIE_ID}&pageNumber=${pageNumber}`);
        if (!res.ok) throw new Error();
        container.innerHTML = await res.text();
    } catch {
        container.innerHTML = `<div class="text-red-400 text-center py-10 font-bold">${movieLocalizer.commentLoadError}</div>`;
    }
}

function getRequestVerificationToken() {
    const tokenInput = document.querySelector('#globalCsrfForm input[name="__RequestVerificationToken"]') || document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : null;
}

async function handleFormSubmit(e, form) {
    e.preventDefault();
    const btn = form.querySelector('button[type="submit"]');
    const originalHTML = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = `${movieLocalizer.send}...`;

    try {
        const res = await fetch('/Comment/AddComment', {
            method: 'POST',
            body: new FormData(form),
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const data = await res.json();
        if (data.success) {
            showToast(data.message);
            form.reset();
            if (form.id === 'replyForm') window.closeReplyModal();
            if (form.id === 'mainCommentForm') resetStars();
            loadComments();
        } else {
            showToast(data.message, "error");
        }
    } catch {
        showToast("Sunucu hatası!", "error");
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalHTML;
    }
}

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById('mainCommentForm')?.addEventListener('submit', (e) => handleFormSubmit(e, e.target));
    document.getElementById('replyForm')?.addEventListener('submit', (e) => handleFormSubmit(e, e.target));
});

window.deleteComment = function (id) {
    document.getElementById('deleteCommentId').value = id;
    document.getElementById('deleteConfirmModal').classList.add('active');
};

window.closeDeleteModal = function () {
    document.getElementById('deleteConfirmModal').classList.remove('active');
    document.getElementById('deleteCommentId').value = '';
};

window.confirmDeleteComment = async function () {
    const id = document.getElementById('deleteCommentId').value;
    if (!id) return;
    const token = getRequestVerificationToken();
    const formData = new FormData();
    formData.append('CommentId', id);
    if (token) formData.append('__RequestVerificationToken', token);

    try {
        const res = await fetch('/Comment/Delete', { method: 'POST', body: formData });
        const data = await res.json();
        if (data.success) {
            showToast(data.message);
            closeDeleteModal();
            loadComments();
        } else {
            showToast(data.message, "error");
        }
    } catch {
        showToast("İşlem sırasında hata oluştu", "error");
    }
};

window.editComment = async function (id) {
    try {
        const res = await fetch(`/Comment/Edit?commentId=${id}`);
        const html = await res.text();
        document.getElementById('editModalContainer').innerHTML = html;
        document.getElementById('editModal').classList.add('active');
    } catch { showToast("Form yüklenemedi", "error"); }
};

window.closeEditModal = function () { document.getElementById('editModal').classList.remove('active'); };

document.addEventListener('submit', async (e) => {
    const form = e.target.closest('#editCommentForm');
    if (form) {
        e.preventDefault();
        const formData = new FormData(form);
        if (!formData.has('__RequestVerificationToken')) {
            const token = getRequestVerificationToken();
            if (token) formData.append('__RequestVerificationToken', token);
        }
        try {
            const res = await fetch('/Comment/Edit', { method: 'POST', body: formData });
            const data = await res.json();
            if (data.success) { showToast(data.message); closeEditModal(); loadComments(); }
            else showToast(data.message, "error");
        } catch {
            showToast("Güncelleme hatası", "error");
        }
    }
});

function setupStarEvents() {
    const stars = document.querySelectorAll('.star-btn');
    stars.forEach(star => {
        star.addEventListener('click', () => {
            lockedRating = parseInt(star.dataset.val);
            document.getElementById('ratingInput').value = lockedRating;
            updateStars(lockedRating);
        });
        star.addEventListener('mouseenter', () => updateStars(parseInt(star.dataset.val)));
        star.addEventListener('mouseleave', () => updateStars(lockedRating));
    });
}

function updateStars(count) {
    const stars = document.querySelectorAll('.star-btn');
    stars.forEach((s, idx) => {
        s.classList.toggle('star-active', idx < count);
        s.style.fontVariationSettings = `'FILL' ${idx < count ? 1 : 0}`;
    });
}

function resetStars() {
    lockedRating = 0;
    document.getElementById('ratingInput').value = 0;
    updateStars(0);
}

window.openReplyModal = function (id, user) {
    document.getElementById('replyParentId').value = id;
    document.getElementById('replyingToUser').innerText = user;
    document.getElementById('replyModal').classList.add('active');
};

window.closeReplyModal = function () { document.getElementById('replyModal').classList.remove('active'); };

window.openTrailer = function () {
    if (!TRAILER_URL) return;
    let embedUrl = TRAILER_URL.replace("watch?v=", "embed/");
    document.getElementById('trailerFrame').src = embedUrl + "?autoplay=1";
    document.getElementById('trailerModal').classList.replace('hidden', 'flex');
};

window.closeTrailer = function () {
    document.getElementById('trailerFrame').src = "";
    document.getElementById('trailerModal').classList.replace('flex', 'hidden');
};

window.addToWishlist = async function (id) {
    const token = getRequestVerificationToken();
    const formData = new FormData();
    formData.append('movieId', id);
    if (token) formData.append('__RequestVerificationToken', token);
    try {
        const res = await fetch('/Wishlist/Add', { method: 'POST', body: formData });
        const contentType = res.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") === -1) {
            showToast(movieLocalizer.loginToComment, "error");
            return;
        }
        const data = await res.json();
        showToast(data.message, data.success ? "success" : "error");
    } catch {
        showToast("İşlem başarısız veya giriş yapmalısınız.", "error");
    }
};

function showToast(msg, type = "success") {
    Toastify({
        text: msg,
        duration: 3000,
        position: "right",
        style: {
            background: type === "success" ? "#2563eb" : "#dc2626",
            borderRadius: "10px"
        }
    }).showToast();
}