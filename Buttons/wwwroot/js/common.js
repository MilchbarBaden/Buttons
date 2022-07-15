'use strict';

/**
 * @param {Element} script
 */
function getDataFromJson(script) {
    const jsonData = JSON.parse(script.textContent);
    return {
        x: jsonData.Offset.Item1,
        y: jsonData.Offset.Item2,
        width: jsonData.Size.Item1,
        height: jsonData.Size.Item2,
        scaleX: jsonData.Scale.Item1,
        scaleY: jsonData.Scale.Item2,
    };
}

/**
 * @param {Element} outer
 */
function cropButton(outer) {
    const image = outer.getElementsByClassName('button-image')[0];
    const scripts = outer.getElementsByTagName('script');
    if (scripts.length != 1) {
        return;
    }

    const jsonScript = scripts[0];
    const data = getDataFromJson(jsonScript);
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

window.addEventListener('load', () => {
    const buttons = document.getElementsByClassName('button-outer');
    for (const outer of buttons) {
        cropButton(outer);
    }
});
