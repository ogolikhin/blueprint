"use strict";

var executionEnvironmentDetector = (function () {
    executionEnvironmentDetector.prototype.userBrowser = {};

    function executionEnvironmentDetector() {
        this.userBrowser = this.getBrowserInfo();
    }
    executionEnvironmentDetector.prototype.getBrowserInfo = function () {
        var ua = window.navigator !== undefined ? window.navigator.userAgent: "";
        return this.getBrowserInfoUserAgent(ua, bowser);
    };
    executionEnvironmentDetector.prototype.getBrowserInfoUserAgent = function (ua, browser) {
        function getFirstMatch(regex) {
            var match = ua.match(regex);
            return (match && match.length > 1 && match[1]) || '';
        }

        if (/tablet pc/i.test(ua)) {
            browser.tablet = false;
        }
        if(browser.android) {
            if (!browser.mobile) {
                browser.tablet = true;
            } else if (!(/Mobile/i.test(ua))) { //in case bowser thinks it's mobile but the UA has 'mobile' in it
                browser.tablet = true;
            }
        }
        if (browser.osversion !== undefined) {
            browser.osMajorVersion = parseInt(browser.osversion.split(".")[0], 10);
        }
        if (browser.ios) {
            if (/safari/i.test(ua)) {
                browser.safari = true;
            }
        }
        if (/Mac\sOS\sX/i.test(ua)) {
            browser.osx = true;
            var osxVersion = getFirstMatch(/Mac\sOS\sX\s(\d+([_.]\d+)?)/i).replace(/_/g, ".");
            osxVersion = osxVersion.split(".");
            if(parseInt(osxVersion[0], 10) >= 11 ||
                (osxVersion.length == 2 && parseInt(osxVersion[0], 10) == 10 && parseInt(osxVersion[1], 10) >= 9)) browser.osx10_9plus = true;
        }
        else if (/Windows\sNT\s6.1/i.test(ua) ||
            /Windows\sNT\s6.2/i.test(ua) ||
            /Windows\sNT\s6.3/i.test(ua) ||
            /Windows\sNT\s10/i.test(ua)) { // in Windows 10 Microsoft jumped from 6.3 (Windows 8.1) to 10.0
            browser.win7plus = true;

            if (/Windows\sNT\s10/i.test(ua)) { // we also want to detect Windows 10 separately from Windows 7/8/8.1 as the Edge browser works only on Windows 10
                browser.win10plus = true;
            }
        }
        browser.ua = ua;
        browser.blueprintSupportedBrowser = false;
        if (browser.mobile) {
            if (!browser.tablet) {
                browser.blueprintSupportedBrowser = false;
                return browser;
            } else {
                browser.mobile = false;
            }
        }

        if (browser.msie && parseInt(browser.version, 10) >= 11 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && parseInt(browser.version, 10) >= 40 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && parseInt(browser.version, 10) >= 40 && browser.osx && browser.osx10_9plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && parseInt(browser.version, 10) >= 40 && browser.ios && browser.osMajorVersion >= 8) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (!browser.chrome && browser.safari && parseInt(browser.version, 10) >= 7 && browser.ios && browser.osMajorVersion >= 8) {
            //Chrome UA in iOS has "Safari" too, so we need to make sure that is not Chrome when we test Safari
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && parseInt(browser.version, 10) >= 40 && browser.android && browser.osMajorVersion >= 5) {
            browser.blueprintSupportedBrowser = true;
        }
        return browser;
    };
    executionEnvironmentDetector.prototype.isSupportedVersion = function() {
        return this.userBrowser.blueprintSupportedBrowser;
    };
    executionEnvironmentDetector.prototype.isTouchDevice = function() {
        return this.userBrowser.tablet || this.userBrowser.mobile;
    };
    executionEnvironmentDetector.prototype.isWindows = function() {
        return this.userBrowser.win7plus;
    };
    executionEnvironmentDetector.prototype.isMacOSX = function() {
        return this.userBrowser.osx;
    };
    executionEnvironmentDetector.prototype.isiOS = function() {
        return this.userBrowser.ios;
    };
    executionEnvironmentDetector.prototype.isAndroid = function() {
        return this.userBrowser.android;
    };
    executionEnvironmentDetector.prototype.isIE = function() {
        return this.userBrowser.msie;
    };
    executionEnvironmentDetector.prototype.isChrome = function() {
        return this.userBrowser.chrome;
    };
    executionEnvironmentDetector.prototype.isSafari = function() {
        return this.userBrowser.safari;
    };
    return executionEnvironmentDetector;
}());

var appBootstrap = (function() {
    appBootstrap.prototype.executionEnvironment = {};

    function appBootstrap() {
        this.executionEnvironment = new executionEnvironmentDetector();
    }

    appBootstrap.prototype.isSupportedVersion = (function () {
        if (new executionEnvironmentDetector().isSupportedVersion()) {
            return true;
        }

        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {

                var divUnsupportedBrowser = document.createElement("div");
                divUnsupportedBrowser.id = "unsupported-browser-container";
                divUnsupportedBrowser.innerHTML = xhr.responseText;
                document.body.insertBefore(divUnsupportedBrowser, document.body.firstChild);

                var innerScript = divUnsupportedBrowser.getElementsByTagName("script");
                for (var i = 0; i < innerScript.length; i++) {
                    eval(innerScript[i].text);
                }
            }
        };
        xhr.open('GET', '/novaweb/static/unsupported-browser.html');
        xhr.send();
        return false;
    })();

    appBootstrap.prototype.initApp = function() {
        var app = angular.module("app", ["app.main"]);

        if (this.executionEnvironment.isTouchDevice()) {
            document.body.className += " is-touch";
            // if touch device, we set the min-height to the screen height resolution so that the user can swipe up and
            // remove the browser chrome, therefore maximizing the available space. Recalculates on orientation change.
            document.body.style.minHeight = screen.height + "px";

            window.addEventListener("orientationchange", function() {
                document.body.style.minHeight = screen.height + "px";
            });
        }
        if (this.executionEnvironment.isAndroid()) {
            document.body.className += " is-android";
        }
        if (this.executionEnvironment.isiOS()) {
            document.body.className += " is-ios";
        }
        if (this.executionEnvironment.isWindows()) {
            document.body.className += " is-windows";
        }
        if (this.executionEnvironment.isMacOSX()) {
            document.body.className += " is-macosx";
        }
        if (this.executionEnvironment.isChrome()) {
            document.body.className += " is-chrome";
        }
        if (this.executionEnvironment.isIE()) {
            document.body.className += " is-msie";
        }
        if (this.executionEnvironment.isSafari()) {
            document.body.className += " is-safari";
        }

        angular.bootstrap(document, ["app"], {
            strictDi: true
        });
    };

    return new appBootstrap();
})();
