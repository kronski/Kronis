var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
class KronisHueData {
    constructor() {
        this.baseUrl = "";
        this.autoRefreshState = false;
    }
}
;
export class LightState {
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
export class GroupAction {
}
class Group {
}
class Groups {
}
const localStorageDataKey = "kronisHueData", localStorageLightsKey = "kronisHueLights", localStorageGroupsKey = "kronisHueGroups";
export class KronisHue {
    constructor(useLocalstorage = true) {
        this.useLocalstorage = useLocalstorage;
        this.loadFromLocalStorage();
    }
    canRefresh() {
        if ((this.data.access_token && this.data.username) || this.data.baseUrl)
            return true;
        return false;
    }
    loadFromLocalStorage() {
        if (!this.useLocalstorage)
            return;
        this.data = this.loadAnyFromLocalStorage(localStorageDataKey) || new KronisHueData();
        this.lights = this.loadAnyFromLocalStorage(localStorageLightsKey) || new Lights();
        this.groups = this.loadAnyFromLocalStorage(localStorageGroupsKey) || new Groups();
    }
    loadAnyFromLocalStorage(key) {
        let jsonstr = window.localStorage.getItem(key);
        return (jsonstr) ? JSON.parse(jsonstr) : null;
    }
    saveSettings() {
        this.saveAnyToLocalStorage(localStorageDataKey, this.data);
    }
    saveAnyToLocalStorage(key, data) {
        if (!this.useLocalstorage)
            return;
        var jsonstr = JSON.stringify(data);
        window.localStorage.setItem(key, jsonstr);
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
        this.saveAnyToLocalStorage(localStorageDataKey, this.data);
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
                this.saveAnyToLocalStorage(localStorageDataKey, this.data);
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
                    this.saveAnyToLocalStorage(localStorageDataKey, this.data);
                    return this.data.nonce;
                });
            }).catch(() => {
                this.data.nonce = null;
                this.saveAnyToLocalStorage(localStorageDataKey, this.data);
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
                        this.saveAnyToLocalStorage(localStorageDataKey, this.data);
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
                        this.saveAnyToLocalStorage(localStorageDataKey, this.data);
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
                        this.saveAnyToLocalStorage(localStorageDataKey, this.data);
                        return data;
                    });
                }
            }).catch(() => {
                return null;
            });
        });
    }
    locateHue() {
        return __awaiter(this, void 0, void 0, function* () {
            return fetch("/api/kronishue/locateHue", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({})
            }).then((response) => {
                if (response.ok) {
                    return response.json().then((data) => {
                        if (data) {
                            let items = data;
                            if (items.length > 0) {
                                let ip = items[0];
                                this.data.baseUrl = `http://${ip}/api`;
                                this.saveAnyToLocalStorage(localStorageDataKey, this.data);
                                return this.data.baseUrl;
                            }
                        }
                        return null;
                    });
                }
                else
                    return null;
            }).catch(() => {
                return null;
            });
        });
    }
    registerLocalHue() {
        return __awaiter(this, void 0, void 0, function* () {
            return fetch("/api/kronishue/registerLocalHue", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(this.getApiInput())
            }).then((response) => {
                if (!response.ok)
                    throw "Request failed";
                return response.json().then((data) => {
                    if (data && data[0] && data[0].error && data[0].error.description)
                        throw data[0].error.description;
                    if (!data || !data[0] || !data[0].success || !data[0].success.username)
                        throw "Invalid result";
                    this.data.localusername = data[0].success.username;
                    this.saveAnyToLocalStorage(localStorageDataKey, this.data);
                });
            });
        });
    }
    getApiInput() {
        let baseurl = this.data.baseUrl;
        if (baseurl && this.data.localusername)
            baseurl += "/" + this.data.localusername;
        return {
            token: !baseurl ? this.data.access_token : null,
            username: !baseurl ? this.data.username : null,
            baseUrl: baseurl
        };
    }
    getLights(refresh = false) {
        return __awaiter(this, void 0, void 0, function* () {
            if (!refresh && this.lights && !this.data.autoRefreshState)
                return this.lights;
            return fetch("/api/kronishue/lights", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(this.getApiInput())
            }).then((response) => {
                if (response.ok) {
                    return response.json().then((data) => {
                        this.lights = data;
                        for (let id in this.lights) {
                            this.lights[id].id = id;
                        }
                        this.saveAnyToLocalStorage(localStorageLightsKey, this.lights);
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
    getGroups(refresh = false) {
        return __awaiter(this, void 0, void 0, function* () {
            if (!refresh && this.lights && !this.data.autoRefreshState)
                return this.groups;
            return fetch("/api/kronishue/groups", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(this.getApiInput())
            }).then((response) => {
                if (response.ok) {
                    return response.json().then((data) => {
                        this.groups = data;
                        for (let id in this.groups) {
                            this.groups[id].id = id;
                        }
                        this.saveAnyToLocalStorage(localStorageGroupsKey, this.groups);
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
    setLightState(id, state) {
        return __awaiter(this, void 0, void 0, function* () {
            let data = this.getApiInput();
            data.state = state;
            return fetch(`/api/kronishue/lights/${id}/state`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data)
            }).then((response) => {
                if (response.ok) {
                    return response.json().then(() => {
                        return data;
                    });
                }
                else
                    throw "Failed";
            });
        });
    }
    setGroupAction(id, action) {
        return __awaiter(this, void 0, void 0, function* () {
            let data = this.getApiInput();
            data.action = action;
            return fetch(`/api/kronishue/groups/${id}/action`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data)
            }).then((response) => {
                if (response.ok) {
                    return response.json().then(() => {
                        return data;
                    });
                }
                else
                    throw "Failed";
            });
        });
    }
}
//# sourceMappingURL=kronishue.js.map