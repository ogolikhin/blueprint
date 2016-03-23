# Nova Web

Please be aware: we are using lower-case hypen separated name convention for file and folder names!

## Setup
If you haven't installed node.js please install it first (latest LTS version from https://nodejs.org). Then run devsetup (on Windows) to install required npm packages globally.
```
 devsetup
```

We don't have full list of required npm packages yet, so run
```
npm i
```
after pulling latest code from the repository or if gulp/npm complains about missing dependencies.

In order to run nova prototype simply run (see gulp section below for other tasks):
```
 gulp dev
```
or
```
npm run dev
```

## npm install --save-dev
package.json file that is used to install node-modules currently includes webpackage, karma, gulp, typescript, typings and components we need for the prototype. Please adjust this as needed going forward

## npm install --save
we also using npm for grabbing open source client side libraries (bower is not used anymore)
Note that for client side libraries that are not open source (example mxgraph), we will not be using bower

## gulp 
We have number of gulp tasks:

Use
```
gulp help
```
to see list of available tasks.

Use
```
gulp build
```
to build production version in the dist folder.

Use
```
gulp dev
```
to build the project and start a browser-sync dev server with live-reload on default port (8000). Currently there are no server components but we are using proxy to redirect all /svc calls to http://localhost:9801/svc

Use
```
gulp test
```
to run unit tests using Karma and PhantomJS. 
Note: all unit test are located together with the code using next pattern: [name_of_file_under_test].spec.ts, see src\modules\shell\login\auth.svc.spec.ts for example.

Use
```
gulp serve
```
to build the project in production version and run it with browser-sync server.

Note: you also can run all these tasks (except serve) using
```
npm run {dev | test | build}
```
as npm scripts defined in package.json 

# Synchronize Node.JS Install Version with Visual Studio 2015:
http://ryanhayes.net/synchronize-node-js-install-version-with-visual-studio-2015/

# If in the console window we are getting the error message that 'typings' is not recognized as an internal or external command, operable program or batch file,
add the following path %APPDATA%\npm to system environment variable PATH.
