'use strict';

//<div class="button-outer" data-id="@Model.Id" data-status="@Model.Status">
//    <img class="button-image" src="~/buttons/@Model.Path" />
//    @{
//        var options = new JsonSerializerOptions() {IncludeFields = true};
//    var json = JsonSerializer.Serialize(@Model.Crop, options);
//    <script type="application/json">@Html.Raw(json)</script>
//    }
//</div>

function createButton(button) {
    const cropData = flattenCropData(button.Crop);

    const outer = document.createElement('div');
    outer.classList.add('selection-button');

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
    outer.appendChild(buttonOuter);

    cropButtonWithData(buttonOuter, cropData);

    return outer;
}

window.addEventListener('load', () => {
    const buttonContainer = document.getElementById('selection-items');
    const jsonScript = document.getElementById('selection-data');
    const data = JSON.parse(jsonScript.textContent);

    for (let button of data) {
        const element = createButton(button);
        buttonContainer.appendChild(element);
    }
});