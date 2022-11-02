'use strict';

function flattenCropData(nestedData) {
    return {
        x: nestedData.OffsetX,
        y: nestedData.OffsetY,
        width: nestedData.Width,
        height: nestedData.Height,
        scaleX: nestedData.ScaleX,
        scaleY: nestedData.ScaleY,
    };
}

/**
 * @param {Element} script
 */
function getDataFromJson(script) {
    const jsonData = JSON.parse(script.textContent);
    return flattenCropData(jsonData);
}

/**
 * @param {Element} outer
 */
function cropButtonWithData(outer, data) {
    const image = outer.getElementsByClassName('button-image')[0];
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
}

/**
 * @param {Element} outer
 */
function cropButton(outer) {
    const scripts = outer.getElementsByTagName('script');
    if (scripts.length !== 1) {
        return;
    }

    const data = getDataFromJson(scripts[0]);
    cropButtonWithData(outer, data);
}

window.addEventListener('load', () => {
    const buttons = document.getElementsByClassName('button-outer');
    for (const outer of buttons) {
        cropButton(outer);
    }
});
