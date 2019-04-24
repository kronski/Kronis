import * as md5 from '../node_modules/ts-md5/dist/md5.js';

export class KronisHue {
    code: string | null;
    state: string | null;
    deviceid: string | null;
    devicename: string | null;
    clientid: string | null;
    appid: string | null;

    constructor() {
        this.code = window.localStorage.getItem("code");
        this.state = window.localStorage.getItem("state");

        this.deviceid = window.localStorage.getItem("deviceid");
        if (!this.deviceid) {
            this.deviceid = this.getRandomStr(64);

            window.localStorage.setItem("deviceid", this.deviceid);
        }
        this.devicename = this.deviceid;
        this.clientid = "RZNWVvJBddeczq9ekYvi3nnRMCYi9jdU";
        this.appid = "kronishue";
    }

    getRandomStr(length: number): string {
        var random_num = new Uint8Array(length);
        window.crypto.getRandomValues(random_num);

        let str = "";
        random_num.forEach(int => {
            str += int.toString(16);
        });

        return str;

    }

    getAuthLink() {
        this.state = this.getRandomStr(64);
        window.localStorage.setItem("state", this.state);

        const linkhref = `https://api.meethue.com/oauth2/auth?clientid=${this.clientid}&appid=${this.appid}&deviceid=${this.deviceid}&devicename=${this.devicename}&state=${this.state}&response_type=code`;

        return linkhref;
    }


    storeCode(): boolean {
        let state = window.localStorage.getItem("state");
        let url_string = window.location.href
        let url = new URL(url_string);
        let requeststate = url.searchParams.get("state");
        let requestcode = url.searchParams.get("code");

        if (state == requeststate) {
            if (requestcode)
                window.localStorage.setItem("code", requestcode);
            return true;
        }
        else {
            return false;
        }
    }

    async getNonce() {
        await $.ajax({
            url: `https://api.meethue.com/oauth2/token?code=${this.code}&grant_type=authorization_code`,
            type: 'POST'
        });
    }

    async getTokenWithDigest(nonce: string) {
        let HASH1 = md5.Md5.hashStr("kVWjgzqk8hayM38pAudrA6psflju6k0T:oauth2_client@api.meethue.com:GHFV3f4L736bwgEB");
        let HASH2 = md5.Md5.hashStr("POST:/oauth2/token");
        let response = md5.Md5.hashStr(HASH1 + ":" + "7b6e45de18ac4ee452ee0a0de91dbb10" + ":" + HASH2);

        let authheader = `Digest username="${this.clientid}", realm="oauth2_client@api.meethue.com", nonce="${nonce}", uri="/oauth2/token", response="${response}"`;

        await $.ajax({
            url: `https://api.meethue.com/oauth2/token?code=${this.code}&grant_type=authorization_code`,
            headers: {
                "Authorization": authheader
            }
        });
    }
}