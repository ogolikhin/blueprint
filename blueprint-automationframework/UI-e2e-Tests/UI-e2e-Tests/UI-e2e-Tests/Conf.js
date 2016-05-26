function allure_report_jetty_deploy() {
    console.log('Generating allure reports from xml using maven plugin and deploying them on port:1234[localhost or jenkins node ip] via jetty server.It should not take more than 1 minute......');
    console.log('If at times there is some issue in report deployment or reports are not available on mentioned port, please restart jenkins master and re run the test build');
    
    var exec = require('child_process').exec;
    
    function puts(error, stdout, stderr) {
        sys.puts(stdout)
    }
    exec("mvn site -Dallure.results_pattern=allure-results && mvn jetty:run -Djetty.port=1234", puts);
    var startTimes = Date.now();
    while (Date.now() - startTimes < 60000) {
    }
}

exports.config = {
    
    framework: 'jasmine2',
    seleniumAddress: 'http://localhost:4444/wd/hub',
    specs: ['./Specs/Storyteller/LoginTestSpec.js', './Specs/Storyteller/EditingNavigatingModalStorytellerSpec.js'],
    allScriptsTimeout: 20000,
    onPrepare: function () {
        browser.driver.manage().window().maximize();
        browser.manage().timeouts().implicitlyWait(25000);
        var logger = require('winston');
        logger.add(logger.transports.File, { filename: './Log/logfile.log' });
        
        var AllureReporter = require('jasmine-allure-reporter');
        jasmine.getEnv().addReporter(new AllureReporter({
            allureReport: {
                resultsDir: './allure-results/'
            }
        }));
        
        jasmine.getEnv().afterEach(function (done) {
            browser.takeScreenshot().then(function (png) {
                allure.createAttachment('Screenshot', function () {
                    return new Buffer(png, 'base64')
                }, 'image/png')();
                done();
            })
        });

    },
    onComplete: function () {
        //allure_report_jetty_deploy();
       // send_mail();
    },
    
    jasmineNodeOpts: {
        // onComplete will be called just before the driver quits.
        onComplete: null,
        // If true, display spec names.
        isVerbose: true,
        // If true, print colors to the terminal.
        showColors: true,
        // If true, include stack traces in failures.
        includeStackTrace: true,
        // Default time to wait in ms before a test fails.
        defaultTimeoutInterval: 3600000
    },
 
    multiCapabilities: [{
            'browserName': 'chrome',
            //'nativeEvents': true,
           // 'disable-popup-blocking': true
            'chromeOptions': {
                // Get rid of --ignore-certificate yellow warning
                args: ['--no-sandbox', '--test-type=browser'],
                // Set download path and avoid prompting for download even though
                // this is already the default on Chrome but for completeness
                prefs: {
                    'download': {
                        'prompt_for_download': false,
                        'default_directory': 'C:/DownloadFile'
                    },
                },
            },
           
        },]
   /*     
         multiCapabilities: [{
            'browserName': 'internet explorer',
             'platform': 'ANY',
            'version': '11',
             'nativeEvents': false,
                'unexpectedAlertBehaviour': 'accept',
            'ignoreProtectedModeSettings': true,
            'enablePersistentHover': false,
            'disable-popup-blocking': true,
        'ignoreZoomSetting': true


        },],

   capabilities: {
        'browserName': 'internet explorer',
        'version': 11,
        'nativeEvents': false,
        'unexpectedAlertBehaviour': 'accept',
        'ignoreProtectedModeSettings': true,
        'enablePersistentHover': false,
          'ignoreZoomSetting': true,
        'disable-popup-blocking': true
        }*/
};

/* {
         'browserName' : 'firefox'chrome
         }*/