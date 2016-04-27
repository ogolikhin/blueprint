var loaders = require("./loaders");
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var webpack = require('webpack');
var path = require('path');

var del = require('del');
del(['dist/*']);

module.exports = {
    entry: {
        app: './index.ts',
        vendor: ['angular', 'angular-ui-router', 'angular-ui-bootstrap', 'angular-sanitize', 'bootstrap/dist/css/bootstrap.css', 'bowser', 'ag-grid', 'ag-grid/dist/styles/ag-grid.css']
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
        new webpack.optimize.UglifyJsPlugin(
            {
                warning: false,
                mangle: true,
                comments: false
            }
        ),
        new HtmlWebpackPlugin({
            template: './index.html',
            filename: '../index.html',
            inject: 'body',
            hash: true
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
         ]),
         new webpack.DefinePlugin({
             VERSION: JSON.stringify(require('../package.json').version),
             BUILD_YEAR: new Date().getFullYear().toString()
         })
    ],
    module:{
        loaders: loaders
    }
};