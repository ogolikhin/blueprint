var webpack = require('webpack');
var path = require('path');

var CopyWebpackPlugin = require('copy-webpack-plugin');
var ExtractTextPlugin = require("extract-text-webpack-plugin");

var loaders = require("./loaders");
var vendor_libs = require('./vendors');
var _DIST = path.resolve('./dist');
var APP = path.join(__dirname, '../src');

var del = require('del');
del(['dist/*']);

module.exports = {
    context: APP,
    entry: {
        app: ['webpack/hot/dev-server', './index.ts'],
        vendor: vendor_libs
    },
    output: {
        publicPath: "/novaweb/",
        path: path.resolve(_DIST + '../dist/novaweb/'),
        filename: '[name].bundle.js'
    },
    plugins: [
        new ExtractTextPlugin("[name].css"),
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false,
                drop_console: true
            },
            //mangle: true,
            //beautify: false,
            sourceMap: false
        }),
        new webpack.optimize.CommonsChunkPlugin("vendor", "vendor.bundle.js"),
        new CopyWebpackPlugin([
            // {output}/file.txt
            {from: '**/*.view.html'},
            {from: './web.config'},
            {from: '../node_modules/tinymce/plugins', to: './libs/tinymce/plugins'},
            {from: '../node_modules/tinymce/themes', to: './libs/tinymce/themes'},
            {from: '../node_modules/tinymce/skins', to: './libs/tinymce/skins'},
            {from: '../node_modules/bowser/bowser.js', to: './static/bowser.js'},
            {from: './unsupported-browser', to: './static'},
            {from: '../libs/tinymce/plugins/tinymce-mention', to: './libs/tinymce/plugins/mention'},
            {from: '../libs/mxClient/icons', to: './libs/mxClient/icons'},
            {from: '../libs/mxClient/images', to: './libs/mxClient/images'},
            {from: '../libs/mxClient/stencils', to: './libs/mxClient/stencils'},
            {from: '../assets', to: './static'},
            {from: '../src/modules/editors/bp-process/styles/images', to: './static/bp-process/images'},
            {from: '../src/styles/images/icons', to: './static/images/icons'}
        ]),
        new webpack.DefinePlugin({
            VERSION: JSON.stringify(require('../package.json').version),
            BUILD_YEAR: new Date().getFullYear().toString()
        })
    ],
    resolve: {
        root: __dirname,
        extensions: ['', '.ts', '.js', '.json'],
        alias: {
            tinymce: 'tinymce/tinymce',
            mxClient: path.resolve(__dirname, '../libs/mxClient/js/mxClient.min.js'),
            mxClientCss: path.resolve(__dirname, '../libs/mxClient/css')
        }
    },
    module:{
        loaders: loaders,
        noParse: [/angular-perfect-scrollbar-2/]
    },
    resolveLoader: {
        modulesDirectories: ["node_modules"]
    },
    'html-minify-loader': {
        empty: true,
        dom: {
            lowerCaseAttributeNames: false
        }
    },
    devtool: 'source-map'

};
