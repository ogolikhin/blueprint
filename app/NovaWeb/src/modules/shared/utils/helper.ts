import { Models} from "../../main";
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

    static stripWingdings(htmlText: string): string {
        let _htmlText = htmlText || "";
        let wingdingsRegEx = /font-family:[ ]?['"]?Wingdings['"]?/gi;
        return _htmlText.replace(wingdingsRegEx, "");
    };

    static autoLinkURLText(node: Node) {
        /* tslint:disable */
        const urlPattern: RegExp = /(?:(?:ht|f)tp(?:s?)\:\/\/|~\/|\/)?(?:\w+:\w+@)?((?:(?:[-\w\d{1-3}]+\.)+(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|edu|co\.uk|ac\.uk|it|fr|tv|museum|asia|local|travel|[a-z]{2}))|((\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)(\.(\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)){3}))(?::[\d]{1,5})?(?:(?:(?:\/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|\/)+|\?|#)?(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?:#(?:[-\w~!$ |\/.,*:;=]|%[a-f\d]{2})*)?/gi;
        const protocolPattern: RegExp = /((?:ht|f)tp(?:s?)\:\/\/)/;
        /* tslint:enable */

        // if it's already an A tag we exit
        if (node.nodeType === 1 && node.nodeName.toUpperCase() === "A") {
            return;
        }

        // if it doesn't contain a URL in the text, we exit
        if (!urlPattern.test(node.textContent)) {
            return;
        }

        // if it has children, we go deeper
        if (node.hasChildNodes()) {
            [].forEach.call(node.childNodes, function(child) {
                if (child.nodeType === 1) { // we dig into HTML children only
                    Helper.autoLinkURLText(child);
                } else if (child.nodeType === 3) {
                    let nodeText: string = child.textContent;
                    let urls = nodeText.match(urlPattern);
                    if (urls) {
                        urls.forEach((url) => {
                            let defaultProtocol = "";
                            if (!protocolPattern.test(url)) {
                                defaultProtocol = "http://";
                            }

                            nodeText = nodeText.replace(url, `<a href="${defaultProtocol + url}" target="_blank">${url}</a>`);
                        });
                        let span = document.createElement("span");
                        span.innerHTML = nodeText;
                        child.parentNode.replaceChild(span, child);
                    }
                }
            });
        }
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

    public static canUtilityPanelUseSelectedArtifact(artifact: Models.IArtifact): boolean {
        return artifact &&
            artifact.prefix &&
            ["ACO", "_CFL", "PR"].indexOf(artifact.prefix) === -1;
    }
}

