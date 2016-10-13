var ExtractTextPlugin = require("extract-text-webpack-plugin");
var path = require("path");
module.exports = [
    {
        test: /\.ts(x?)$/,
        exclude: [

        ],
        loader: 'awesome-typescript-loader'
    },     {
        test: /\.css$/,
        loader: ExtractTextPlugin.extract('style-loader', 'css-loader?sourceMap')
    },     {
        test: /\.scss$/,
        loader: ExtractTextPlugin.extract('style-loader', 'css?sourceMap!sass?sourceMap')
    }, {
        test: /\.html$/,
        exclude: /node_modules/,
        loader: 'raw'
    }, {
        test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        loader: 'url-loader?limit=10000&mimetype=application/font-woff'
    }, {
        test: /\.(ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        loader: 'file-loader'
    }, {
        test: /\.jpg$/,
        exclude: /node_modules/,
        loader: 'file'
    }, {
        test: /\.gif$/,
        exclude: /node_modules/,
        loader: 'file'
    }, {
        test: /\.png$/,
        exclude: /node_modules/,
        loader: 'url'
    }, {
        test: require.resolve('tinymce/tinymce'),
        loaders: [
          'imports?this=>window',
          'exports?window.tinymce'
        ]
    }, {
          test: /tinymce\/(themes|plugins)\//,
          loaders: [
            'imports?this=>window'
          ]
    }
];


