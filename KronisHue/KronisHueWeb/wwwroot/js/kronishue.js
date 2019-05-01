var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
class KronisHueData {
}
;
class LightState {
}
class LightSWUpdate {
}
class LightCapabilitiesControlCT {
}
class LightCapabilitiesControl {
}
class LightCapabilitiesStreaming {
}
class LightCapabilities {
}
class LightConfig {
}
class Light {
}
class Lights {
}
export class KronisHue {
    constructor(useLocalstorage = true) {
        this.useLocalstorage = useLocalstorage;
        this.loadFromLocalStorage();
    }
    loadFromLocalStorage() {
        if (!this.useLocalstorage)
            return;
        this.loadDataFromLocalStorage();
        this.loadLightsFromLocalStorage();
    }
    loadDataFromLocalStorage() {
        let jsonstr = window.localStorage.getItem("kronisHueData");
        this.data = (jsonstr) ? JSON.parse(jsonstr) : new KronisHueData();
    }
    loadLightsFromLocalStorage() {
        let jsonstr = window.localStorage.getItem("kronisHueLights");
        this.lights = (jsonstr) ? JSON.parse(jsonstr) : null;
    }
    saveDataToLocalStorage() {
        if (!this.useLocalstorage)
            return;
        var jsonstr = JSON.stringify(this.data);
        window.localStorage.setItem("kronisHueData", jsonstr);
    }
    saveLightsToLocalStorage() {
        if (!this.useLocalstorage)
            return;
        var jsonstr = JSON.stringify(this.lights);
        window.localStorage.setItem("kronisHueLights", jsonstr);
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
        this.data.state = this.getRandomStr(64);
        this.saveDataToLocalStorage();
        return fetch("/api/kronishue/appinfo", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                nonce: this.data.nonce,
                code: this.data.code
            })
        }).then((response) => {
            return response.json().then((o) => {
                const linkhref = `https://api.meethue.com/oauth2/auth?clientid=${o.clientid}&appid=${o.appid}&deviceid=${this.data.deviceid}&devicename=${this.data.devicename}&state=${this.data.state}&response_type=code`;
                return linkhref;
            });
        }).catch(() => {
            return null;
        });
    }
    storeCode() {
        let state = this.data.state;
        let url_string = window.location.href;
        let url = new URL(url_string);
        let requeststate = url.searchParams.get("state");
        let requestcode = url.searchParams.get("code");
        if (state == requeststate) {
            if (requestcode) {
                this.data.code = requestcode;
                this.saveDataToLocalStorage();
                return true;
            }
            return false;
        }
        else {
            return false;
        }
    }
    refreshNonce() {
        return __awaiter(this, void 0, void 0, function* () {
            return fetch('/api/kronishue/nonce', {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ code: this.data.code })
            }).then((response) => {
                return response.text().then((nonce) => {
                    this.data.nonce = nonce;
                    this.saveDataToLocalStorage();
                    return this.data.nonce;
                });
            }).catch(() => {
                this.data.nonce = null;
                this.saveDataToLocalStorage();
                return this.data.nonce;
            });
        });
    }
    getTokenWithDigest() {
        return __awaiter(this, void 0, void 0, function* () {
            return fetch("/api/kronishue/token", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    nonce: this.data.nonce,
                    code: this.data.code
                })
            }).then((response) => {
                if (response.ok) {
                    return response.json().then((data) => {
                        this.data.access_token = data.access_token;
                        this.data.access_token_expires_in = data.access_token_expires_in;
                        this.data.refresh_token = data.refresh_token;
                        this.data.refresh_token_expires_in = data.refresh_token_expires_in;
                        this.data.token_type = data.token_type;
                        this.saveDataToLocalStorage();
                        return data;
                    });
                }
            }).catch(() => {
                return null;
            });
        });
    }
    refreshTokenWithDigest() {
        return __awaiter(this, void 0, void 0, function* () {
            return fetch("/api/kronishue/refreshtoken", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    nonce: this.data.nonce,
                    code: this.data.code,
                    refresh_token: this.data.refresh_token
                })
            }).then((response) => {
                if (response.ok) {
                    return response.json().then((data) => {
                        this.data.access_token = data.access_token;
                        this.data.access_token_expires_in = data.access_token_expires_in;
                        this.data.refresh_token = data.refresh_token;
                        this.data.refresh_token_expires_in = data.refresh_token_expires_in;
                        this.data.token_type = data.token_type;
                        this.saveDataToLocalStorage();
                        return data;
                    });
                }
            }).catch(() => {
                return null;
            });
        });
    }
    setDeviceType() {
        return __awaiter(this, void 0, void 0, function* () {
            return fetch("/api/kronishue/setdevicetype", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    token: this.data.access_token,
                    devicetype: "kronishueweb"
                })
            }).then((response) => {
                if (response.ok) {
                    return response.json().then((data) => {
                        this.data.username = data.username;
                        this.saveDataToLocalStorage();
                        return data;
                    });
                }
            }).catch(() => {
                return null;
            });
        });
    }
    getLights(refresh = false) {
        return __awaiter(this, void 0, void 0, function* () {
            if (!refresh && this.lights)
                return this.lights;
            return fetch("/api/kronishue/lights", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    token: this.data.access_token,
                    username: this.data.username
                })
            }).then((response) => {
                if (response.ok) {
                    return response.json().then((data) => {
                        this.lights = data;
                        for (let id in this.lights) {
                            this.lights[id].id = id;
                        }
                        this.saveLightsToLocalStorage();
                        return data;
                    });
                }
                else
                    return null;
            }).catch(() => {
                return null;
            });
        });
    }
}
//# sourceMappingURL=kronishue.js.map