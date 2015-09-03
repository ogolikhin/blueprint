'use strict';

var gulp = require('gulp');

var $ = require('gulp-load-plugins')({
    pattern: ['gulp-*', 'main-bower-files', 'uglify-save-license', 'del']
});

gulp.task('upload-doc', function () {
    console.log('upload-doc is done');
    gulp.src('./doc/**/*')
        .pipe($.spsave({
            username: "",
            password: "",
            siteUrl: "https://blueprintsys.sharepoint.com/rnd/",
            folder: "Shared Documents/Architecture/GitHub"
        }));
});
