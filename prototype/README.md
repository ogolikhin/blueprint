# Nova Prototypes

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
bower is used for grabing open source client side libraries, I tried to mimic what we are using for Impact Analysis and Rapid Review here plus other components I use for the prototype.

Note that for client side libraries that are not open source (example mxgraph), we will not be using bower

## gulp 
Enter
```
gulp serve
```
to start a gulp web-server with live-reload on default port (9000). Currently there are not that many server components other than a number of json mock objects that has to be served from webserve as they behave differently without a webserver

In http://localhost:9000 see links to various prototypes using different widget libraries.
Please see this document for more info https://github.com/BlueprintSys/blueprint/tree/develop/app/Blueprint/doc


