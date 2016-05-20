var loaders = require("./loaders");
var webpack = require('webpack');

// Do not use code coverage when started with --debug parameter
var postLoaders = [
      {
          test: /^((?!\.spec\.ts).)*.ts$/,
          exclude: /(node_modules|bower_components)/,
          loader: 'istanbul-instrumenter'
      }
];
function isDebug(argument) {
    return argument === '--debug';
}
if (process.argv.some(isDebug)) {   
    postLoaders = [];
}

module.exports = {
  entry: ['./src/index.ts'],
  output: {
    filename: 'build.js',
    path: 'tmp'
  },
  resolve: {
    root: __dirname,
    extensions: ['', '.ts', '.js', '.json']
  },
  resolveLoader: {
    modulesDirectories: ["node_modules"]
  },
  devtool: "source-map-inline",
  plugins: [
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
    preLoaders: [
          // Tslint loader support for *.ts files
          //
          // See: https://github.com/wbuchwalter/tslint-loader
            { test: /\.ts$/, loader: 'tslint-loader', exclude: ['../node_modules'] }
        ]
  }
};

