'use srtrict';

var gulp = require('gulp'),
	bower = require('gulp-main-bower-files'),
	addsrc = require('gulp-add-src'),
	filter = require('gulp-filter'),
	concat = require('gulp-concat'),
	rename = require('gulp-rename'),
	srcmaps = require('gulp-sourcemaps'),
	tscompile = require('gulp-typescript'),
	jsminify = require('gulp-uglify')
	cssminify = require('gulp-csso'),
	del = require('del');

var src = {
	ts: 'app/**/*.ts',
	js: 'app/**/*.js',
	css: 'app/**/*.css',
	content: ['app/**/*.jpg', 'app/**/*.svg', 'app/**/*.png', 'app/**/*.ico', 'app/**/*.html']
}

var dst = {
	pub: 'pub/',
	lib: 'pub/lib/'
}

// pre-build

gulp.task('clean', function (cb) {
	del([dst.pub], cb);
})

// build

gulp.task('start-build', ['clean'])

gulp.task('bower', ['start-build'], function () {
	var jsfilter = filter('**/*.js')
	var cssfilter = filter('**/*.css')
	return gulp.src('bower.json')
		.pipe(bower())
		.pipe(jsfilter)
		.pipe(concat('lib.min.js'))
		.pipe(jsminify())
		.pipe(gulp.dest(dst.lib))
		.pipe(jsfilter.restore())
		.pipe(cssfilter)
		.pipe(concat('lib.min.css'))
		.pipe(cssminify())
		.pipe(gulp.dest(dst.lib))
		.pipe(cssfilter.restore())
		.pipe(rename(function (path) {
    	if (~path.dirname.indexOf('fonts')) {
    		path.dirname = '/fonts'
    	}
    }))
    .pipe(gulp.dest(dst.lib));
})

gulp.task('js', ['start-build'], function () {
	return gulp.src([src.ts])
		.pipe(srcmaps.init())
		.pipe(concat('app.min.js'))
		.pipe(jsminify())
		.pipe(srcmaps.write())
		.pipe(gulp.dest(dst.pub));
})

gulp.task('css', ['start-build'], function () {
	return gulp.src([src.css])
		.pipe(srcmaps.init())
		.pipe(concat('app.min.css'))
		.pipe(cssminify())
		.pipe(srcmaps.write())
		.pipe(gulp.dest(dst.pub));
})

gulp.task('content', ['start-build'], function () {
	return gulp.src(src.content)
		.pipe(gulp.dest(dst.pub));
})

gulp.task('end-build', ['bower', 'js', 'css', 'content'])

// post-build

gulp.task('post-build', ['end-build'])

gulp.task('build', ['post-build'])

gulp.task('run', ['build'])
