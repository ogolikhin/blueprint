"use strict";

var executionEnvironmentDetector = (function () {
    function executionEnvironmentDetector() {
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
        if(browser.osversion !== undefined)
            browser.osMajorVersion = parseInt(browser.osversion.split(".")[0], 10);
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
        if (bowser.mobile || bowser.tablet) {
            browser.blueprintSupportedBrowser = false;
        }
        else if (browser.msie && parseInt(browser.version, 10) >= 11 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.msedge && browser.win10plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.firefox && parseInt(browser.version, 10) >= 19 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && parseInt(browser.version, 10) >= 25 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && browser.osx && browser.osx10_9plus) {
            browser.blueprintSupportedBrowser = true;
        }
        return browser;
    };
    executionEnvironmentDetector.prototype.isSupportedVersion = function() {
        return this.getBrowserInfo().blueprintSupportedBrowser;
        };
    return executionEnvironmentDetector;
}());

var appBootstrap = (function() {
    function appBootstrap() {
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
    }());

    appBootstrap.prototype.initApp = function() {
        var app = angular.module("app", ["app.main"])

        angular.bootstrap(document, ["app"], {
            strictDi: true
        });
    };

    return new appBootstrap();
}());
