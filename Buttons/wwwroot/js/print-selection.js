'use strict';

const checked = new Set();
let buttonData = [];

/**
 * @param {HTMLInputElement} checkbox
 */
function checkboxChanged(checkbox, button) {
    if (checkbox.checked) {
        checked.add(button.Id);
    } else {
        checked.delete(button.Id);
    }

    const selectionCount = document.getElementById('selection-count');
    selectionCount.innerText = `${checked.size}`;
}

function createButton(button) {
    const cropData = flattenCropData(button.Crop);

    const outer = document.createElement('div');
    outer.classList.add('selection-button');

    const checkbox = document.createElement('input');
    const checkboxId = `check-${button.Id}`;
    checkbox.setAttribute('type', 'checkbox');
    checkbox.setAttribute('id', checkboxId);
    checkbox.setAttribute('name', 'buttons');
    checkbox.setAttribute('value', button.Id);
    checkbox.addEventListener('change', () => checkboxChanged(checkbox, button));

    const label = document.createElement('label')
    label.setAttribute('for', checkboxId);

    const buttonOuter = document.createElement('div');
    buttonOuter.classList.add('button-outer');
    buttonOuter.setAttribute('data-id', button.Id);
    buttonOuter.setAttribute('data-status', button.Status);

    const image = document.createElement('img');
    image.addEventListener('load', () =>
        cropButtonWithData(buttonOuter, cropData));
    image.classList.add('button-image');
    image.setAttribute('src', `/buttons/${button.Path}`);

    buttonOuter.appendChild(image);
    label.appendChild(buttonOuter);
    outer.appendChild(checkbox);
    outer.appendChild(label);

    cropButtonWithData(buttonOuter, cropData);

    return outer;
}

window.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('selection-form');
    form.addEventListener('submit', () => {
        setTimeout(() => window.location.reload(), 1000);
    });
});

window.addEventListener('load', () => {
    const buttonContainer = document.getElementById('selection-items');
    const jsonScript = document.getElementById('selection-data');
    buttonData = JSON.parse(jsonScript.textContent);

    for (const button of buttonData) {
        const element = createButton(button);
        buttonContainer.appendChild(element);
    }

    if (buttonData.length == 0) {
        const empty = document.createElement('h2');
        empty.innerText = 'There are no buttons yet';
        buttonContainer.appendChild(empty);
    }
});
