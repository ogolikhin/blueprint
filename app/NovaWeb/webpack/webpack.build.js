function isDebug(argument) {
    return argument === '--debug';
}

var webpack = require('webpack');
var failPlugin = require('webpack-fail-plugin');
var path = require('path');
var isDebug = process.argv.some(isDebug);
var CopyWebpackPlugin = require('copy-webpack-plugin');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var HtmlWebpackPlugin = require('html-webpack-plugin');
var autoprefixer = require('autoprefixer');
var ProgressBarPlugin = require('progress-bar-webpack-plugin');

var loaders = require("./loaders");
var _DIST = path.resolve('./dist');
var _APP = path.join(__dirname, './../src');

var del = require('del');
del(['dist/*']);

module.exports = {
    context: _APP,
    entry: {
        app: './index.ts',
        vendor: ['./../src/vendor.ts']
    },
    output: {
        publicPath: "/",
        path: path.resolve(_DIST + '/novaweb/'),
        filename: '[name].bundle.[chunkhash].js'
    },
    plugins: [
        failPlugin,
        new ProgressBarPlugin(),
        new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /en/),
        new HtmlWebpackPlugin({
            template: './index.html',
            filename: '../index.html',
            inject: false,
            minify: {
                collapseWhitespace: true,
                removeComments: true,
                caseSensitive: true
            }
        }),
        new ExtractTextPlugin('[name].bundle.[chunkhash]css', {allChunks: true}),
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false,
                drop_console: true
            },
            sourceMap: isDebug
        }),
        new webpack.optimize.CommonsChunkPlugin("vendor", "vendor.bundle.[chunkhash].js"),
        new CopyWebpackPlugin([
            // {output}/file.txt
            {from: './web.config'},
            {from: './favicon**', to:'../'},
            {from: '../node_modules/tinymce/plugins', to: './novaweb/libs/tinymce/plugins'},
            {from: '../node_modules/tinymce/themes', to: './novaweb/libs/tinymce/themes'},
            {from: '../node_modules/tinymce/skins', to: './novaweb/libs/tinymce/skins'},
            {from: '../node_modules/bowser/bowser.js', to: './novaweb/static/bowser.js'},
            {from: '../src/redirect/silverlight-links.js', to: './novaweb/static/redirect-silverlight-links.js'},

            {from: '../libs/tinymce/plugins/tinymce-mention', to: './novaweb/libs/tinymce/plugins/mention'},

            {from: '../libs/mxClient/icons', to: './novaweb/libs/mxClient/icons'},
            {from: '../libs/mxClient/images', to: './novaweb/libs/mxClient/images'},
            {from: '../libs/mxClient/stencils', to: './novaweb/libs/mxClient/stencils'},
            {from: '../libs/mxClient/resources', to: './novaweb/libs/mxClient/resources'},
            {from: '../libs/mxClient/css', to: './novaweb/libs/mxClient/css'},
            {from: '../libs/mxClient/js', to: './novaweb/libs/mxClient/js'},

            {from: '../assets', to: './novaweb/static'},
            {from: './unsupported-browser', to: './novaweb/static'},

            {from: '../node_modules/bootstrap-sass/assets/fonts', to: './novaweb/fonts'},

            {from: '../src/fonts', to: './novaweb/fonts'},
            {from: '../src/images', to: './novaweb/static/images'},

            {from: '../src/modules/editors/bp-process/styles/images', to: './novaweb/static/bp-process/images'},
            {from: '../src/images/icons', to: './novaweb/static/images/icons'}
        ]),
        new webpack.DefinePlugin({
            KEEN_PROJECT_ID: undefined,
            KEEN_WRITE_KEY: undefined,
            //KEEN_PROJECT_ID: JSON.stringify('582cb85c8db53dfda8a78767'),
            //KEEN_WRITE_KEY: JSON.stringify('E011AFC42952D3500532FA364DA5DC06BB962F988B2F171CB252201B357F48BCBA671F8A8E62060148129B391FE2D1B3A4E8D9BD6F0629DFF66C9C7C2C1F8F612A80E44ACDEA4F6B1408AAF403649EFF9394A399844C744E0E4F72CA204A0E13'),
            ENABLE_LOCAL_HOST_TRACKING: false,
            ENABLE_LOG: false,
            VERSION: JSON.stringify(require('../package.json').version),
            BUILD_YEAR: new Date().getFullYear().toString()
        })
    ],
    postcss: [
        autoprefixer({browsers: ['last 2 versions']})
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
        ]
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
    devtool: 'cheap-module-source-map'
};
