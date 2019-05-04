export function samewidth() {
    for (let grpindex = 1; grpindex < 10; grpindex++) {
        let widths = $.makeArray($("same-size+" + grpindex).map(() => { return this.clientWidth; }));
        let maxwidth = Math.max(...widths);
        $("same-size+" + grpindex).css("min-width", maxwidth);
    }
}
//# sourceMappingURL=file.js.map