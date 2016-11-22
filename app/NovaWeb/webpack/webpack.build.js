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
        publicPath: "/novaweb/",
        path: path.resolve(_DIST + '/novaweb/'),
        filename: '[name].bundle.js'
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
        new ExtractTextPlugin('[name].css', {allChunks: true}),

        new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false,
                drop_console: true
            },
            //mangle: true,
            //beautify: false,
            sourceMap: isDebug
        }),
        new webpack.optimize.CommonsChunkPlugin("vendor", "vendor.bundle.js"),
        new CopyWebpackPlugin([
            // {output}/file.txt
            {from: './web.config'},
            {from: './favicon**', to: '../'},
            {from: '../node_modules/tinymce/plugins', to: './libs/tinymce/plugins'},
            {from: '../node_modules/tinymce/themes', to: './libs/tinymce/themes'},
            {from: '../node_modules/tinymce/skins', to: './libs/tinymce/skins'},
            {from: '../node_modules/bowser/bowser.js', to: './static/bowser.js'},

            {from: '../libs/tinymce/plugins/tinymce-mention', to: './libs/tinymce/plugins/mention'},

            {from: '../libs/mxClient/icons', to: './libs/mxClient/icons'},
            {from: '../libs/mxClient/images', to: './libs/mxClient/images'},
            {from: '../libs/mxClient/stencils', to: './libs/mxClient/stencils'},
            {from: '../libs/mxClient/resources', to: './libs/mxClient/resources'},
            {from: '../libs/mxClient/css', to: './libs/mxClient/css'},
            {from: '../libs/mxClient/js', to: './libs/mxClient/js'},

            {from: '../assets', to: './static'},
            {from: './unsupported-browser', to: './static'},

            {from: '../node_modules/bootstrap-sass/assets/fonts', to: './fonts'},

            {from: '../src/fonts', to: './fonts'},
            {from: '../src/images', to: './static/images'},

            {from: '../src/modules/editors/bp-process/styles/images', to: './static/bp-process/images'},
            {from: '../src/images/icons', to: './static/images/icons'}
        ]),
        new webpack.DefinePlugin({
            ENABLE_LOCAL_HOST_TRACKING:false,
            ENABLE_LOG:false,
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
