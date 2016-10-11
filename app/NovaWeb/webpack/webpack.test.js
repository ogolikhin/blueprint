var loaders = require("./loaders");
var webpack = require('webpack');
var path = require('path');
var FailPlugin = require('webpack-fail-plugin');

// Do not use code coverage when started with --debug parameter
var postLoaders = [
      {
          test: /^((?!\.(spec)|(mock)\.ts).)*.ts$/,
          exclude: [/node_modules/, /bower_components/, /storyteller/],
          loader: 'istanbul-instrumenter'
      }
];
var preLoaders = [
      {
          test: /\.ts$/,
          loader: 'tslint-loader',
          exclude: [/node_modules/, /storyteller/]
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
  devtool: sourceMap,
  bail: true,
  plugins: [
    FailPlugin,
    new webpack.ProvidePlugin({
      $: 'jquery',
      jQuery: 'jquery',
      'window.jQuery': 'jquery',
      'window.jquery': 'jquery'
    }),
    new webpack.DefinePlugin({
        VERSION: JSON.stringify(require('../package.json').version),
        BUILD_YEAR: new Date().getFullYear().toString()
    })
  ],
  module: {
    loaders: loaders,
    postLoaders: postLoaders,
    preLoaders: preLoaders,
    noParse: [/angular-perfect-scrollbar-2/, /tinymce/]
  }
};

