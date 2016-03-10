# Nova Prototype

If you haven't installed node.js, bower and gulp globally, please install them first.
```
 npm install -g bower
 npm install -g gulp
```

In order to run nova prototype simply run:
```
 npm install
 bower install
 gulp serve
```

## npm install
package.json file that is used to install node-modules currently includes bower, gulp and components we need for the prototype. Please adjust this as needed going forward

## bower install
bower is used for grabing open source client side libraries.
Note that for client side libraries that are not open source (example mxgraph), we will not be using bower

## gulp 
A number of tasks are defined for minification and typescript compilation but they are not currently in use. They definetely need to be tested when used as part of build step or from the IDE

Use
```
gulp serve
```
to start a gulp web-server with live-reload on default port (8000). Currently there are no server components but we can use proxy to redirect all /svc calls to blueprint/blueprint-current REST services

# Synchronize Node.JS Install Version with Visual Studio 2015:
http://ryanhayes.net/synchronize-node-js-install-version-with-visual-studio-2015/ 
