function previewFile(input) {
    const preview = document.getElementById('previewImage');
    const file = input.files[0];
    const reader = new FileReader();
    reader.onloadend = function () { preview.src = reader.result; }
    if (file) { reader.readAsDataURL(file); }
}