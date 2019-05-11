"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const fs = require("fs");
const http = require("http");
const fetch = require("node-fetch");
var settings = {
    username: undefined,
    huebridgeip: "philips-hue",
    serverinterface: "127.0.0.1",
    serverport: 3000
};
const readFile = (path, opts = 'utf8') => new Promise((resolve, reject) => {
    fs.readFile(path, opts, (err, data) => {
        if (err)
            reject(err);
        else
            resolve(data);
    });
});
const server = http.createServer((req, res) => {
    res.statusCode = 200;
    res.setHeader('Content-Type', 'text/plain');
    res.end('Hello World\n');
}).on('listening', () => {
    console.log(`Server running at http://${settings.serverinterface}:${settings.serverport}/`);
}).on('close', () => {
    console.info('server closed');
}).on('error', (err) => {
    console.error(err);
}).listen(settings.serverport, settings.serverinterface);
function registerOnHue() {
    fetch(`http://${settings.huebridgeip}/api`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ "devicetype": "kronishuelogger" })
    })
        .then(res => res.json())
        .then(response => {
        if (Array.isArray(response) && response[0] && response[0].success && response[0].success.username) {
            settings.username = response[0].success.username;
            saveSettings();
        }
        else if (Array.isArray(response) && response[0] && response[0].error && response[0].error.description) {
            console.log(response[0].error.description);
        }
        else {
            console.log("Unknown response");
        }
    });
}
function saveSettings() {
    let data = JSON.stringify(settings, null, 2);
    fs.writeFile("settings.json", data, 'utf8', function (err) {
        if (err) {
            console.log("An error occured while writing JSON Object to File.");
            return console.log(err);
        }
        console.log("JSON file has been saved.");
    });
}
function loadSettings() {
    return readFile("settings.json", 'utf8').then((data) => {
        settings = JSON.parse(data);
    }).catch((err) => {
        console.log("An error occured while reading from File.");
        console.log(err);
    });
}
function checkHueForChange() {
}
var mainTimeout;
function mainfunc() {
    mainTimeout = undefined;
    if (settings.username === undefined) {
        console.log("Register app on hue bridge");
        registerOnHue();
        if (settings.username === undefined) {
            mainTimeout = setTimeout(mainfunc, 5000);
            return;
        }
    }
    if (settings.username !== undefined) {
        console.log("Fetching changes from hue bridge");
        checkHueForChange();
        mainTimeout = setTimeout(mainfunc, 60000);
        return;
    }
}
function initapp() {
    return __awaiter(this, void 0, void 0, function* () {
        yield loadSettings();
        mainfunc();
    });
}
process.stdin.resume();
process.on('SIGINT', function () {
    if (mainTimeout !== undefined) {
        console.log("Stopping main");
        clearTimeout(mainTimeout);
    }
    server.close();
    server.close(function () {
        console.log("Finished all requests");
        process.exit(0);
    });
});
initapp();
//# sourceMappingURL=huelogger.js.map