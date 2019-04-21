$(function () {
    var clientid = "RZNWVvJBddeczq9ekYvi3nnRMCYi9jdU";
    var appid = "kronishue";
    var state = window.localStorage.getItem("state");
    var url_string = window.location.href;
    var url = new URL(url_string);
    var requeststate = url.searchParams.get("state");
    var requestcode = url.searchParams.get("code");
    //asd https://kronishue.azurewebsites.net/app/?code=twTAXjGc&state=231c2ea6a1ec3632418b115212372d65c65c61b5068131fe5dae3e51c61580b98dee13d99f2ff4a29dd4235fcd64975672fcd72e9ea0e2cfc1b644034be
    if (state == requeststate) {
        window.localStorage.setItem("code", requestcode);
        $("#result").text("Code set");
    }
    else {
        $("#result").text("Code not set");
    }
});
//# sourceMappingURL=authgranted.js.map