'use strict';

function onFileChange(event) {
    const files = [...event.target.files];
    const file = files.find(f => f.type.startsWith('image/'));

    if (!file) {
        alert("Invalid File! Plese select something else.")
        return;
    }

    const reader = new FileReader();

    reader.onload = event => {
        const previews = document.getElementsByClassName('file-preview');
        for (const preview of previews) {
            preview.setAttribute('src', event.target.result);
            preview.classList.add('visible');
        }
    };

    reader.readAsDataURL(file);
}

document.getElementById('file').addEventListener('change', onFileChange);