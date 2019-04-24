var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import * as md5 from '../node_modules/ts-md5/src/md5.js';
export class KronisHue {
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
    getRandomStr(length) {
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
    storeCode() {
        let state = window.localStorage.getItem("state");
        let url_string = window.location.href;
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
    getNonce() {
        return __awaiter(this, void 0, void 0, function* () {
            yield $.ajax({
                url: `https://api.meethue.com/oauth2/token?code=${this.code}&grant_type=authorization_code`,
                type: 'POST'
            });
        });
    }
    getTokenWithDigest(nonce) {
        return __awaiter(this, void 0, void 0, function* () {
            let HASH1 = md5.Md5.hashStr("kVWjgzqk8hayM38pAudrA6psflju6k0T:oauth2_client@api.meethue.com:GHFV3f4L736bwgEB");
            let HASH2 = md5.Md5.hashStr("POST:/oauth2/token");
            let response = md5.Md5.hashStr(HASH1 + ":" + "7b6e45de18ac4ee452ee0a0de91dbb10" + ":" + HASH2);
            let authheader = `Digest username="${this.clientid}", realm="oauth2_client@api.meethue.com", nonce="${nonce}", uri="/oauth2/token", response="${response}"`;
            yield $.ajax({
                url: `https://api.meethue.com/oauth2/token?code=${this.code}&grant_type=authorization_code`,
                headers: {
                    "Authorization": authheader
                }
            });
        });
    }
}
