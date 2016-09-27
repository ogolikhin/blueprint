var backend = process.env.npm_config_backend || process.env.npm_package_config_backend || "http://localhost:9801";
console.log({backend: backend});

var url = require('url'),
   proxy = require('proxy-middleware');
var svcProxyOptions = url.parse(backend + '/svc');
svcProxyOptions.route = '/svc';
var sharedProxyOptions = url.parse(backend + '/shared');
sharedProxyOptions.route = '/shared';

module.exports = [proxy(svcProxyOptions), proxy(sharedProxyOptions)];