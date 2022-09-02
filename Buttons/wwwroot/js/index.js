'use strict';

const statusToDisplay = {
    'Uploaded': 'Editing',
    'Confirmed': 'Waiting for Print',
    'Printed': 'Printed',
}

/**
 * @param {Element} button
 * @param {string} editLink
 */
function onButtonClicked(button, editLink, removeLink) {
    const data = {
        id: button.getAttribute('data-id'),
        status: button.getAttribute('data-status'),
    };

    const detailView = document.getElementById('detail-view');
    const sourceImage = button.getElementsByClassName('button-image')[0];
    const targetImage = detailView.getElementsByClassName('button-image')[0];

    targetImage.setAttribute('src', sourceImage.getAttribute('src'));
    targetImage.style['width'] = sourceImage.style['width'];
    targetImage.style['height'] = sourceImage.style['height'];
    targetImage.style['left'] = sourceImage.style['left'];
    targetImage.style['top'] = sourceImage.style['top'];

    const status = document.getElementById('detail-status');
    status.innerText = statusToDisplay[data.status];

    const edit = document.getElementById('detail-edit');
    edit.setAttribute('href', editLink.replace('__id__', data.id));

    const remove = document.getElementById('detail-remove');
    remove.setAttribute('href', removeLink.replace('__id__', data.id));

    const buttonStatus = data.status === 'Printed' ? 'none' : 'flex';
    edit.style['display'] = buttonStatus;
    remove.style['display'] = buttonStatus;

    detailView.style['display'] = 'flex';
}

window.addEventListener('DOMContentLoaded', () => {
    const detailView = document.getElementById('detail-view');

    const edit = document.getElementById('detail-edit');
    const editLink = edit.getAttribute('href');
    const remove = document.getElementById('detail-remove');
    const removeLink = remove.getAttribute('href');

    const items = document.getElementsByClassName('overview-item');
    for (const item of items) {
        const button = item.getElementsByClassName('button-outer')[0];
        button.addEventListener('click', () => onButtonClicked(button, editLink, removeLink));
    }

    const closeButton = document.getElementById('detail-close');
    closeButton.addEventListener('click', () => detailView.style['display'] = 'none');

    // Close detail page when background is clicked.
    detailView.addEventListener('click', event => {
        if (event.target === detailView) {
            detailView.style['display'] = 'none';
        }
    });

    remove.addEventListener('click', event => {
        if (!confirm('Do you really want to delete this button?')) {
            event.preventDefault();
        }
    })
});
