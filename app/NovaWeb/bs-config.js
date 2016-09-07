
/*
 |--------------------------------------------------------------------------
 | Browser-sync config file
 |--------------------------------------------------------------------------
 |
 | For up-to-date information about the options:
 |   http://www.browsersync.io/docs/options/
 |
 | There are more options than you see here, these are just the ones that are
 | set internally. See the website for more info.
 |
 |
 */
var backend = "http://localhost:9801";
console.log({backend: backend});

var url = require('url'),
   proxy = require('proxy-middleware');
var proxyOptions = url.parse(backend + '/svc');
proxyOptions.route = '/svc';
var loginProxyOptions = url.parse(backend + '/Login/WinLogin.aspx');
loginProxyOptions.route = '/Login/WinLogin.aspx';
module.exports = {
    "host": "localhost",
    "port": 8000,
    "server": {
                "baseDir": "dist",
                "middleware": [proxy(proxyOptions), proxy(loginProxyOptions)]
                },
    "ui": false,
    "online": false,
    "notify": false,
    "ghostMode": false 
};