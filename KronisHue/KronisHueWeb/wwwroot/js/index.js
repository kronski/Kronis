import { KronisHue } from "./kronishue.js";
$(function () {
    function genTemplate(name) {
        let templateelem = document.getElementById(name);
        if (!templateelem)
            return null;
        let source = templateelem.innerHTML;
        var template = Handlebars.compile(source);
        return template;
    }
    function refresh(refresh = false) {
        if (!document)
            return;
        let groupstemplate = genTemplate("groupTemplate");
        let rows = document.getElementById("cardRows");
        if (rows) {
            while (rows.firstChild) {
                rows.removeChild(rows.firstChild);
            }
            let hue = new KronisHue();
            if (hue.canRefresh()) {
                Promise.all([
                    hue.getLights(refresh),
                    hue.getGroups(refresh)
                ]).then(([lights, groups]) => {
                    if (!groupstemplate || !rows || !lights || !groups)
                        return;
                    let g = Object.keys(groups).map((key) => {
                        return {
                            group: groups[key],
                            lights: groups[key].lights.map((id) => {
                                return lights[id];
                            })
                        };
                    });
                    let node = document.createElement("div");
                    rows.appendChild(node);
                    node.outerHTML = groupstemplate(g);
                });
            }
        }
    }
    $("#refreshButton").click(() => {
        refresh(true);
    });
    refresh();
});
//# sourceMappingURL=index.js.map