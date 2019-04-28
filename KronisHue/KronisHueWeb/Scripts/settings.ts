import { KronisHue } from "./kronishue.js";

$(function () {
    let hue = new KronisHue();

    function refreshSettings() {
        $("#codeLabel").val(hue.data.code || "Not set");
        $("#nonceLabel").val(hue.data.nonce || "Not set");

        $("#accessToken").val(hue.data.access_token || "Not set");
        $("#accessTokenExpires").val(hue.data.access_token_expires_in || "Not set");
        $("#refreshToken").val(hue.data.refresh_token || "Not set");
        $("#refreshTokenExpires").val(hue.data.refresh_token_expires_in || "Not set");

        $("#username").val(hue.data.username || "Not set");
    }

    $("#authButton").click(function () {
        hue.getAuthLink().then((url) => {
            if(url)
                window.location.href = url;
        });
    }); 

    $("#nonceButton").click(function () {
        hue.refreshNonce().then((data) => {
            if (data) {
                refreshSettings();
            }
        });
        
    }); 

    $("#tokenButton").click(function () {
        hue.getTokenWithDigest().then((data) => {
            refreshSettings();
        });
    }); 

    $("#refreshTokenButton").click(function () {
        hue.refreshTokenWithDigest().then((data) => {
            refreshSettings();
        });
    }); 

    $("#usernameButton").click(function () {
        hue.setDeviceType().then((data) => {
            refreshSettings();
        });
    }); 


    refreshSettings();
});