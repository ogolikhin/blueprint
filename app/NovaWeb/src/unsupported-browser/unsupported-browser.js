"use strict";

var executionEnvironmentDetector = (function () {
    executionEnvironmentDetector.prototype.userBrowser = {};

    function executionEnvironmentDetector() {
        this.userBrowser = this.getBrowserInfo();
    }

    executionEnvironmentDetector.prototype.getBrowserInfo = function () {
        var ua = window.navigator !== undefined ? window.navigator.userAgent : "";
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
        if (browser.android) {
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
            if (parseInt(osxVersion[0], 10) >= 11 ||
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
// Removing tablet and mobile support for MVP
        if (browser.mobile || browser.tablet) {
            browser.blueprintSupportedBrowser = false;
            return browser;
        }
        /*
         if (browser.mobile) {
         if (!browser.tablet) {
         browser.blueprintSupportedBrowser = false;
         return browser;
         } else {
         browser.mobile = false;
         }
         }
         */

        if (browser.msie && parseInt(browser.version, 10) >= 11 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && parseInt(browser.version, 10) >= 50 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        }
        else if (browser.chrome && parseInt(browser.version, 10) >= 50 && browser.osx && browser.osx10_9plus) {
            browser.blueprintSupportedBrowser = true;
        }
// Removing tablet and mobile support for MVP
        /*
         else if (browser.chrome && parseInt(browser.version, 10) >= 50 && browser.ios && browser.osMajorVersion >= 9) {
         browser.blueprintSupportedBrowser = true;
         }
         else if (!browser.chrome && browser.safari && parseInt(browser.version, 10) >= 7 && browser.ios && browser.osMajorVersion >= 9) {
         //Chrome UA in iOS has "Safari" too, so we need to make sure that is not Chrome when we test Safari
         browser.blueprintSupportedBrowser = true;
         }
         else if (browser.chrome && parseInt(browser.version, 10) >= 50 && browser.android && browser.osMajorVersion >= 5) {
         browser.blueprintSupportedBrowser = true;
         }
         */
        return browser;
    };
    executionEnvironmentDetector.prototype.isSupportedVersion = function () {
        return this.userBrowser.blueprintSupportedBrowser;
    };
    executionEnvironmentDetector.prototype.isTouchDevice = function () {
        return this.userBrowser.tablet || this.userBrowser.mobile;
    };
    executionEnvironmentDetector.prototype.isWindows = function () {
        return this.userBrowser.win7plus;
    };
    executionEnvironmentDetector.prototype.isMacOSX = function () {
        return this.userBrowser.osx;
    };
    executionEnvironmentDetector.prototype.isiOS = function () {
        return this.userBrowser.ios;
    };
    executionEnvironmentDetector.prototype.isAndroid = function () {
        return this.userBrowser.android;
    };
    executionEnvironmentDetector.prototype.isIE = function () {
        return this.userBrowser.msie;
    };
    executionEnvironmentDetector.prototype.isChrome = function () {
        return this.userBrowser.chrome;
    };
    executionEnvironmentDetector.prototype.isSafari = function () {
        return this.userBrowser.safari;
    };
    executionEnvironmentDetector.prototype.isLandscape = function () {
        return (window.orientation && Math.abs(window.orientation) === 90);
    };
    executionEnvironmentDetector.prototype.isFontFaceSupported = function () {
        // Based on http://www.paulirish.com/2009/font-face-feature-detection/
        var
            rule = "@font-face { font-family: 'font'; src: 'font.ttf'; }",
            styleSheet,
            head = document.head || document.getElementsByTagName("head")[0] || document.documentElement,
            styleTag = document.createElement("style"),
            implementation = document.implementation || {
                    hasFeature: function () {
                        return false;
                    }
                };

        styleTag.type = "text/css";
        head.insertBefore(styleTag, head.firstChild);
        styleSheet = styleTag.sheet || styleTag.styleSheet;

        if (!styleSheet) {
            return false;
        }

        if (implementation.hasFeature("CSS2", "")) {
            var isSupported = false;
            try {
                styleSheet.insertRule(rule, 0);
                isSupported = !(/unknown/i).test(styleSheet.cssRules[0].cssText);
                styleSheet.deleteRule(0);
            } catch (e) {
            }
            return isSupported;
        } else {
            styleSheet.cssText = rule;

            return styleSheet.cssText.length !== 0 && !(/unknown/i).test(styleSheet.cssText) &&
                styleSheet.cssText
                    .replace(/\r+|\n+/g, "")
                    .indexOf(rule.split(" ")[0]) === 0;
        }
    };
    executionEnvironmentDetector.prototype.isWebfontAvailable = function (fontFace) {
        // Based on http://www.lalit.org/lab/javascript-css-font-detect/

        // The font will be compared against all the three default fonts.
        // and if it doesn't match all 3 then that font is not available.
        var baseFonts = ["monospace", "sans-serif", "serif"];

        // We use "m" (or "w") because these two characters take up the maximum width.
        // And we use a "LLi" so that the same matching fonts can get separated.
        var testString = "mmmmmmmmmmlli";

        // We test using 72px font size (but we may use any size) to amplify differences in fonts.
        var testFontSize = 72;

        var body = document.body || document.getElementsByTagName("body")[0];

        // We create a DIV in the document to get the width of the text we use to test
        // and we position it off-screen
        var div = document.createElement("DIV");
        div.style.fontSize = testFontSize.toString() + "px";
        div.style.position = "absolute";
        div.style.top = "-" + (testFontSize * 2).toString() + "px";
        div.innerHTML = testString;

        var defaultWidth = {},
            defaultHeight = {};

        // We store the baseline values for the default fonts
        baseFonts.forEach(function (item) {
            div.style.fontFamily = item;
            body.appendChild(div);
            defaultWidth[item] = div.offsetWidth; // width for the default font
            defaultHeight[item] = div.offsetHeight; // height for the default font
            body.removeChild(div); // HTML cleanup
        });

        // Now we try to render with the webfont, with fallback to the same default fonts.
        // If any of the measurements is different, it means that the webfont was rendered.
        var isDifferent = false;
        baseFonts.forEach(function (item) {
            div.style.fontFamily = "'" + fontFace + "'," + item; // webfont along with the base font for fallback
            body.appendChild(div);
            // At least one of the dimension should be different if the webfont has rendered.
            var notMatchingDimensions = (div.offsetWidth !== defaultWidth[item]) || (div.offsetHeight !== defaultHeight[item]);
            body.removeChild(div); // HTML cleanup

            isDifferent = isDifferent || notMatchingDimensions;
        });

        return isDifferent;
    };
    return executionEnvironmentDetector;
}());

var appBootstrap = (function () {
    appBootstrap.prototype.executionEnvironment = {};
    appBootstrap.prototype.isPageHidden = false;

    function appBootstrap() {
        this.executionEnvironment = new executionEnvironmentDetector();
    }

    appBootstrap.prototype.isSupportedVersion = function () {
        if (this.executionEnvironment.isSupportedVersion()) {
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
    };

    // if touch device, we set the min-height to the screen height resolution so that the user can swipe up and
    // remove the browser chrome, therefore maximizing the available space. Recalculates on orientation change.
    appBootstrap.prototype.orientationHandler = function () {
        if (this.executionEnvironment.isTouchDevice()) {
            document.body.style.minHeight = (this.executionEnvironment.isiOS() && this.executionEnvironment.isLandscape() ? screen.width : screen.height) + "px";
            if (this.executionEnvironment.isLandscape()) {
                document.body.classList.remove("is-portrait");
                document.body.classList.add("is-landscape");
            } else {
                document.body.classList.remove("is-landscape");
                document.body.classList.add("is-portrait");
            }
        }
    };

    appBootstrap.prototype.setupBodyClasses = function () {
        var self = this;

        if (self.executionEnvironment.isTouchDevice()) {
            document.body.className += " is-touch";
            self.orientationHandler();

            window.addEventListener("orientationchange", function () {
                self.orientationHandler();
            });
        }
        if (self.executionEnvironment.isAndroid()) {
            document.body.className += " is-android";
        }
        if (self.executionEnvironment.isiOS()) {
            document.body.className += " is-ios";
        }
        if (self.executionEnvironment.isWindows()) {
            document.body.className += " is-windows";
        }
        if (self.executionEnvironment.isMacOSX()) {
            document.body.className += " is-macosx";
        }
        if (self.executionEnvironment.isChrome()) {
            document.body.className += " is-chrome";
        }
        if (self.executionEnvironment.isIE()) {
            document.body.className += " is-msie";
        }
        if (self.executionEnvironment.isSafari()) {
            document.body.className += " is-safari";
        }
    };

    appBootstrap.prototype.setupPageVisibility = function (evt) {
        var visible = false, hidden = true;
        var eventMap = {
            focus: visible,
            focusin: visible,
            pageshow: visible,
            blur: hidden,
            focusout: hidden,
            pagehide: hidden
        };

        evt = evt || window.event;
        if (evt.type in eventMap) {
            this.isPageHidden = eventMap[evt.type];
        } else {
            this.isPageHidden = document[documentHiddenProperty];
        }
        document.body.classList.remove(this.isPageHidden ? "is-visible" : "is-hidden");
        document.body.classList.add(this.isPageHidden ? "is-hidden" : "is-visible");
    };

    appBootstrap.prototype.checkWebFont = function () {
        var self = this;

        if (// test for web font support only if the page is actually visible
        (!self.isPageHidden && !self.executionEnvironment.isWebfontAvailable("Blueprint Webfont Test"))
        || !self.executionEnvironment.isFontFaceSupported()
        ) {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    var divUnsupportedNoFont = document.createElement("div");
                    divUnsupportedNoFont.id = "unsupported-nofont-container";
                    divUnsupportedNoFont.innerHTML = xhr.responseText;

                    for (var n in document.body.children) {
                        var node = document.body.children[n];
                        if (node.nodeType === 1 && node.tagName.toUpperCase() !== "SCRIPT") { //ELEMENT_NODE
                            document.body.removeChild(node);
                        }
                    }
                    document.body.appendChild(divUnsupportedNoFont);
                }
            };
            xhr.open('GET', '/novaweb/static/unsupported-nofont.html');
            xhr.send();
        }

        var webfontTester = document.getElementById("webfont-tester");
        if (webfontTester) {
            webfontTester.parentNode.removeChild(webfontTester);
        }
    };

    appBootstrap.prototype.initApp = function () {
        var self = this;

        // Make use of the Page Visibility API https://www.w3.org/TR/page-visibility/
        var documentHiddenProperty = "hidden";
        // set the initial state but only if browser supports the Page Visibility API
        if (document[documentHiddenProperty] !== undefined) {
            self.setupPageVisibility({type: document[documentHiddenProperty] ? "blur" : "focus"});
        }

        self.setupBodyClasses();

        setTimeout(function () {
            self.checkWebFont();
        }, 1000);

        var app = angular.module("app", ["app.main"]);
        angular.bootstrap(document, ["app"], {
            strictDi: true
        });
    };

    return new appBootstrap();
})();
