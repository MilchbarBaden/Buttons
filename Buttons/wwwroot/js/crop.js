'use strict';

const dataFields = ['x', 'y', 'width', 'height', 'scaleX', 'scaleY'];

function getData() {
    const data = {};
    for (const field of dataFields) {
        const elements = document.getElementsByName(field);
        let value;
        if (elements.length > 0) {
            const attribute = elements[0].getAttribute('value');
            value = parseFloat(attribute);
        } else {
            console.warn(`${field} not found`);
            value = 0;
        }
        data[field] = value;
    }
    return data;
}

function addOrSetData(form, name, value) {
    const elements = document.getElementsByName(name);

    for (const element of elements) {
        element.setAttribute('value', value);
    }

    if (elements.length === 0) {
        const element = document.createElement('input');
        element.setAttribute('type', 'hidden');
        element.setAttribute('name', name);
        element.setAttribute('value', value);
        form.append(element);
    }
}

function formSubmit(event, cropper) {
    const form = document.getElementById('crop-form');
    const data = cropper.getData();

    for (const field of dataFields) {
        addOrSetData(form, field, data[field]);
    }
}

function setupCropper(cropper, data) {
    if (data.width < 1e-5) {
        return;
    }

    cropper.zoomTo(1);
    const crop = cropper.getData();
    cropper.zoomTo(crop.width / data.width);

    const box = cropper.getCropBoxData();
    const factor = {
        x: box.width / data.width,
        y: box.height / data.height,
    }
    cropper.moveTo(box.left - data.x * factor.x, box.top - data.y * factor.y);
}

window.addEventListener('DOMContentLoaded', () => {
    const image = document.getElementById('crop-image');
    const form = document.getElementById('crop-form');
    const reset = document.getElementById('crop-reset');

    const cropper = new Cropper(image, {
        dragMode: 'move',
        aspectRatio: 1,
        autoCropArea: 0.9,
        guides: false,
        cropBoxMovable: false,
        cropBoxResizable: false,
        toggleDragModeOnDblclick: false,

        ready() {
            const data = getData();
            setupCropper(this.cropper, data);
        },
    });

    form.addEventListener('submit', event => formSubmit(event, cropper));
    reset.addEventListener('click', _ => cropper.reset());
});
