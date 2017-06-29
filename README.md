# blueprint

The repo for HTML5 / SPA and distributed services code.  

## Running the site

The HTML5 site requires webpack to run and supports multiple entry points as outlined below
* `npm start` - quick alies to run the site. Should be default way to start the site
* `npm run dev`<sup>^</sup> - bundle the site for local development
* `npm run dev:open-browser` - bundle the site for local development and launch default browser
* `npm run dev:no-browser`
* `npm run dev:build`
* `npm run dev:public`
* `npm run pretest` - runs tslint over all .ts files (excluding .mock and .spec) using dev rules
* `npm run test` - run the complete site test suite
* `npm run test:debug`
* `npm run test:spec --spec=[FILENAME]` - tests an individual file
* `npm run build`<sup>*</sup> - bundle and minify the site for 'production' deployment

* <sup>*</sup>once build if you need to view locally you can run `node dist backend=http://nw.blueprintsys.net` or any other API location
* <sup>^</sup>Optional parms can be added to specify external backend `-- --backend=http://nw.blueprintsys.net` or any other API location

# branch info

master - trunk

hotfix - production code hotfix branch

release - target release code branch, bug fix merge

dev - main development code branch, regular development merge

----

###Copper Pipeline
Unit Tests - Nova Services
[![Build Status](https://jenkins.blueprintsys.net/buildStatus/icon?job=copper-nova-develop)](https://jenkins.blueprintsys.net/job/copper-nova-develop)

###Silver Pipeline
##### Build
Silverlight Build [![Build Status](https://jenkins.blueprintsys.net/buildStatus/icon?job=build-develop)](https://jenkins.blueprintsys.net/job/build-develop)

Nova Web App & Silverlight Build
[![Build Status](https://jenkins.blueprintsys.net/buildStatus/icon?job=build-develop)](https://jenkins.blueprintsys.net/job/build-develop)

##### Integration Test Site Deployment
Nova
[![Build Status](https://jenkins.blueprintsys.net/buildStatus/icon?job=deploy-site-novaIntegration)](https://jenkins.blueprintsys.net/job/deploy-site-novaIntegration)

##### Integration Tests
Nova
[![Build Status](https://jenkins.blueprintsys.net/buildStatus/icon?job=test-integration-nova-develop)](https://jenkins.blueprintsys.net/job/test-integration-nova-develop)
