'use strict';

var webpackConfig = require('./webpack/webpack.test.js');
require('phantomjs-polyfill')
webpackConfig.entry = {};
webpackConfig.devtool = 'eval';

var spec = process.env.npm_config_spec;

if (!spec) {
    console.log('Please specify --spec=test-file-name-without-spec-and-ts');
    console.log('Usage: npm run test:spec --spec=auth.svc');
    process.exit(0);
}

var preprocessors = {};
preprocessors['src/**/' + spec + '.spec.ts'] = ['webpack', 'sourcemap'];

module.exports = function (config) {
    config.set({
        basePath: '',
        frameworks: ['jasmine'],
        port: 9876,
        colors: true,
        logLevel: config.LOG_INFO,
        autoWatch: true,
        browsers: ['PhantomJS'],
        singleRun: true,
        autoWatchBatchDelay: 300,
        files: [
            './node_modules/phantomjs-polyfill/bind-polyfill.js',
            './src/**/'+spec+'.spec.ts'
        ],
        mime: {
            'text/x-typescript': ['ts','tsx']
        },
        preprocessors: preprocessors,
        webpackMiddleware: {
            stats: {
                chunkModules: false,
                colors: true
            }
        },
        webpack: webpackConfig,
        reporters: [
            'dots',
            'spec',
            'coverage'
        ],
        coverageReporter: {
            reporters: [
                {
                    dir: 'reports/coverage/',
                    subdir: '.',
                    type: 'html'
                }, {
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
