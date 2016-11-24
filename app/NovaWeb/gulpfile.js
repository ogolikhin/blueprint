// Imports

var gulp = require('gulp');
var sass = require('gulp-sass');
var styleguide = require('sc5-styleguide');

// Path definitions
var styleguidePort = 4000;
var sourcePath = 'src';
var htmlWild = sourcePath + '/**/*.html';
var styleSourcePath = sourcePath + '/styles';
var scssWild = styleSourcePath + '/**/*.scss';
var scssRoot = styleSourcePath + '/main.scss';
var overviewPath = styleSourcePath + '/styleguide-overview.md';

var buildPath = '.build';
var styleBuildPath = buildPath + '/styles';

var tmpPath = '.tmp';
var styleguideTmpPath = tmpPath + '/styleguide';

gulp.task('scss', function () {
    return gulp.src([scssRoot])
        .pipe(sass())
        .pipe(gulp.dest(styleBuildPath));
});

// Running styleguide development server to view the styles while you are working on them
//
// This task runs the interactive style guide for use by developers. It runs a built-in server
// and contains all the interactive features and should be updated automatically whenever the
// styles are modified.

gulp.task('styleguide:generate', function () {
    return gulp.src(scssWild)
        .pipe(styleguide.generate({
            title: 'Blueprint Styleguide',
            server: true,
            port: styleguidePort,
            rootPath: styleguideTmpPath,
            overviewPath: overviewPath
        }))
        .pipe(gulp.dest(styleguideTmpPath));
});

gulp.task('styleguide:applystyles', function () {
    return gulp.src([scssRoot])
        .pipe(sass({
            errLogToConsole: true
        }))
        .pipe(styleguide.applyStyles())
        .pipe(gulp.dest(styleguideTmpPath));
});

gulp.task('styleguide', ['styleguide:generate', 'styleguide:applystyles']);

gulp.task('html', function () {
    return gulp.src([htmlWild])
        .pipe(gulp.dest(buildPath));
});

// Developer mode

gulp.task('styleguide:dev', ['html', 'scss', 'styleguide'], function () {
    gulp.watch(htmlWild, ['html']);
    gulp.watch(scssWild, ['scss', 'styleguide']);
    console.log(
        '\nDeveloper mode!\n\nSC5 Styleguide available at http://localhost:' + styleguidePort + '/\n'
    );
});


