'use strict';

window.addEventListener('load', () => {
    const buttonCount = document.getElementsByClassName('print-button').length;
    if (buttonCount > 0) {
        window.print();
    }
    window.close();
});
