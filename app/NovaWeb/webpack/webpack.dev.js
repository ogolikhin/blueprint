var loaders = require("./loaders");
var BrowserSyncPlugin = require('browser-sync-webpack-plugin');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var webpack = require('webpack');
var path = require('path');

var url = require('url'),
   proxy = require('proxy-middleware');
var proxyOptions = url.parse('http://localhost:9801/svc');
proxyOptions.route = '/svc';
var loginProxyOptions = url.parse('http://localhost:9801/Login/WinLogin.aspx');
loginProxyOptions.route = '/Login/WinLogin.aspx';

var del = require('del');
del(['dist/*']);

module.exports = {
    entry: {
        app: './index.ts',
        vendor: ['angular', 'angular-ui-router', 'angular-ui-bootstrap', 'angular-sanitize', 'bootstrap/dist/css/bootstrap.css', 'bowser']
    },
    output: {
        filename: 'app.js',
        path: 'dist/novaweb'
    },
    resolve: {
        root: __dirname,
        extensions: ['', '.ts', '.js', '.json']
    },
    resolveLoader: {
        modulesDirectories: ["node_modules"]
    },
    devtool: 'source-map',
    context: path.join(__dirname, '../src'),
    plugins: [
        new ExtractTextPlugin("[name].css"),
        new HtmlWebpackPlugin({
            template: './index.html',
            filename: '../index.html',
            inject: 'body',
            hash: true
        }),
        new BrowserSyncPlugin({
            host: 'localhost',
            port: 8000,
            server: {
                baseDir: 'dist',
                middleware: [proxy(proxyOptions), proxy(loginProxyOptions)]
            },
            ui: false,
            online: false,
            notify: false
        }),
        new webpack.optimize.CommonsChunkPlugin("vendor", "vendor.js"),
        // Uncomment next lines if jQuery is required for the app
        //new webpack.ProvidePlugin({
        //    $: 'jquery',
        //    jQuery: 'jquery',
        //    'window.jQuery': 'jquery',
        //    'window.jquery': 'jquery'
        //}),
         new CopyWebpackPlugin([
             // {output}/file.txt
             { from: '**/*.view.html' },
             { from: '../node_modules/bowser/bowser.js', to: './static/bowser.js' },
             { from: './unsupported-browser', to: './static' }
         ])
    ],
    module:{
        loaders: loaders
    }
};