var commandLineArgs = require('command-line-args');

var optionDefinitions = [
    { name: 'backend', type: String, defaultValue: "http://localhost:9801"}
];

var express = require('express'),
    proxy = require('http-proxy-middleware'),
    app = express(),
    build = 'dist',
    proxyPort = 8000;

var options = commandLineArgs(optionDefinitions);

console.log({backend: options.backend});

app.use(express.static(build));

var proxyOptions = {
    target: options.backend,
    changeOrigin: true
};

var apiProxy = proxy(proxyOptions);

app.use("/svc/*", apiProxy);

// app.use('/js', express.static(__dirname + '/svx'));
// app.use('/dist', express.static(__dirname + '/../dist'));
// app.use('/css', express.static(__dirname + '/css'));
// app.use('/partials', express.static(__dirname + '/partials'));
//
// app.all('/*', function(req, res, next) {
//     // Just send the index.html for other files to support HTML5Mode
//     res.sendFile('index.html', { root: __dirname });
// });

app.listen(proxyPort, function () {
    console.log('Example app listening on port ', proxyPort, '!')
});
