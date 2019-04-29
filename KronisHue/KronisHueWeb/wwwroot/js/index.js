import { KronisHue } from "./kronishue.js";
$(function () {
    let hue = new KronisHue();
    let template = this.querySelector("#cardTemplate");
    let rows = this.getElementById("cardRows");
    if (template && rows) {
        for (let i = 0; i < 20; i++) {
            let node = document.createElement("div");
            rows.appendChild(node);
            node.outerHTML = template.innerHTML;
        }
    }
});
//# sourceMappingURL=index.js.map