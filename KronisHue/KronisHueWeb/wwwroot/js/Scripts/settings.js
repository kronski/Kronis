import * as k from "./kronishue.js";
$(function () {
    let hue = new k.KronisHue();
    $("#authlink").attr("href", hue.getAuthLink());
});
