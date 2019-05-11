import * as fs from 'fs';
import * as http from 'http';
import * as fetch from 'node-fetch';
import { isMainThread } from 'worker_threads';

var settings = {
    username:undefined,
    huebridgeip:"philips-hue",
    
    serverinterface:"127.0.0.1",
    serverport:3000
}

const readFile = (path:string, opts = 'utf8') =>
  new Promise<string>((resolve, reject) => {
    fs.readFile(path, opts, (err, data) => {
      if (err) 
        reject(err)
      else 
        resolve(data)
    })
  })

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
            // "Content-Type": "application/x-www-form-urlencoded",
        },
        body: JSON.stringify({"devicetype":"kronishuelogger"})
    })
    .then(res => res.json())
    .then(response => {
        if(Array.isArray(response) && response[0] && response[0].success && response[0].success.username ){
            settings.username = response[0].success.username;
            saveSettings();
        } 
        else if(Array.isArray(response) && response[0] && response[0].error && response[0].error.description ){
            console.log(response[0].error.description);
        }
        else
        {
            console.log("Unknown response");
        }

    });
}

function saveSettings(){
    let data = JSON.stringify(settings,null, 2)

    fs.writeFile("settings.json", data, 'utf8', function (err) {
        if (err) {
            console.log("An error occured while writing JSON Object to File.");
            return console.log(err);
        }
     
        console.log("JSON file has been saved.");
    });
}

function loadSettings() {
    return readFile("settings.json", 'utf8' ).then((data)=>{
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
    if(settings.username === undefined) {
        console.log("Register app on hue bridge");

        registerOnHue();

        if(settings.username === undefined) {
            mainTimeout = setTimeout(mainfunc,5000);
            return;
        }
    }
    if(settings.username !== undefined) {
        console.log("Fetching changes from hue bridge");
        
        checkHueForChange();

        mainTimeout = setTimeout(mainfunc,60000);
        return;
    }
}

async function initapp() {
    await loadSettings();
    mainfunc();
}

process.stdin.resume();

process.on( 'SIGINT', function () {

    if(mainTimeout!==undefined) {
        console.log("Stopping main");
        clearTimeout(mainTimeout);
    }

    server.close()
    server.close(function () {
      console.log("Finished all requests");

      process.exit(0);
    });
 });

initapp();

