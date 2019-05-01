/// <binding BeforeBuild='default' />
const { src, dest, parallel } = require('gulp');

function scripts_types() {
    return src('node_modules/handlebars/types/index.d.ts').pipe(dest('Scripts/Types/'));
}

function wwwroot_libs_js() {
    return src().pipe(dest('wwwroot/lib/js/'));
    //return src('node_modules/bootstrap4-toggle/js/*').pipe(dest('wwwroot/lib/js/'));
}

function wwwroot_lib_css() {
    return src().pipe(dest('wwwroot/lib/css/'));
    //return src('node_modules/bootstrap4-toggle/css/*').pipe(dest('wwwroot/lib/css/'));
}


exports.default = parallel(scripts_types, wwwroot_libs_js, wwwroot_lib_css);