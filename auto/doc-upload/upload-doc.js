'use strict';

var gulp = require('gulp');
	
//var $ = require('gulp-load-plugins')({
//    pattern: ['gulp-*', 'main-bower-files', 'uglify-save-license', 'del']
//});

var spsave = require('./gulp-spsave/index');


gulp.task('upload-doc', function () {
    console.log('upload-doc is done');
    gulp.src(['../doc/**/*','../**/doc/*', '!../node_modules/**/doc/*'], {base: './'} )
        .pipe(spsave( {
				username: "build@blueprintsys.com",
				password: "Bu1ld100",
				siteUrl: "https://blueprintsys.sharepoint.com/rnd/",
				siteFolder: "Shared Documents/Architecture/Document Repository"
        }));
});

gulp.task('upload-doc-master', function () {
    console.log('upload-doc is done');
    gulp.src(['../doc/**/*','../**/doc/*', '!../node_modules/**/doc/*'], {base: './'} )
        .pipe(spsave( {
				username: "build@blueprintsys.com",
				password: "Bu1ld100",
				siteUrl: "https://blueprintsys.sharepoint.com/rnd/",
				siteFolder: "Shared Documents/Architecture/Document Repository (master)"
        }));
});