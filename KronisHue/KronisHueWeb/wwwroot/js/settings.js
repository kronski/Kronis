$(function () {
    let hue = new KronisHue();
    $("#authlink").attr("href", hue.getAuthLink());
});
