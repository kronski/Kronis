import { KronisHue } from "./kronishue.js";
 
$(function () {
    function refresh(refresh: boolean = false) {
        if (!document)
            return;
        let templateelem = document.getElementById("cardTemplate");
        if (!templateelem)
            return;

        let source = templateelem.innerHTML;
        var template = Handlebars.compile(source);

        let rows = document.getElementById("cardRows");
        if (template && rows) {
            while (rows.firstChild) {
                rows.removeChild(rows.firstChild);
            }

            let hue = new KronisHue();
            if (hue.data.username && hue.data.access_token) {
                hue.getLights(refresh).then((lights) => {
                    if (!template || !rows)
                        return;
                    for (let id in lights) {
                        let node = document.createElement("div");
                        rows.appendChild(node);
                        let light = lights[id];
                        node.outerHTML = template(light);
                    }
                });
            }
        }
    }

    $("#refreshButton").click(() => {
        refresh(true);
    });

    refresh();

    
});