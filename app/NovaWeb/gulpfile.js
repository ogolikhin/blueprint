'use srtrict';

var gulp = require('gulp'),
    srcmaps = require('gulp-sourcemaps'),
    tsc = require('gulp-typescript'),
    tslint = require('gulp-tslint'),
    sass = require('gulp-sass'),
    cssnano = require('gulp-cssnano'),
    del = require('del'),
    inject = require('gulp-inject'),
    es = require('event-stream'),
    bowerFiles = require('main-bower-files'),
    angularFilesort = require('gulp-angular-filesort'),
    webserver = require('gulp-webserver');

var src = {
    ts: 'app/**/*.ts',
    js: 'app/**/*.js',
    css: 'assets/styles/**/*.css',
    scss: 'assets/styles/**/*.scss',

    css_Bundle: [

    ],

    content: ['app/**/*.jpg', 'app/**/*.svg', 'app/**/*.png', 'app/**/*.ico', 'app/**/*.html']
}

var dst = {
    dist: 'dist/',
    libs: 'dist/assets/libs/'
}

// pre-build

gulp.task('clean', function (done) {
    return del([dst.dist], done);
});

gulp.task('bower-install', function () {

    var install = require("gulp-install");

    return gulp.src(['./bower.json'])
        .pipe(install());
});

// build

gulp.task('start-build', ['clean']);

gulp.task('end-build', ['start-build', 'inject']);

gulp.task('post-build', ['end-build']);

gulp.task('build', ['post-build']);

var tsProject = tsc.createProject('tsconfig.json');

/**
 * Remove all generated JavaScript files from TypeScript compilation.
 */
gulp.task('clean-ts', function (cb) {
    var typeScriptGenFiles = [
        src.js,    // path to all JS files auto gen'd by editor
        src.js + '.map' // path to all sourcemap files auto gen'd by editor
    ];

    // delete the files
    del(typeScriptGenFiles, cb);
});

/**
 * Lint all custom TypeScript files.
 */
gulp.task('ts-lint', function () {
    return gulp.src(src.ts).pipe(tslint()).pipe(tslint.report('prose'));
});

/**
 * Compile TypeScript and include references to library and app .d.ts files.
 */
gulp.task('compile-ts', ['start-build'], function () {
    var sourceTsFiles = [src.ts,                //path to typescript files
        "typings/**/*.d.ts"]; //reference to library .d.ts files

    var tsResult = gulp.src(sourceTsFiles)
        .pipe(srcmaps.init())
        .pipe(tsc(tsProject));

    // tsResult.dts.pipe(gulp.dest());
    return tsResult.js
        .pipe(srcmaps.write('.'))
        .pipe(gulp.dest(dst.dist + 'app/'));
});

gulp.task('sass', ['start-build'], function () {
    return gulp.src(src.scss)
        .pipe(sass())
        .pipe(cssnano())
        .pipe(gulp.dest(dst.dist + 'assets/styles/'))
});

gulp.task('content', ['start-build'], function () {
    return gulp.src(src.content)
        .pipe(gulp.dest(dst.dist + 'app/'));
});

gulp.task('libs', ['start-build'], function () {
    return gulp.src('assets/libs/**/*')
        .pipe(gulp.dest(dst.libs));
});

gulp.task('inject', ['libs', 'content', 'compile-ts', 'sass'],
    function () {

        return gulp.src('./index.html')
            .pipe(inject(gulp.src(bowerFiles(), { read: false }), { name: 'bower', relative: true }))
            .pipe(inject(es.merge(
                gulp.src(dst.dist + src.css, { read: false }),
                gulp.src(dst.dist + src.js)
                    .pipe(angularFilesort())
                ),
                { ignorePath: '/dist' }
                ))
            .pipe(gulp.dest(dst.dist));
    });

gulp.task('dev', ['watch', 'serve']);

gulp.task('watch', function () {
    gulp.watch([src.ts], ['ts-lint', 'compile-ts', 'inject']);
    gulp.watch([src.content], ['content', 'inject']);
    gulp.watch([src.scss], ['sass', 'inject']);
    gulp.watch('index.html', ['inject']);
});

gulp.task('serve', function () {
    var options = {
        livereload: true,
        open: true,
        proxies:
        {
            source: '/svc',
            target: 'http://localhost:9801/svc'
        }
    };

    if (require('fs').existsSync('.config/serve.config.json')) {
        console.log('use .config/serve.config.json')
        options = require('./.config/serve.config.json');
        gulp.src(dst.dist)
            .pipe(webserver(options));
    }
    else {
        console.log('use default config');
        gulp.src(dst.dist)
            .pipe(webserver(options));
    }
});
