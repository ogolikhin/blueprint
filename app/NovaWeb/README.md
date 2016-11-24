# Nova Web

## Setup
If you haven't installed node.js please install it first (latest LTS version from [https://nodejs.org](https://nodejs.org)). 

Developers may use whatever IDE they wish, however the IDE must support the following:  
* Editorconfig
* TSLint
* Sytnax highlighting  

Most common IDE's are [Webstorm](https://www.jetbrains.com/webstorm/) or [VSCode](https://code.visualstudio.com). _VSCode requires additional plugins configuration_

run `devsetup` (on Windows) to install required npm packages globally.
```
devsetup
```

We don't have full list of required npm packages yet, so run `npm i` after pulling latest code from the repository or if gulp/npm complains about missing dependencies. You may also need to run \blueprint\svc\db\AdminStorage\AdminStorage_Migration.sql, otherwise new label will not show up in the interface.

### npm - single package manager for front-end and dev dependencies 
Use `npm install --save-dev` to install node-modules used in development/build or testing.
Now it includes webpack, karma, gulp, typescript, typings and multiple plugins (webpack/karma/gulp). Please adjust this as needed going forward

Use `npm install --save` for grabbing open source client side libraries (bower is not used anymore). 
Client side libraries that are not open source (example mxgraph) should be added under version control (libs folder)

### TypeScript definition files

If you install a new npm package for development please use `typings search` to find the appropriate definition library to install.  
 
 Some packages must be installed with the `--global` switch while others simply can be installed with `typings --save PACKAGE_NAME`

## Development
### Standards
1. Code standards are maintained in the [project wiki](https://github.com/BlueprintSys/blueprint/wiki/Code-Standards). All developers are required to know and follow all standards.  
Questions are welcome
2. Use camelCase name convention for file and folder names! Identify type (service, controller, filter, component, directive) by separating with a `.`
    * For example `class CustomPropertiesService` should be located in `CustomProperties.service.ts` file
2. Use ES2015/TS import/export to define dependencies.
2. Use [Angular 1.5 components](https://code.angularjs.org/1.5.3/docs/guide/component) /[Course on Pluralsight](https://app.pluralsight.com/library/courses/building-components-angular-1-5/table-of-contents)/ instead of separated view and controller. Use directives when you need advanced directive definition options like priority, terminal, multi-element or when restricted to attribute.
2. Avoid using $scope object (it is possible to use it when you need to notify Angular about changes completed outside of Angular scope. For example use $scope.$applyAsync when integrating with non Angular libraries)
2. All client side labels and messages should be ready for localization (right now we supporting only en-US. **Please add new labels only for en-US**, but not for fr-CA as it's not required). Please look at existing [Localization Best Practices - Strings in HTML - Localization Keys](https://blueprintsys.sharepoint.com/rnd/_layouts/15/guestaccess.aspx?guestaccesstoken=iBqQRHfCLTIEVJtpvZ0qquKLmr52v90H%2brBbSOmZRWI%3d&docid=0ad77a05c9de2460f86ca2dec01e8dfd4). While the following document explains how to create and maintain localization strings for Nova client. [Nova Web Application - Localization Primer](https://github.com/BlueprintSys/blueprint/blob/develop/app/NovaWeb/doc/Nova%20Web%20Application%20-%20Localization%20Primer.docx?raw=true). svc\db\AdminStorage\AdminStorage_Migration.sql needs to be executed when you getting latest version from Git, otherwise no new label will show up in the interface.
 
**<center>Many of these rules are enforced by tslint.  
Please, pay attention for tslint warning messages as they will result in build errors.</center>**

### Workflow
We have number of npm scripts for main developer activities (look at `scripts` section of package.json).

* Use `npm run dev:open-browser` to build the project and launch the browser on the default host and port (http://localhost:8000). After using this script, `npm run dev` will launch the browser as well.
* Use `npm run dev:no-browser` to build the project and stop launching the browser. After using this script, `npm start` won't launch the browser anymore.
* Use `npm run dev:public` to build the project and binds the server to all hosts. This is useful if you need to give remote access to your local server (e.g. by using your machine's IP address: http://XXX.XXX.XXX.XXX:8000). Please note that the you will need to manually launch the browser.

You can override default backend URL from command line:
```
npm start --backend=http://titan.blueprintsys.net
```
or 'permanently' in the user profile
```
npm config set nova:backend http://titan.blueprintsys.net
```

### Styleguide  

To help with UX and Creative work flow as well as to ensure proper re-use of components we are leveraging a styleguid. 
Serveral options for this exist however most involve maintaining a second site. As this is a costly options we have decided
 to use an automated utility to use [SC5 Style Guide Generator](http://styleguide.sc5.io/).  
 
 Usage of this automated generate depends on adding specifically formatted comments into the `.scss` files on the site. 
 These comments follow [KSS Syntax](http://warpspire.com/kss/syntax/). Take time to read the specific examples on [documenting syntax](https://github.com/SC5/sc5-styleguide#documenting-syntax).  
 
 You can access the style guide at [http://localhost:4000](http://localhost:4000) by the following commands:
  * `gulp styleguide`      - will build the styleguide
  * `gulp styleguide:dev`  - will build the styleguide with live watch on files (must manually refresh browser)


### Unit testing
Note: all unit test are located together with the code using next pattern: [name-of-file-under-test].spec.ts, for example see src\modules\shell\login\auth.svc.spec.ts. 
It's possible to create special 'tests' folder with tests when component folder already contains many files.

Use next pattern: [name-of-service.svc].mock.ts when you need to create mock for existing service.

Unit test and mock files are excluded from the code coverage report based on pattern (.spec|.mock).ts.

* Use `gulp test` to run all unit tests using Karma and PhantomJS. 
* Use `gulp test:debug` to run all unit tests using Karma and Chrome. 
  * Cancel the task to close the browser instance.
* Use `npm run test:spec --spec=auth.svc` to run all tests from auth.svc.spec.ts file using Karma and Chrome. 
  * Karma will watch for the changes in the spec and related files to transpile the changes and re-run unit tests.
  * You cannot run it from VS2015 task runner because it require parameter to specify spec file
  * Cancel the script in shell to close the Chrome instance.
