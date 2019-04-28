import { KronisHue } from "./kronishue.js";

$(function () {
    let hue = new KronisHue();
    if (hue.storeCode()) {
        $("#result").text("Authorized successfully");
    }
    else {
        $("#result").text("Authorization failed");
    }
});