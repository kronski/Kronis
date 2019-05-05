"use strict";
var SameWidth;
(function (SameWidth) {
    jQuery.fn.extend({
        sameWidth: function () {
            let widths = this.map(function () {
                let width = $(this).width();
                return width || 0;
            }).toArray();
            let maxwidth = Math.max(...widths);
            return this.width(maxwidth);
        }
    });
    SameWidth.sameWidthFunc = function () {
    };
})(SameWidth || (SameWidth = {}));
//# sourceMappingURL=samewidth.js.map