'use strict';

var gulp = require('gulp-npm-run')(require('gulp-help')(require('gulp')), {
    exclude: ['postinstall', 'test:spec'], // the test script is excluded 
    //include: { 'necessary': 'a must-have task, because...' }, // just a helpful description 
    require: ['build'], // maybe because other tasks depend it 
    requireStrict: false,
    npmRun: true // rather than `npm run script` gulp runs the script's value / command(s) 
});
var browserSync = require('browser-sync').create();

// Static server
gulp.task('serve', ['build'], function () {
    var options = (require('fs').existsSync('.config/serve.config.json'))
        ? require('./.config/serve.config.json')
        : {
            open: true,
            proxies:
            {
                source: '/svc',
                target: 'http://localhost:9801/svc'
            }
        };

    options.server = "./dist";

    browserSync.init(options);
});
