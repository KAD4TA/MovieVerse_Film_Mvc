function openDeleteModal(id, name, photoUrl) {
    document.getElementById('modalId').value = id;
    document.getElementById('modalName').textContent = name;
    document.getElementById('modalPhoto').src = photoUrl || '/images/default-director.jpg';

    const modal = document.getElementById('deleteModal');
    if (modal) {
        modal.classList.remove('hidden');
        modal.classList.add('flex');
    }
}

function closeDeleteModal() {
    const modal = document.getElementById('deleteModal');
    if (modal) {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
    }
}