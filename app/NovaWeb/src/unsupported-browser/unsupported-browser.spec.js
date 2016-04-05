// See List of User Agent Strings - http://www.useragentstring.com/pages/useragentstring.php
describe('executionEnvironmentDetector', function() {

    it('Browser info is defined', function() {
        // Arrange
        var detector = new executionEnvironmentDetector();

        // Act
        var browser = detector.getBrowserInfo();

        // Assert
        expect(browser).toBeTruthy();
    });

    it("Android 2.3", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Linux; U; Android 2.3.5; en-us; HTC Vision Build/GRI40) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.android).toBeTruthy();
        expect(browserInfo.osversion).toEqual("2.3.5");
        expect(browserInfo.osMajorVersion).toEqual(2);
        expect(browserInfo.mobile).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.webkit).toBeTruthy();
        expect(browserInfo.name).toEqual("Android");
        expect(browserInfo.version).toEqual("4.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Android 4.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Linux; U; Android 4.0.3; ko-kr; LG-L160L Build/IML74K) AppleWebkit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.android).toBeTruthy();
        expect(browserInfo.osversion).toEqual("4.0.3");
        expect(browserInfo.osMajorVersion).toEqual(4);
        expect(browserInfo.mobile).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.webkit).toBeTruthy();
        expect(browserInfo.name).toEqual("Android");
        expect(browserInfo.version).toEqual("4.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Android 4.2", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Linux; U; Android 4.2.2; en-ca; GT-P5113 Build/JDQ39) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Safari/534.30";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.android).toBeTruthy();
        expect(browserInfo.osversion).toEqual("4.2.2");
        expect(browserInfo.osMajorVersion).toEqual(4);
        expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.webkit).toBeTruthy();
        expect(browserInfo.name).toEqual("Android");
        expect(browserInfo.android).toBeTruthy();

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Android 4.2 - Chrome 40", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Linux; Android 4.2.2; GT-P5113 Build/JDQ39) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.109 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.android).toBeTruthy();
        expect(browserInfo.osversion).toEqual("4.2.2");
        expect(browserInfo.osMajorVersion).toEqual(4);
        expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("40.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Android 4.4 - Galaxy Tab", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Linux; Android 4.4.2; en-ca; SAMSUNG SM-T330NU Build/KOT49H) AppleWebKit/537.36 (KHTML, like Gecko) Version/1.5 Chrome/28.0.1500.94 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.android).toBeTruthy();
        expect(browserInfo.osversion).toEqual("4.4.2");
        expect(browserInfo.osMajorVersion).toEqual(4);
        expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("28.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Android 4.4 - Appium", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Linux; Android 4.4.2; Android SDK built for x86 Build/KK) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.android).toBeTruthy();
        expect(browserInfo.osversion).toEqual("4.4.2");
        expect(browserInfo.osMajorVersion).toEqual(4);
        //expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("30.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Android 5.0 - Appium", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Linux; Android 5.0; Android SDK built for x86_64 Build/LRX09D) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/37.0.0.0 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.android).toBeTruthy();
        expect(browserInfo.osversion).toEqual("5.0");
        expect(browserInfo.osMajorVersion).toEqual(5);
        //expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("37.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("iOS 5.1", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (iPad; CPU OS 5_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko ) Version/5.1 Mobile/9B176 Safari/7534.48.3";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.ios).toBeTruthy();
        expect(browserInfo.osversion).toEqual("5.1");
        expect(browserInfo.osMajorVersion).toEqual(5);
        expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.safari).toBeTruthy();
        expect(browserInfo.name).toEqual("iPad");
        expect(browserInfo.version).toEqual("5.1");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("iOS 6", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.ios).toBeTruthy();
        expect(browserInfo.osversion).toEqual("6.0");
        expect(browserInfo.osMajorVersion).toEqual(6);
        expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.safari).toBeTruthy();
        expect(browserInfo.name).toEqual("iPad");
        expect(browserInfo.version).toEqual("6.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("iOS 7 - Chrome Mobile iOS 40", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (iPad; CPU OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) CriOS/40.0.2214.69 Mobile/11D257 Safari/9537.53";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.ios).toBeTruthy();
        expect(browserInfo.osversion).toEqual("7.1.2");
        expect(browserInfo.osMajorVersion).toEqual(7);
        expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("40.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("iOS 7 - Mobile Safari 7.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (iPad; CPU OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.ios).toBeTruthy();
        expect(browserInfo.osversion).toEqual("7.1.2");
        expect(browserInfo.osMajorVersion).toEqual(7);
        expect(browserInfo.tablet).toBeTruthy();

        // Assert - Browser
        expect(browserInfo.safari).toBeTruthy();
        expect(browserInfo.name).toEqual("iPad");
        expect(browserInfo.ios).toBeTruthy();

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Linux - Firefox 31.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (X11; Linux i586; rv:31.0) Gecko/20100101 Firefox/31.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("31.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Linux - Chrome 41", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.0 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("41.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Mac OS X 10.9 - Safari 7.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.3 Safari/7046A194A";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.osx).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.safari).toBeTruthy();
        expect(browserInfo.name).toEqual("Safari");
        expect(browserInfo.version).toEqual("7.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Mac OS X 10.10 - Chrome 41", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.osx).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("41.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Mac OS X 10.10 - Firefox 33.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10; rv:33.0) Gecko/20100101 Firefox/33.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.osx).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("33.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows XP - Chrome 41", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2224.3 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeFalsy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("41.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows XP - Firefox 21", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 5.1; rv:21.0) Gecko/20130401 Firefox/21.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeFalsy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("21.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows XP - Internet Explorer 8.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 1.1.4322)";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeFalsy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.msie).toBeTruthy();
        expect(browserInfo.name).toEqual("Internet Explorer");
        expect(browserInfo.version).toEqual("8.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows Vista - Chrome 23", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.0) yi; AppleWebKit/345667.12221 (KHTML, like Gecko) Chrome/23.0.1271.26 Safari/453667.1221";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeFalsy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("23.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows Vista - Firefox 24", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeFalsy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("24.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows Vista - Internet Explorer 9.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0; chromeframe/11.0.696.57)";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeFalsy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.msie).toBeTruthy();
        expect(browserInfo.name).toEqual("Internet Explorer");
        expect(browserInfo.version).toEqual("9.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows 7 - Chrome 24", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.60 Safari/537.17";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("24.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows 7 - Chrome 40", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("40.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows 7 - Firefox 18", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:18.0)  Gecko/20100101 Firefox/18.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("18.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows 7 - Firefox 35", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("35.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows 7 - Internet Explorer 8.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; GTB7.4; InfoPath.2; SV1; .NET CLR 3.3.69573; WOW64; en-US)";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.msie).toBeTruthy();
        expect(browserInfo.name).toEqual("Internet Explorer");
        expect(browserInfo.version).toEqual("8.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows 7 - Internet Explorer 11.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; InfoPath.3; rv:11.0) like Gecko";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.msie).toBeTruthy();
        expect(browserInfo.name).toEqual("Internet Explorer");
        expect(browserInfo.version).toEqual("11.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows 7 - Opera 12", function () {
        // Arrange
        var userAgent =
            "Opera/9.80 (Windows NT 6.1; U; es-ES) Presto/2.9.181 Version/12.00";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.opera).toBeTruthy();
        expect(browserInfo.name).toEqual("Opera");
        expect(browserInfo.version).toEqual("12.00");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows 7 - Safari 5.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.safari).toBeTruthy();
        expect(browserInfo.name).toEqual("Safari");
        expect(browserInfo.version).toEqual("5.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

    it("Windows 8 - Chrome 32", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("32.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows 8 - Firefox 27", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.2; Win64; x64; rv:27.0) Gecko/20121011 Firefox/27.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("27.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows 8.1 - Chrome 40", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.chrome).toBeTruthy();
        expect(browserInfo.name).toEqual("Chrome");
        expect(browserInfo.version).toEqual("40.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows 8.1 - Internet Explorer 11.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; Touch; .NET4.0E; .NET4.0C; .NET CLR 3.5.30729; .NET CLR 2.0.50727; .NET CLR 3.0.30729; InfoPath.3; Tablet PC 2.0; rv:11.0) like Gecko";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.msie).toBeTruthy();
        expect(browserInfo.name).toEqual("Internet Explorer");
        expect(browserInfo.version).toEqual("11.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows 8.1 - Firefox 35", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.win7plus).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        expect(browserInfo.firefox).toBeTruthy();
        expect(browserInfo.name).toEqual("Firefox");
        expect(browserInfo.version).toEqual("35.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeTruthy();
    });

    it("Windows Phone - IE Mobile 9.0", function () {
        // Arrange
        var userAgent =
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)";

        // Act
        var browserInfo = new executionEnvironmentDetector().getBrowserInfoUserAgent(userAgent, bowser._detect(userAgent));

        // Assert - OS
        expect(browserInfo.mobile).toBeTruthy();
        expect(browserInfo.tablet).toBeFalsy();

        // Assert - Browser
        expect(browserInfo.windowsphone).toBeTruthy();
        expect(browserInfo.name).toEqual("Windows Phone");
        expect(browserInfo.version).toEqual("9.0");

        expect(browserInfo.blueprintSupportedBrowser).toBeFalsy();
    });

});
