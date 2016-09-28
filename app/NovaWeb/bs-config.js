
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
var proxy_config = require('./webpack/proxy.dev');

module.exports = {
    "host": "localhost",
    "port": 8000,
    "server": {
        "baseDir": "dist",
        "middleware": proxy_config
    },
    "ui": false,
    "online": false,
    "notify": false,
    "ghostMode": false
};