'use strict';

var cropper;

window.addEventListener('DOMContentLoaded', () => {
    const image = document.getElementById('crop-image');
    cropper = new Cropper(image, {
        dragMode: 'move',
        aspectRatio: 1,
        autoCropArea: 0.9,
        guides: false,
        //highlight: false,
        cropBoxMovable: false,
        cropBoxResizable: false,
        toggleDragModeOnDblclick: false,
    });
});