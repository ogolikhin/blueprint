import "bowser";

declare var bowser: any;

export interface IExecutionEnvironmentDetectorService {
    getBrowserInfo();
    getBrowserInfoUserAgent(ua: string, detected: any);
    isSupportedVersion();
}

export interface IBrowserInfo extends BowserModule.IBowserUA {
    ua: string;
    blueprintSupportedBrowser: boolean;
    win7plus: boolean;
    osx: boolean;
    osMajorVersion: number;
}

// Typescript implementation based on https://github.com/ded/bowser
export class ExecutionEnvironmentDetector implements IExecutionEnvironmentDetectorService {

    public static bowser: IBrowserInfo;

    public getBrowserInfo() {
        var ua = window.navigator !== undefined ? window.navigator.userAgent : "";
        var browser = window["bowser"] || bowser;

        return this.getBrowserInfoUserAgent(ua, browser);
    }


    // Method to return osversion, browser and version - please see https://github.com/ded/bowser/blob/master/src/useragents.js
    public getBrowserInfoUserAgent(ua: string, browser: any): IBrowserInfo {
        // Bowser considers any user agent with "tablet" in it to be a tablet, including "Tablet PC 2.0"
        if (/tablet pc/i.test(ua)) {
            browser.tablet = false;
        }

        if (browser.osversion !== undefined)
            browser.osMajorVersion = parseInt(browser.osversion.split(".")[0], 10);

        if (browser.ios) {
            if (/safari/i.test(ua)) {
                browser.safari = true;
            }
        }

        if (/Mac\sOS\sX/i.test(ua)) {
            browser.osx = true;
        } else if (
            /Windows\sNT\s6.1/i.test(ua) ||
            /Windows\sNT\s6.2/i.test(ua) ||
            /Windows\sNT\s6.3/i.test(ua)) {
            browser.win7plus = true;
        }
        browser.ua = ua;

        //true if (IE9 + /FF19+/Chrome26 + on Windows 7+, Safari 7+ on OSX, Safari6 + /Chrome35+ on iOS 6+, Chrome35+ on Android 4+)
        browser.blueprintSupportedBrowser = false;
        if (bowser.mobile) {
            bowser.blueprintSupportedBrowser = false;
        } else if (browser.msie && browser.version >= 9 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        } else if (browser.firefox && browser.version >= 19 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        } else if (browser.chrome && browser.version >= 26 && browser.win7plus) {
            browser.blueprintSupportedBrowser = true;
        } else if (browser.safari && browser.version >= 7 && browser.osx) {
            browser.blueprintSupportedBrowser = true;
        } else if (browser.chrome && browser.version >= 35 && browser.ios && browser.osMajorVersion >= 6) {
            browser.blueprintSupportedBrowser = true;
        } else if (browser.safari && browser.version >= 6 && browser.ios && browser.osMajorVersion >= 6) {
            browser.blueprintSupportedBrowser = true;
        } else if (browser.chrome && browser.version >= 35 && browser.osMajorVersion >= 4) {
            browser.blueprintSupportedBrowser = true;
        } /*else if (browser.android && browser.osMajorVersion >= 4) { //?
                browser.blueprintSupportedBrowser = true;
            } */
        return browser;
    }


    // Method to return true if (IE9 + /FF19+/Chrome26 + on Windows 7+, Safari 7+ on OSX, Safari6 + /Chrome35+ on iOS 6+, Chrome35+ on Android 4+)
    public isSupportedVersion(): boolean {
        return this.getBrowserInfo().blueprintSupportedBrowser;
    }
}
