$(function () {
    let state = window.localStorage.getItem("state");

    let url_string = window.location.href
    let url = new URL(url_string);
    let requeststate = url.searchParams.get("state");
    let requestcode = url.searchParams.get("code");

    if (state == requeststate) {
        window.localStorage.setItem("code", requestcode);
        $("#result").text("Code set");
    }
    else {
        $("#result").text("Code not set");
    }

});
