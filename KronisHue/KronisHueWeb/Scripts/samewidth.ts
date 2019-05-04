interface JQuery {
    sameWidth(): JQuery;
}

namespace SameWidth {
    jQuery.fn.extend({
        sameWidth: function () {
            let widths = this.map(function () {
                let width = $(this).width();
                return width || 0;
            }).toArray();
            let maxwidth = Math.max(...widths)
            return this.width(maxwidth);
        }
    });


    export var sameWidthBody = function () {
        jQuery('body').sameWidth();
    }
}