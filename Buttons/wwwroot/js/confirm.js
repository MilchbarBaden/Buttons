'use strict';

window.addEventListener('load', () => {
    const image = document.getElementById('confirm-image');
    const data = getData();
    const width = image.naturalWidth;
    const height = image.naturalHeight;
    const factor = {
        x: width / data.width,
        y: height / data.height,
    };
    image.style['width'] = `${factor.x * 100}%`;
    image.style['height'] = `${factor.y * 100}%`;
    image.style['left'] = `${(-data.x / width) * factor.x * 100}%`;
    image.style['top'] = `${(-data.y / height) * factor.y * 100}%`;
});
