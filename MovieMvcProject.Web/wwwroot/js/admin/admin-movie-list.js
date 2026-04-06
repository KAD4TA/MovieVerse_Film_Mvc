function openDeleteModal(id, title, poster, year, category, rating) {
    document.getElementById('m-id-input').value = id;
    document.getElementById('m-title').innerText = title;
    document.getElementById('m-confirm-text').innerText = `"${title}"`;
    document.getElementById('m-poster').src = poster;
    document.getElementById('m-year').innerText = year;
    document.getElementById('m-category').innerText = category;
    document.getElementById('m-rating').innerText = rating;
    const modal = document.getElementById('deleteModal');
    modal.classList.remove('hidden');
    document.body.classList.add('no-scroll');
}

function closeDeleteModal() {
    const modal = document.getElementById('deleteModal');
    modal.classList.add('hidden');
    document.body.classList.remove('no-scroll');
}