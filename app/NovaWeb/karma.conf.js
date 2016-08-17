'use strict';

var webpackConfig = require('./webpack/webpack.test.js');
require('phantomjs-polyfill');
webpackConfig.entry = {};

//webpackConfig.module.loaders[1].loader = 'style!css';
//webpackConfig.module.loaders[2].loader = 'style!css!sass';

module.exports = function (config) {
    config.set({
        basePath: '',
        frameworks: ['jasmine'],
        port: 9876,
        colors: true,
        logLevel: config.LOG_INFO,
        autoWatch: false,
        browsers: ['PhantomJS'],
        singleRun: true,
        autoWatchBatchDelay: 300,
        browserNoActivityTimeout: 30000,
        files: [
            './node_modules/phantomjs-polyfill/bind-polyfill.js',
            './src/test.ts',
            // This is an edge case for javascript tests when we load script here.
            // An alternative is to use requireJS for loading in test files.
            // DO NOT DO IT ANYMORE!!!
            './node_modules/bowser/bowser.js',
            './src/unsupported-browser/unsupported-browser.js',
            './src/unsupported-browser/unsupported-browser.spec.js',
            {
                pattern: './src/unsupported-browser/unsupported-browser.html',
                watched: false,
                included: false,
                served: true,
                nocache: false
            },
            {
                pattern: './src/unsupported-browser/unsupported-nofont.html',
                watched: false,
                included: false,
                served: true,
                nocache: false
            }
        ],
        proxies: {
            '/novaweb/static/unsupported-browser.html': '/base/src/unsupported-browser/unsupported-browser.html',
            '/novaweb/static/unsupported-nofont.html': '/base/src/unsupported-browser/unsupported-nofont.html'
        },
        babelPreprocessor: {
            options: {
                presets: ['es2015']
            }
        },
        preprocessors: {
            'src/test.ts': ['webpack'],
            'src/**/!(*.spec)+(.js)': ['coverage']
        },
        webpackMiddleware: {
            stats: {
                chunkModules: false,
                colors: true
            }
        },
        webpack: webpackConfig,
        reporters: [
            'dots',
            //'spec',
            'coverage'
        ],
        coverageReporter: {
            reporters: [
                {
                    dir: 'reports/coverage/',
                    subdir: '.',
                    type: 'html'
                },{
                    dir: 'reports/coverage/',
                    subdir: '.',
                    type: 'cobertura'
                }, {
                    dir: 'reports/coverage/',
                    subdir: '.',
                    type: 'json'
                }
            ]
        }
    });
};
