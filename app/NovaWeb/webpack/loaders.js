var ExtractTextPlugin = require("extract-text-webpack-plugin");
var path = require("path");
module.exports = [
    {
        test: /\.ts$/,
        exclude: [
            /node_modules/,
            path.join(__dirname, "../src/fonts"),
            path.join(__dirname, "../src/images"),
            path.join(__dirname, "../src/styles")
        ],
        loader: "awesome-typescript",
        include: [
            path.join(__dirname, "../src")
        ]
    },
    {
        test: /\.css$/,
        loader: ExtractTextPlugin.extract("style",
            ["css", "postcss"]),

        include: [
            //important for performance!
            path.join(__dirname, "../libs"),
            path.join(__dirname, "../node_modules")
        ]
    },
    {
        test: /\.scss$/,
        loader: ExtractTextPlugin.extract("style",
            ["css?sourceMap", "postcss", "sass?outputStyle=expanded&sourceMap=true&sourceMapContents=true"]),
        exclude: [
            path.join(__dirname, "../src/fonts"),
            path.join(__dirname, "../src/images")
        ],
        include: [
            path.join(__dirname, "../libs"),
            path.join(__dirname, "../node_modules"),
            path.join(__dirname, "../src")
        ]
    },
    {
        test: /\.json$/,
        exclude: /node_modules/,
        loader: "raw",
        includes: [
            path.join(__dirname, "../src")

        ]
    },
    {
        test: /\.xml$/,
        exclude: /node_modules/,
        loader: "raw",
        includes: [
            path.join(__dirname, "../libs/mxClient")

        ]
    },
    {
        test: /\.html$/,
        exclude: /node_modules/,
        loader: "raw",
        include: [
            path.join(__dirname, "../src/modules"),
            path.join(__dirname, "../src/unsupported-browser")

        ]
    },
    {
        test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        loader: "url?limit=10000&mimetype=application/font-woff",
        include: [
            path.join(__dirname, "../src/fonts"),
            path.join(__dirname, "../node_modules")
        ]
    },
    {
        test: /\.(ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
        loader: "url?limit=10000"
    },
    {
        test: /\.jpg$/,
        exclude: /node_modules/,
        loader: "file"
    },
    {
        test: /\.gif$/,
        exclude: /node_modules/,
        loader: "file"
    },
    {
        test: /\.png$/,
        exclude: /node_modules/,
        loader: "url"
    },
    {
        test: require.resolve("tinymce/tinymce"),
        loaders: [
            "imports?this=>window",
            "exports?window.tinymce"
        ]
    },
    {
        test: /tinymce\/(themes|plugins)\//,
        loaders: [
            "imports?this=>window"
        ]
    },
    {
        test: require.resolve(path.join(__dirname, "../libs/mxClient/js/mxClient.js")),
        loaders: [
            'imports?mxBasePath=>"./novaweb/libs/mxClient", mxLoadStylesheets=>false, mxLoadResources=>false'
        ],
        include: [
            path.join(__dirname, "../libs/mxClient")
        ]
    },
    {
        test: require.resolve(path.join(__dirname, "../libs/mxClient/js/mxClient.min.js")),
        loaders: [
            'imports?mxBasePath=>"./novaweb/libs/mxClient", mxLoadStylesheets=>false, mxLoadResources=>false'
        ],
        include: [
            path.join(__dirname, "../libs/mxClient")
        ]
    }
];
