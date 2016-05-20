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
Nova prototype makes use of http://www.jqwidgets.com/ for the main experience and in the visible tab, right after starting the server, you'd see an empty scenario manager where you can create new instances of scenarios from the tree on explorer pane.
You may also open scenario manager standalone prototype by opening http://localhost:8000/scenario-manager.html

## npm install
package.json file that is used to install node-modules currently includes bower, gulp and components we need for the prototype. Please adjust this as needed going forward

## bower install
bower is used for grabing open source client side libraries, I tried to mimic what we are using for Impact Analysis and Rapid Review here plus other components I use for the prototype.

Note that for client side libraries that are not open source (example mxgraph), we will not be using bower

## gulp 
A number of tasks are defined for minification and typescript compilation but they are not currently in use. They definetely need to be tested when used as part of build step or from the IDE

Enter
```
gulp help 
```
for the list pf the available task. 

Use
```
gulp serve
```
to start a gulp web-server with live-reload on default port (8000). Currently there are no server components other than a number of json mock objects that has to be served from webserve as they behave differently without a webserver


