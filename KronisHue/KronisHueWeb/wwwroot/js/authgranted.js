$(function () {
    var state = window.localStorage.getItem("state");
    var url_string = window.location.href;
    var url = new URL(url_string);
    var requeststate = url.searchParams.get("state");
    var requestcode = url.searchParams.get("code");
    if (state == requeststate) {
        window.localStorage.setItem("code", requestcode);
        $("#result").text("Code set");
    }
    else {
        $("#result").text("Code not set");
    }
});
//# sourceMappingURL=authgranted.js.map