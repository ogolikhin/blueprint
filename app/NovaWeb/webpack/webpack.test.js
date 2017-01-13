var loaders = require("./loaders");
var webpack = require('webpack');
var path = require('path');
var FailPlugin = require('webpack-fail-plugin');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var ProgressBarPlugin = require('progress-bar-webpack-plugin');

var autoprefixer = require('autoprefixer');

// Do not use code coverage when started with --debug parameter
var postLoaders = [
    {
        test: /^((?!\.(spec)|(mock)\.ts).)*.ts$/,
        exclude: [/node_modules/, /bower_components/, /storyteller/],
        loader: 'istanbul-instrumenter',
        include: [
            path.join(__dirname, "../src")
        ]
    }
];
var preLoaders = [
    {
        test: /\.spec.ts$/,
        loader: 'tslint-loader',
        exclude: [/node_modules/],
        include: [
            path.join(__dirname, "../src")
        ]
    }
];
function isDebug(argument) {
    return argument === '--debug';
}
var sourceMap = "eval";
if (process.argv.some(isDebug)) {
    postLoaders = [];
    preLoaders = [];
    sourceMap = "source-map-inline";
    console.log("Is Debug");
}

module.exports = {
    entry: ['./src/index.ts'],
    output: {
        filename: 'build.js',
        path: 'tmp'
    },
    resolve: {
        root: __dirname,
        extensions: ['', '.ts', '.js', '.json'],
        alias: {
            tinymce: 'tinymce/tinymce',
            mxClient: path.resolve(__dirname, '../libs/mxClient/js/mxClient.js')
        }
    },
    resolveLoader: {
        modulesDirectories: ["node_modules"]
    },
    tslint: {
        emitErrors: true,
        failOnHint: true,
        configuration: {
            rules: {
                "only-arrow-functions": false
            }
        }
    },
    devtool: sourceMap,
    bail: true,
    plugins: [
        FailPlugin,
        new ProgressBarPlugin(),
        new ExtractTextPlugin("[name].css"),
        new webpack.ProvidePlugin({
            $: 'jquery',
            jQuery: 'jquery',
            'window.jQuery': 'jquery',
            'window.jquery': 'jquery'
        }),
        new webpack.DefinePlugin({
            KEEN_PROJECT_ID: undefined,
            KEEN_WRITE_KEY: undefined,
            ENABLE_LOCAL_HOST_TRACKING: false,
            ENABLE_LOG: true,
            VERSION: JSON.stringify(require('../package.json').version),
            BUILD_YEAR: new Date().getFullYear().toString()
        })
    ],
    postcss: [
        autoprefixer({browsers: ['last 2 versions']})
    ],
    module: {
        loaders: loaders,
        postLoaders: postLoaders,
        preLoaders: preLoaders
    }
};
