﻿import * as moment from "moment";

export class Helper {
    static get UID(): string {        
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            /* tslint:disable:no-bitwise */ 
            var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
            /* tslint:enable:no-bitwise */
            return v.toString(16);
        });
    }

    static toDashCase(token: string): string {
        token = token.replace(/(\B[A-Z][a-z]+)/g, function(match) {
            return "-" + match.toLowerCase();
        });
        return token.toLowerCase();
    };

    static toCamelCase(token: string): string {
        token = token.replace(/[\-_\s]+(.)?/g, function(match, chr) {
            return chr ? chr.toUpperCase() : "";
        });
        // Ensure 1st char is always lowercase
        return token.replace(/^([A-Z])/, function(match, chr) {
            return chr ? chr.toLowerCase() : "";
        });
    };

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

    static toLocaleNumber(number: number, locale?: string): string {
        if (number === null || typeof number === "undefined" || isNaN(number)) {
            return null;
        }
        
        let numberAsString: string = number.toString();

        if (number - Math.round(number) !== 0) {
            let decimalSeparator = this.getDecimalSeparator(locale);

            if (decimalSeparator !== ".") {
                numberAsString = numberAsString.replace(".", decimalSeparator);
            }
        }

        return numberAsString;
    };

    static parseLocaleNumber(numberAsAny: any, locale?: string): number {
        let number: string;
        let decimalSeparator = this.getDecimalSeparator(locale);
        let thousandSeparator = decimalSeparator === "." ? "," : ".";

        number = (numberAsAny || "").toString();
        number = number.replace(thousandSeparator, "");
        if (decimalSeparator !== ".") {
            number = number.replace(decimalSeparator, ".");
        }

        return parseFloat(number);
    };

    static getDecimalSeparator(locale?: string): string {
        let separator = ".";
        let locale_ = locale || this.getFirstBrowserLanguage();
        if (Number.toLocaleString) {
            separator = (1.1).toLocaleString(locale_).replace(/\d/g, "");
        }

        return separator;
    };

    static uiDatePickerFormatAdaptor(format: string): string  {
        let adapted = format;
        //adapted = adapted.replace(/[^DMY/.-]/gi, "");
        adapted = adapted.replace(/[\u200F]/g, ""); //special case for RTL languages
        adapted = adapted.replace(/D/g, "d").replace(/Y/g, "y");

        if (adapted.length === adapted.replace(/[^dMy]/g, "").length) {
            adapted = adapted.match(/(d{1,4}|M{1,4}|y{1,4})/g).join(" ");
        }

        return adapted;
    };

    static toStartOfTZDay(date: Date): Date  {
        let momentDate = moment(date);

        if (!momentDate.isValid()) {
            return null;
        }

        let momentString = momentDate.utc().startOf("day").format("YYYY-MM-DD");

        return moment(momentString).toDate();
    };

    static autoLinkURLText(node: Node) {
        /* tslint:disable */
        const autoLinkPattern: RegExp = /(?:(?:ht|f)tp(?:s?)\:\/\/|~\/|\/)?(?:\w+:\w+@)?((?:(?:[-\w\d{1-3}]+\.)+(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|edu|co\.uk|ac\.uk|it|fr|tv|museum|asia|local|travel|[a-z]{2}))|((\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)(\.(\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)){3}))(?::[\d]{1,5})?(?:(?:(?:\/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|\/)+|\?|#)?(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?:#(?:[-\w~!$ |\/.,*:;=]|%[a-f\d]{2})*)?/gi;
        /* tslint:enable */

        // if it's already an A tag we exit
        if (node.nodeType === 1 && node.nodeName.toUpperCase() === "A") {
            return;
        }

        // if it doesn't contain a URL in the text, we exit
        if (!autoLinkPattern.test(node.textContent)) {
            return;
        }

        // if it has children, we go deeper
        if (node.hasChildNodes()) {
            [].forEach.call(node.childNodes, function(child) {
                if (child.nodeType === 1) { // we dig into HTML children only
                    Helper.autoLinkURLText(child);
                } else if (child.nodeType === 3) {
                    let nodeText: string = child.textContent;
                    let urls = nodeText.match(autoLinkPattern);
                    if (urls) {
                        urls.forEach((url) => {
                            nodeText = nodeText.replace(url, `<a href="${url}" target="_blank">${url}</a>`);
                        });
                        let span = document.createElement("span");
                        span.innerHTML = nodeText;
                        child.parentNode.replaceChild(span, child);
                    }
                }
            });
        }

        //if (processURLText) {
        /*
            let nodeText: string = node.textContent;
            nodeText.match(autoLinkPattern).forEach((url) => {
                nodeText = nodeText.replace(url, `<a href="${url}" target="_blank">${url}</a>`);
            });
            (<HTMLElement>node).innerHTML = nodeText;
        */
        //}
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

