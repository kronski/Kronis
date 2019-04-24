import { KronisHue } from "./kronishue";

$(function () {
    let hue = new KronisHue();
    if (hue.storeCode()) {
        $("#result").text("Authorized successfully");
    }
    else {
        $("#result").text("Authorization failed");
    }
});