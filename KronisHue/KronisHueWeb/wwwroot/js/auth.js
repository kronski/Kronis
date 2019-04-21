$(function () {
    function getRandomStr(length) {
        var random_num = new Uint8Array(length);
        window.crypto.getRandomValues(random_num);
        var str = "";
        random_num.forEach(function (int) {
            str += int.toString(16);
        });
        return str;
    }
    var clientid = "RZNWVvJBddeczq9ekYvi3nnRMCYi9jdU";
    var appid = "kronishue";
    var state = getRandomStr(64);
    var deviceid = window.localStorage.getItem("deviceid");
    if (!deviceid) {
        deviceid = getRandomStr(64);
        window.localStorage.setItem("deviceid", deviceid);
    }
    var devicename = deviceid;
    window.localStorage.setItem("state", state);
    var linkhref = "https://api.meethue.com/oauth2/auth?clientid=" + clientid + "&appid=" + appid + "&deviceid=" + deviceid + "&devicename=" + devicename + "&state=" + state + "&response_type=code";
    document.querySelector("#authlink").setAttribute("href", linkhref);
});
//# sourceMappingURL=auth.js.map