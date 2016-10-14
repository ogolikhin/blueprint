var webpack = require('webpack');
var path = require('path');
var CopyWebpackPlugin = require('copy-webpack-plugin');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var HtmlWebpackPlugin = require('html-webpack-plugin');

var loaders = require("./loaders");
var vendor_libs = require('./vendors');
var proxy_config = require('./proxy.dev');

var _APP = path.join(__dirname, './../src');


var del = require('del');
del(['dist/*']);


function isDebug(argument) {
    return argument === '--debug';
}

if (process.argv.some(isDebug)) {
    console.log("Is Debug");
}

module.exports = {
    context: _APP,
    entry: {
        app: ['webpack/hot/dev-server', './index.ts'],
        vendor: vendor_libs
    },
    output: {
        publicPath: "/novaweb/",
        path: path.resolve(_APP + '../dist/novaweb/'),
        filename: '[name].bundle.js'
    },
    devServer: {
        host: '0.0.0.0',
        port: 8000,
        proxy: proxy_config,
        watchOptions: {
            aggregateTimeout: 300,
            poll: 1000
        },
        historyApiFallback: true
    },
    plugins: [
        new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /en/),

        new HtmlWebpackPlugin({
            template: './index.html',
            filename: '../index.html',
            inject: false
        }),
        new ExtractTextPlugin("[name].css"),
        new webpack.HotModuleReplacementPlugin(),
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
            {from: '../libs/mxClient/resources', to: './libs/mxClient/resources'},
            {from: '../libs/mxClient/css', to: './libs/mxClient/css'},
            {from: '../libs/mxClient/js', to: './libs/mxClient/js'},

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
        extensions: ['', '.webpack.js', '.ts', '.js', '.json'],
        alias: {
            tinymce: 'tinymce/tinymce',
            mxClient: path.resolve(__dirname, '../libs/mxClient/js/mxClient.js'),
            mxClientCss: path.resolve(__dirname, '../libs/mxClient/css')
        }
    },
    tslint: {
        emitErrors: true,
        failOnHint: true
    },
    module: {
        loaders: loaders,
        preLoaders: [
            {
                test: /\.ts$/,
                loader: 'tslint-loader',
                exclude: ['../node_modules']
            }
        ],
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
