'use strict';

const checked = new Set();
let buttonData = [];

function getFilters() {
    const search = document.getElementById('selection-filter').value;
    const showNotPrinted = document.getElementById('selection-not-printed').checked;
    const showPrinted = document.getElementById('selection-printed').checked;
    return { search, showNotPrinted, showPrinted };
}

function buttonMatchesFilter(button, filter) {
    const show = button.Status === 'Printed' ? filter.showPrinted : filter.showNotPrinted;
    const search = filter.search.toLowerCase();
    const matchesSearch =
        button.Name.toLowerCase().includes(search) ||
        button.OwnerName.toLowerCase().includes(search) ||
        `${button.Id}`.toLowerCase() === search.trim() ||
        (search.length >= 3 && `${button.OwnerId}`.toLowerCase().includes(search));
    return show && matchesSearch;
}

function reloadSelectionItems() {
    const buttonContainer = document.getElementById('selection-items');
    const filter = getFilters();

    buttonContainer.innerText = '';
    let addedCount = 0;
    for (const button of buttonData) {
        if (buttonMatchesFilter(button, filter)) {
            const element = createButton(button);
            buttonContainer.appendChild(element);
            ++addedCount;
        }
    }

    if (addedCount === 0) {
        const empty = document.createElement('h2');
        empty.innerText = buttonData.length === 0 ?
            'There are no buttons yet' :
            'No buttons match the filters';
        buttonContainer.appendChild(empty);
    }
}

/**
 * @param {HTMLInputElement} checkbox
 */
function checkboxChanged(checkbox, button) {
    if (checkbox.checked) {
        checked.add(button.Id);
    } else {
        checked.delete(button.Id);
    }

    const selectionCounts = document.getElementsByClassName('selection-count');
    for (const selectionCount of selectionCounts) {
        selectionCount.innerText = `${checked.size}`;
    }
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

    if (checked.has(button.Id)) {
        checkbox.setAttribute('checked', 'checked');
    }

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
    image.setAttribute('title', `${button.Name} by ${button.OwnerName} [${button.Id} by ${button.OwnerId}]`);

    buttonOuter.appendChild(image);
    label.appendChild(buttonOuter);
    outer.appendChild(checkbox);
    outer.appendChild(label);

    cropButtonWithData(buttonOuter, cropData);

    return outer;
}

function selectAll() {
    const buttonCheckboxes = document.getElementsByName('buttons');
    let allChecked = true;
    for (const checkbox of buttonCheckboxes) {
        allChecked = allChecked && checkbox.checked;
        if (!checkbox.checked) {
            // Using click to check AND fire the change event.
            checkbox.click();
        }
    }

    if (allChecked) {
        for (const checkbox of buttonCheckboxes) {
            if (checkbox.checked) {
                checkbox.click();
            }
        }
    }
}

window.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('selection-form');
    form.addEventListener('submit', () => {
        setTimeout(() => window.location.reload(), 1000);
    });

    const selectAllButton = document.getElementById('selection-select-all');
    selectAllButton.addEventListener('click', selectAll);

    const deleteButton = document.getElementsByName('delete-selection')[0];
    deleteButton.addEventListener('click', e => {
        if (checked.size === 0 || !confirm(`Do you really want do delete ${checked.size} button(s)?`)) {
            e.preventDefault();
        } else {
            // Submit the form, but do not open a new tab in the delete case.
            form.setAttribute('target', '_self');
        }
    });

    const checkboxElements = [
        document.getElementById('selection-not-printed'),
        document.getElementById('selection-printed')
    ];
    for (const filter of checkboxElements) {
        filter.addEventListener('change', () => reloadSelectionItems());
    }

    const filterElement = document.getElementById('selection-filter');
    filterElement.addEventListener('input', () => reloadSelectionItems());
});

window.addEventListener('load', () => {
    const jsonScript = document.getElementById('selection-data');
    buttonData = JSON.parse(jsonScript.textContent);

    reloadSelectionItems();
});
