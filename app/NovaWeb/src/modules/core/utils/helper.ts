
export class Helper {
    static get UID(): string {        
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            /* tslint:disable:no-bitwise */ 
            var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
            /* tslint:enable:no-bitwise */
            return v.toString(16);
        });        
    }

    static stripHTMLTags = (stringToSanitize: string): string => {
        var stringSanitizer = window.document.createElement("DIV");
        stringSanitizer.innerHTML = stringToSanitize;
        return stringSanitizer.textContent || stringSanitizer.innerText || "";
    };

    static escapeHTMLText = (stringToEscape: string): string => {
        var stringEscaper = window.document.createElement("TEXTAREA");
        stringEscaper.textContent = stringToEscape;
        return stringEscaper.innerHTML;
    };

    static decodeHtmlText = (encodedText: string) : string => {
        var dummy = document.createElement("div");
        dummy.innerHTML = encodedText;   
        return dummy.innerText || dummy.textContent;
   };

    /* tslint:disable */
    static findAncestorByCssClass = (element: Element, className: string): any => {
        while ((element = element.parentElement) && !element.classList.contains(className)) {
        }
        return element;
    };

    static stringifySafe = (obj, replacer?, spaces?, cycleReplacer?): any => {
        return JSON.stringify(obj, Helper.serializer(replacer, cycleReplacer), spaces);
    };

    static serializer = (replacer, cycleReplacer): any => {
        var stack = [], keys = [];

        if (cycleReplacer == null) {
            cycleReplacer = function(key, value) {
                if (stack[0] === value) {
                    return "[Circular ~]";
                }
                return "[Circular ~." + keys.slice(0, stack.indexOf(value)).join(".") + "]";
            };
        }

        return function(key, value) {
            if (stack.length > 0) {
                var thisPos = stack.indexOf(this);
                ~thisPos ? stack.splice(thisPos + 1) : stack.push(this);
                ~thisPos ? keys.splice(thisPos, Infinity, key) : keys.push(key);
                if (~stack.indexOf(value)) {
                    value = cycleReplacer.call(this, key, value);
                }
            } else {
                stack.push(value);
            }

            return replacer == null ? value : replacer.call(this, key, value);
        };
    };
    /* tslint:enable */

    static isFontFaceSupported(): boolean {
        // Based on http://www.paulirish.com/2009/font-face-feature-detection/
        let
            rule = "@font-face { font-family: 'font'; src: 'font.ttf'; }",
            styleSheet,
            head = document.head || document.getElementsByTagName("head")[0] || document.documentElement,
            styleTag = document.createElement("style"),
            implementation = document.implementation || { hasFeature: function() { return false; } };

        styleTag.type = "text/css";
        head.insertBefore(styleTag, head.firstChild);
        styleSheet = styleTag.sheet || (<any> styleTag).styleSheet;

        if (!styleSheet) {
            return false;
        }

        if (implementation.hasFeature("CSS2", "")) {
            let isSupported = false;
            try {
                styleSheet.insertRule(rule, 0);
                isSupported = !(/unknown/i).test(styleSheet.cssRules[0].cssText);
                styleSheet.deleteRule(0);
            } catch(e) { }
            return isSupported;
        } else {
            styleSheet.cssText = rule;

            return styleSheet.cssText.length !== 0 && !(/unknown/i).test(styleSheet.cssText) &&
                styleSheet.cssText
                    .replace(/\r+|\n+/g, "")
                    .indexOf(rule.split(" ")[0]) === 0;
        }
    };

    static isWebfontAvailable(fontFace: string): boolean {
        // Based on http://www.lalit.org/lab/javascript-css-font-detect/

        // The font will be compared against all the three default fonts.
        // and if it doesn't match all 3 then that font is not available.
        let baseFonts = ["monospace", "sans-serif", "serif"];

        // We use "m" (or "w") because these two characters take up the maximum width.
        // And we use a "LLi" so that the same matching fonts can get separated.
        let testString = "mmmmmmmmmmlli";

        // We test using 72px font size (but we may use any size) to amplify differences in fonts.
        let testFontSize = 72;

        let body = document.body || document.getElementsByTagName("body")[0];

        // We create a DIV in the document to get the width of the text we use to test
        // and we position it off-screen
        let div = document.createElement("DIV");
        div.style.fontSize = testFontSize.toString() + "px";
        div.style.position = "absolute";
        div.style.top = "-" + (testFontSize * 2).toString() + "px";
        div.innerHTML = testString;

        let defaultWidth = {},
            defaultHeight = {};

        // We store the baseline values for the default fonts
        baseFonts.forEach(function(item) {
            div.style.fontFamily = item;
            body.appendChild(div);
            defaultWidth[item] = div.offsetWidth; // width for the default font
            defaultHeight[item] = div.offsetHeight; // height for the default font
            body.removeChild(div); // HTML cleanup
        });

        // Now we try to render with the webfont, with fallback to the same default fonts.
        // If any of the measurements is different, it means that the webfont was rendered.
        let isDifferent = false;
        baseFonts.forEach(function(item) {
            div.style.fontFamily = "'" + fontFace + "'," + item; // webfont along with the base font for fallback
            body.appendChild(div);
            // At least one of the dimension should be different if the webfont has rendered.
            let notMatchingDimensions = (div.offsetWidth !== defaultWidth[item]) || (div.offsetHeight !== defaultHeight[item]);
            body.removeChild(div); // HTML cleanup
            
            isDifferent = isDifferent || notMatchingDimensions;
        });

        return isDifferent;
    };

    static getFirstBrowserLanguage(): string {
        // The most reliable way of getting the user's preferred langauge would be to read the Accept-Languages request
        // header on the server. In Chrome 32+ and Firefox 32+ that header's value is available in navigator.languages
        // In the returned array the languages are ordered by preference with the most preferred language first (see:
        // https://developer.mozilla.org/en-US/docs/Web/API/NavigatorLanguage/languages
        // For other browsers:
        // - Internet Explorer:
        //   navigator.userLanguage is the language set in Windows Control Panel / Regional Options
        //   navigator.browserLanguage returns the language of the UI of the browser and it is decided by the version of
        //   the executable installed
        //   navigator.systemLanguage gives the locale used by Windows itself
        // - Safari: uses the language set at the system level (similar to navigator.systemLanguage of IE above)
        // The order of elements in browserLanguagePropertyKeys has been set based on the above information.
        let nav = window.navigator,
            browserLanguagePropertyKeys = ["userLanguage", "systemLanguage", "language", "browserLanguage"],
            language;

        // support for HTML 5.1 "navigator.languages"
        if (Array.isArray((<any> nav).languages)) {
            for (let i = 0; i < (<any> nav).languages.length; i++) {
                language = (<any >nav).languages[i];
                if (language && language.length) {
                    return language;
                }
            }
        }

        // support for other well known properties in browsers
        for (let i = 0; i < browserLanguagePropertyKeys.length; i++) {
            language = nav[browserLanguagePropertyKeys[i]];
            if (language && language.length) {
                return language;
            }
        }

        return null;
    };

    public static toFlat(root: any): any[] {
        var stack: any[] = angular.isArray(root) ? root.slice() : [root], array: any[] = [];
        while (stack.length !== 0) {
            var node = stack.shift();
            array.push(node);
            if (angular.isArray(node.children)) {

                for (var i = node.children.length - 1; i >= 0; i--) {
                    stack.push(node.children[i]);
                }
                node.children = null;
            }
        }

        return array;
    }
}

