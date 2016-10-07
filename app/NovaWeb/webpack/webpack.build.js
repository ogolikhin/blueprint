var loaders = require("./loaders");
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var webpack = require('webpack');
var path = require('path');
var vendor_libs = require('./vendors');
var FailPlugin = require('webpack-fail-plugin');

var del = require('del');
del(['dist/*']);

module.exports = {
    entry: {
        app: './index.ts',
        vendor: vendor_libs
    },
    output: {
        filename: 'app.js',
        path: 'dist/novaweb'
    },
    resolve: {
        root: __dirname,
        extensions: ['', '.ts', '.js', '.json'],
        alias: {
            tinymce: 'tinymce/tinymce',
            mxClient: path.resolve(__dirname, '../libs/mxClient/js/mxClient.min.js'),
            mxClientCss: path.resolve(__dirname, '../libs/mxClient/css')
        }
    },
    resolveLoader: {
        modulesDirectories: ["node_modules"]
    },
    devtool: 'source-map',
    context: path.join(__dirname, '../src'),
    //devServer: {
    //    // This is required for webpack-dev-server if using a version <3.0.0.
    //    // The path should be an absolute path to your build destination.
    //        outputPath: path.join(__dirname, 'build')
    //    },

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
             { from: './web.config' },
             { from: './unsupported-browser', to: './static' },

             { from: '../node_modules/bowser/bowser.js', to: './static/bowser.js' },
             { from: '../node_modules/tinymce/plugins', to: './libs/tinymce/plugins' },
             { from: '../node_modules/tinymce/themes', to: './libs/tinymce/themes' },
             { from: '../node_modules/tinymce/skins', to: './libs/tinymce/skins' },

             { from: '../libs/tinymce/plugins/tinymce-mention', to: './libs/tinymce/plugins/mention' },

             { from: '../libs/mxClient/icons', to: './libs/mxClient/icons' },
             { from: '../libs/mxClient/images', to: './libs/mxClient/images' },
             { from: '../libs/mxClient/stencils', to: './libs/mxClient/stencils' },

             { from: '../assets', to: './static'},

             { from: '../src/modules/editors/bp-process/styles/images', to: './static/bp-process/images' },
             { from: '../src/styles/images/icons', to: './static/images/icons' }

         ]),
         new webpack.DefinePlugin({
             VERSION: JSON.stringify(require('../package.json').version),
             BUILD_YEAR: new Date().getFullYear().toString()
         }),
         FailPlugin
    ],
    module:{
        loaders: loaders,
        noParse: [/angular-perfect-scrollbar-2/]
    }
};
