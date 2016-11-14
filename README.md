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
* `npm run test` - run the complete site test suite
* `npm run test:debug`
* `npm run test:spec`
* `npm run build`<sup>*</sup> - bundle and minify the site for 'production' deployment
* `npm run lint`
* `npm run lint:all`
* `npm run lint:nospec`

* <sup>*</sup>once build if you need to view locally you can run `node dist backend=http://nw.blueprintsys.net` or any other API location
* <sup>^</sup>Optional parms can be added to specify external backend `-- --backend=http://nw.blueprintsys.net` or any other API location

# branch info

master - trunk

hotfix - production code hotfix branch

release - target release code branch, bug fix merge

dev - main development code branch, regular development merge

---

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

---
## DevOps - End of Release Process
1. Within the **_high-impact_** and **_general_** channels on SLACK, notify that release branching will occur in xx minutes.

   _Example Message:_
  
   **@channel:** Will be creating a _release-\<CODENAME>_ branch in the next few minutes. Starting from _\<TIME>_ EST, please do not check in any code into the _develop_ branch of either blueprint or blueprint-current repos. Also, please do not kick off any **build-develop** or **build-develop-nwa** builds in Jenkins. Thank you.


2. For each GitHub repository, create a new _release-\<CODENAME>_ (Repositories include: blueprint, blueprint-current, blueprint-tools)
	- Locally, run the following git commands:
	
	   `#> git checkout develop`

	   `#> git pull origin develop`
	   
	   `#> git checkout -b "release-<CODENAME>"`
	   
	   `#> git push origin release-<CODENAME>`
	   

	- In GitHub, draft a new release, assign it to the newly created "_release-\<CODENAME>_" branch
		- Release Title = MajorVersion.MinorVerison
		- 'Publish Release'


3. Create Jenkins Jobs for *release-\<CODENAME>* branches
	- From the Jenkins main page, create a new projects by using the _develop_ projects as your template.
	- **_Copper Builds:_**
		- Copy **copper-raptor-develop** and name **copper-raptor-_release-\<CODENAME>_**
			- Within the Source Code Management section of the newly created project's configuration, replace '_develop_' with '_release-\<CODENAME>_'
		- Copy **copper-nova-develop** and name **copper-nova-_release-\<CODENAME>_**
			- Within the Source Code Management section of the newly created project's configuration, replace '_develop_' with '_release-\<CODENAME>_'
		- Update the **copper-nova-develop** project's configuration such that the default value for the `BP_Major_Version`, `BP_Minor_Version` parameters reflects the up coming release version
	- **_Silver Builds:_**
		- Copy **build-develop** and name **build-_release-\<CODENAME>_**
			- Within the newly created project's configuration, replace all occurances of '_develop_' with '_release-\<CODENAME>_' within the project's parameters
		- Update the **build-develop** and **build-develop-nwa** projects such that the default value for the `BlueprintMajorVersion` parameter reflects the up coming release version


4. Update application version on the _develop_ branch across all repositories and push changes to the _develop_ branch
	- For an example of code changes that need to be applied to update the application version, please refer to the following pull requests:
		- blueprint-current: #2173 https://git.io/voGNf 
		- blueprint: #672 https://git.io/voGNY


5. Until silver-pipeline is completed, temporarly have integration tests use the **_release-\<CODENAME>_** build


6. Once regression testing is completed for _release-\<CODENAME>_, update the integration tests to use the **develop** build
