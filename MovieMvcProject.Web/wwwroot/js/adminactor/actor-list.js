function openDeleteModal(id, name, avatar, birthdate, moviecount) {
    document.getElementById('a-id-input').value = id;
    document.getElementById('a-name').innerText = name;
    document.getElementById('a-confirm-text').innerText = `"${name}"`;
    document.getElementById('a-avatar').src = avatar || "/profile-images/default-profile.png";
    document.getElementById('a-birthdate').innerText = birthdate || "N/A";
    document.getElementById('a-moviecount').innerText = moviecount + " film";

    document.getElementById('deleteModal').classList.remove('hidden');
    document.body.classList.add('no-scroll');
}

function closeDeleteModal() {
    document.getElementById('deleteModal').classList.add('hidden');
    document.body.classList.remove('no-scroll');
}