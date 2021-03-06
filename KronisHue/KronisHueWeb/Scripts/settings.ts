﻿import { KronisHue } from "./kronishue.js";

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

        $("#baseUrl").val(hue.data.baseUrl);
        $("#localusername").val(hue.data.localusername);
        $("#autoRefreshState").prop('checked', hue.data.autoRefreshState);
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


    $("#autoRefreshState").click(function () {
        hue.data.autoRefreshState = $("#autoRefreshState").prop('checked');
        hue.saveSettings();
    }); 

    $("#baseUrl").click(function () {
        hue.data.baseUrl = <string>$("#baseUrl").val();
        hue.saveSettings();
    }); 

    $("#locateHueButton").click(function () {
        hue.locateHue().then(() => {
            refreshSettings();
        });
    }); 

    $("#registerLocalHueButton").click(function () {
        hue.registerLocalHue().then(() => {
            $("#baseUrlIcon").removeClass('fa-times')
            $("#baseUrlIcon").addClass('fa-check')
            $("#localusername-feedback").hide()

            refreshSettings();
        }).catch((msg) => {
            $("#baseUrlIcon").removeClass('fa-check')
            $("#baseUrlIcon").addClass('fa-times')
            $("#localusername-feedback").text(msg).show()

            refreshSettings();
        });
    }); 

    refreshSettings();
});