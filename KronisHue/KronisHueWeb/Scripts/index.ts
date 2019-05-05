import { KronisHue, LightState, GroupAction } from "./kronishue.js";
import "./samewidth.js";

interface JQuery {
    test(options: Object): JQuery;
}

$(function () {
    let hue = new KronisHue();

    function genTemplate(name:string) {
        let templateelem = document.getElementById(name);
        if (!templateelem)
            return null;

        let source = templateelem.innerHTML;
        var template = Handlebars.compile(source);
        return template;
    }


    function refresh(refresh: boolean = false) {
        if (!document)
            return;

        let groupstemplate = genTemplate("groupTemplate");
        let lightgroupselem = document.getElementById("lightgroups");
        if (lightgroupselem) {
            while (lightgroupselem.firstChild) {
                lightgroupselem.removeChild(lightgroupselem.firstChild);
            }

            if (hue.canRefresh()) {
                Promise.all([
                    hue.getLights(refresh),
                    hue.getGroups(refresh)
                ]).then(([lights,groups]) => {
                    if (!groupstemplate || !lightgroupselem || !lights || !groups)
                        return;

                    let g = Object.keys(groups).map((key) => {
                        return {
                            group: groups[key],
                            lights: groups[key].lights.map((id) => {
                                return lights[id];
                            })
                        }
                    });

                    let node = document.createElement("div");
                    lightgroupselem.appendChild(node);
                    node.outerHTML = groupstemplate(g);

                    $(".same-width-1").sameWidth();
                });
            }
        }
    }

    $("#refreshButton").click(() => {
        refresh(true);
    });

    $(document).on("change", function (event) {
        if (!event) return;

        let source = $(event.target);
        if (source.hasClass("lightswitch")) {
            let id = source.attr("data-id");
            if (!id) return;
            let checked = source.is(':checked')

            let state = new LightState();
            state.on = checked;

            hue.setLightState(parseInt(id), state)
        }
        else if (source.hasClass("lighthue")) {
            let id = source.attr("data-id");
            if (!id) return;

            let state = new LightState();
            let val = source.val();
            if (!val) return;
            state.hue = parseInt(<string>val);

            hue.setLightState(parseInt(id), state)
        }
        else if (source.hasClass("lightsat")) {
            let id = source.attr("data-id");
            if (!id) return;

            let state = new LightState();
            let val = source.val();
            if (!val) return;
            state.sat = parseInt(<string>val);

            hue.setLightState(parseInt(id), state)
        }
        else if (source.hasClass("groupswitch")) {
            let id = source.attr("data-id");
            if (!id) return;
            let checked = source.is(':checked')

            let action = new GroupAction();
            action.on = checked;

            hue.setGroupAction(parseInt(id), action)
        }

        
    });

    refresh();
    
});