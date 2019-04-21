$(function () {
    const clientid: string = "RZNWVvJBddeczq9ekYvi3nnRMCYi9jdU";
    const appid: string = "kronishue";

    let state = window.localStorage.getItem("state");

    let url_string = window.location.href
    let url = new URL(url_string);
    let requeststate = url.searchParams.get("state");
    let requestcode = url.searchParams.get("code");

    //asd https://kronishue.azurewebsites.net/app/?code=twTAXjGc&state=231c2ea6a1ec3632418b115212372d65c65c61b5068131fe5dae3e51c61580b98dee13d99f2ff4a29dd4235fcd64975672fcd72e9ea0e2cfc1b644034be

    if (state == requeststate) {
        window.localStorage.setItem("code", requestcode);
        $("#result").text("Code set");
    }
    else {
        $("#result").text("Code not set");
    }

});
