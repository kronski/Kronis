$(function () {
    function getRandomStr(length: number): string {
        var random_num = new Uint8Array(length);
        window.crypto.getRandomValues(random_num);

        let str = "";
        random_num.forEach(int => {
            str += int.toString(16);
        });

        return str;

    }


    const clientid: string = "RZNWVvJBddeczq9ekYvi3nnRMCYi9jdU";
    const appid: string = "kronishue";

    let state = getRandomStr(64);

    let deviceid = window.localStorage.getItem("deviceid");
    if (!deviceid) {
        deviceid = getRandomStr(64);

        window.localStorage.setItem("deviceid", deviceid);
    }
    let devicename = deviceid;

    window.localStorage.setItem("state", state);

    const linkhref = `https://api.meethue.com/oauth2/auth?clientid=${clientid}&appid=${appid}&deviceid=${deviceid}&devicename=${devicename}&state=${state}&response_type=code`;

    document.querySelector("#authlink").setAttribute("href", linkhref);
});
