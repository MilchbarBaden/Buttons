'use strict';

function setFilePreviews(src, visible) {
    const previews = document.getElementsByClassName('file-preview');
    for (const preview of previews) {
        preview.setAttribute('src', src);
        if (visible) {
            preview.classList.add('visible');
        } else {
            preview.classList.remove('visible');
        }
    }

}

function onFileChange(event) {
    const submit = document.querySelector('button[type=submit]');

    const files = [...event.target.files];
    const file = files.find(f => f.type.startsWith('image/'));

    if (!file) {
        submit.setAttribute('disabled', 'disabled');
        setFilePreviews('//:0', false);
        alert('Invalid File! Plese select something else.');
        return;
    }

    const reader = new FileReader();

    reader.onload = event => {
        setFilePreviews(event.target.result, true);
        submit.removeAttribute('disabled');
    };

    reader.readAsDataURL(file);
}

document.getElementById('file').addEventListener('change', onFileChange);