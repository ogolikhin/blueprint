# Nova Web

## Setup
If you haven't installed node.js please install it first (latest LTS version from https://nodejs.org). 

**Notes for Windows environment**:
* Synchronize installed Node.JS version with Visual Studio 2015 if you are going to use Task runner from VS2015 - 
[link](http://ryanhayes.net/synchronize-node-js-install-version-with-visual-studio-2015/)

* If in the console window we are getting the error message that 'gulp' is not recognized as an internal or external command, operable program or batch file,
add the following path %AppData%\npm to system environment variable PATH (it's better to add it before path to nodejs, it will simplify npm update later).

Then run `devsetup` (on Windows) to install required npm packages globally.
```
devsetup
```

We don't have full list of required npm packages yet, so run `npm i` after pulling latest code from the repository or if gulp/npm complains about missing dependencies. You may also need to run \blueprint\svc\db\AdminStorage\AdminStorage_Migration.sql, otherwise new label will not show up in the interface.

### npm - single package manager for front-end and dev dependencies 
Use `npm install --save-dev` to install node-modules used in development/build or testing.
Now it includes webpack, karma, gulp, typescript, typings and multiple plugins (webpack/karma/gulp). Please adjust this as needed going forward

Use `npm install --save` for grabbing open source client side libraries (bower is not used anymore). 
Client side libraries that are not open source (example mxgraph) should be added under version control (libs folder)

### ~~typings -~~ TypeScript definition files
~~Since `tsd` marked as obsolete we are using `typings` to manage TypeScript definitions [(Usage examples)](https://www.npmjs.com/package/typings#quick-start)~~

After the migration to Typescript 2 we are also using npm to get TS definitions, see [available packages](https://www.npmjs.com/~types).

If typings definitions are not available anywhere:
1. create your own definitions `.d.ts` file and place it in `typings/custom/` directory.
2. add a reference to that file in `typings/addons.d.ts`

## Development
### Standards
1. Difference with existing TypeScript [Coding Standards](https://blueprintsys.sharepoint.com/rnd/_layouts/15/guestaccess.aspx?guestaccesstoken=M15zPSIw%2b8V38RkXKY7kVTZ0wsb%2brsHTC0x3J28C%2bhs%3d&docid=0c8dac94f55404e1680e2a2146c6350c2):
  * Use lower-case hyphen separated name convention for file and folder names!
    * For example _class CustomPropertiesSvc_ should be located in `custom-properties.svc.ts` file
  * Use ES2015/TS import/export to define dependencies (instead of TS references)
  * Use [Angular 1.5 components](https://code.angularjs.org/1.5.3/docs/guide/component) /[Course on Pluralsight](https://app.pluralsight.com/library/courses/building-components-angular-1-5/table-of-contents)/ instead of separated view and controller. Use directives when you need advanced directive definition options like priority, terminal, multi-element or when restricted to attribute.
  * Avoid using $scope object (it is possible to use it when you need to notify Angular about changes completed outside of Angular scope. For example use $scope.$applyAsync when integrating with non Angular libraries)
2. All client side labels and messages should be ready for localization (right now we supporting only en-US. **Please add new labels only for en-US**, but not for fr-CA as it's not required). Please look at existing [Localization Best Practices - Strings in HTML - Localization Keys](https://blueprintsys.sharepoint.com/rnd/_layouts/15/guestaccess.aspx?guestaccesstoken=iBqQRHfCLTIEVJtpvZ0qquKLmr52v90H%2brBbSOmZRWI%3d&docid=0ad77a05c9de2460f86ca2dec01e8dfd4). While the following document explains how to create and maintain localization strings for Nova client. [Nova Web Application - Localization Primer](https://github.com/BlueprintSys/blueprint/blob/develop/app/NovaWeb/doc/Nova%20Web%20Application%20-%20Localization%20Primer.docx?raw=true). svc\db\AdminStorage\AdminStorage_Migration.sql needs to be executed when you getting latest version from Git, otherwise no new label will show up in the interface.
 
Some of these rules are enforced by tslint (for `npm run dev` and `npm run test` tasks). Please, pay attention for tslint warning messages and fix them when working with the code.

### Workflow
We have number of npm scripts for main developer activities (look at `scripts` section of package.json). They also available as gulp tasks to support Task Runners in VS/VS Code.

* Use `gulp help` to see list of available tasks or look at `scripts` section of package.json for npm scripts.
* Use `gulp build` or `npm run build` to build production version in the dist folder.
* Use `gulp dev` or `npm run dev` to build the project and start a browser-sync dev server with live-reload on default port (8000). Currently there are no server components but we are using proxy to redirect all /svc calls to http://localhost:9801/svc.

You can override default backend URL from command line:
```
npm run dev --backend=http://titan.blueprintsys.net
```
or 'permanently' in the user profile
```
npm config set nova:backend http://titan.blueprintsys.net
```

### Unit testing
Note: all unit test are located together with the code using next pattern: [name-of-file-under-test].spec.ts, for example see src\modules\shell\login\auth.svc.spec.ts. 
It's possible to create special 'tests' folder with tests when component folder already contains many files.

You may need to install karma-chrome-launcher: `npm i karma-chrome-launcher` to run (and debug) unit tests in Chrome browser

* Use `gulp test` to run all unit tests using Karma and PhantomJS. 
* Use `gulp test:debug` to run all unit tests using Karma and Chrome. 
  * Cancel the task to close the browser instance.
* Use `npm run test:spec --spec=auth.svc` to run all tests from auth.svc.spec.ts file using Karma and Chrome. 
  * Karma will watch for the changes in the spec and related files to transpile the changes and re-run unit tests.
  * You cannot run it from VS2015 task runner because it require parameter to specify spec file
  * Cancel the script in shell to close the Chrome instance.
