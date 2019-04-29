class KronisHueData {
    code: string | null;
    state: string | null;
    deviceid: string | null;
    devicename: string | null;
    nonce: string | null;

    access_token: string | null;
    access_token_expires_in: string | null;
    refresh_token: string | null;
    refresh_token_expires_in: string | null;
    token_type: string | null;

    username: string|null;
};

export class KronisHue {

    data: KronisHueData;
    useLocalstorage: boolean;

    constructor(useLocalstorage: boolean = true) {
        this.useLocalstorage = useLocalstorage;
        
        this.loadFromLocalStorage();
    }

    loadFromLocalStorage() {
        if (!this.useLocalstorage)
            return;

        let jsonstr = window.localStorage.getItem("kronisHueData");
        if (jsonstr) {
            this.data = JSON.parse(jsonstr);
        }
        else
            this.data = new KronisHueData();

    }

    saveToLocalStorage() {
        if (!this.useLocalstorage)
            return;

        var jsonstr = JSON.stringify(this.data);
        let str = JSON.parse(jsonstr);
        window.localStorage.setItem("kronisHueData", jsonstr);
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
        this.data.state = this.getRandomStr(64);
        this.saveToLocalStorage();

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


    storeCode(): boolean {
        let state = this.data.state;
        let url_string = window.location.href
        let url = new URL(url_string);
        let requeststate = url.searchParams.get("state");
        let requestcode = url.searchParams.get("code");

        if (state == requeststate) {
            if (requestcode) {
                this.data.code = requestcode;
                this.saveToLocalStorage();
                return true;
            }
            return false;
        }
        else {
            return false;
        }
    }

    async refreshNonce() {
        return fetch('/api/kronishue/nonce', {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ code: this.data.code })
        }).then((response) => {
            return response.text().then((nonce) => {
                this.data.nonce = nonce;
                this.saveToLocalStorage();
                return this.data.nonce;
            });
        }).catch(() => {
            this.data.nonce = null;
            this.saveToLocalStorage();
            return this.data.nonce;
        });
    }

    async getTokenWithDigest() {
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
                    this.saveToLocalStorage();

                    return data;
                });
            }

        }).catch(() => {
            return null;
        });

    }
    async refreshTokenWithDigest() {
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
                    this.saveToLocalStorage();

                    return data;
                });
            }

        }).catch(() => {
            return null;
        });

    }


    async setDeviceType() {
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
                    this.saveToLocalStorage();

                    return data;
                });
            }

        }).catch(() => {
            return null;
        });
    }

    async getLights() {
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
                    return data;
                });
            }
        }).catch(() => {
            return null;
        });
    }
}