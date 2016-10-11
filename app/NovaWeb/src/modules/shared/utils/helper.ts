import * as angular from "angular";
import {Models} from "../../main";
import {ItemTypePredefined} from "../../main/models/enums";

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
        token = token.replace(/(\B[A-Z][a-z]+)/g, function (match) {
            return "-" + match.toLowerCase();
        });
        return token.toLowerCase();
    };

    static toCamelCase(token: string): string {
        token = token.replace(/[\-_\s]+(.)?/g, function (match, chr) {
            return chr ? chr.toUpperCase() : "";
        });
        // Ensure 1st char is always lowercase
        return token.replace(/^([A-Z])/, function (match, chr) {
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

    static findAncestorByCssClass = (elem: Element, selector: string): Element => {
        let el = elem.parentElement;
        while (el && !el.classList.contains(selector)) {
            el = el.parentElement;
        }

        return el;
    };

    static stringifySafe = (obj, replacer?, spaces?, cycleReplacer?): any => {
        return JSON.stringify(obj, Helper.serializer(replacer, cycleReplacer), spaces);
    };

    /* tslint:disable */
    static serializer = (replacer, cycleReplacer): any => {
        var stack = [], keys = [];

        if (cycleReplacer == null) {
            cycleReplacer = function (key, value) {
                if (stack[0] === value) {
                    return "[Circular ~]";
                }
                return "[Circular ~." + keys.slice(0, stack.indexOf(value)).join(".") + "]";
            };
        }

        return function (key, value) {
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
        // const urlPattern: RegExp = /(?:(?:ht|f)tp(?:s?)\:\/\/|~\/|\/)?(?:\w+:\w+@)?((?:(?:[-\w\d{1-3}]+\.)+(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|edu|co\.uk|ac\.uk|it|fr|tv|museum|asia|local|travel|[a-z]{2}))|((\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)(\.(\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)){3}))(?::[\d]{1,5})?(?:(?:(?:\/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|\/)+|\?|#)?(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?:#(?:[-\w~!$ |\/.,*:;=]|%[a-f\d]{2})*)?/gi;
        const urlPattern: RegExp = /((www\.)|(https?|ftp):\/\/(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}|([a-zA-Z][-a-zA-Z0-9@:%_\+~#=]{2,256}(\.[a-z]{2,6})?)))(:\d{2,5})?\b([-a-zA-Z0-9@:%_\+.~#?&\/\/=]*)/gi;
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
            [].forEach.call(node.childNodes, function (child) {
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

    static addTableBorders(node: Node) {
        // if it's not an Element node we exit
        if (node.nodeType !== 1) {
            return;
        }

        let element = node as HTMLElement;
        let tds = element.querySelectorAll("td");
        [].forEach.call(tds, function (td) {
            if (td.style.borderStyle === "" || td.style.borderStyle.indexOf("none") !== -1) {
                td.style.borderStyle = "solid";
            }
            if (td.style.borderWidth === "" || td.style.borderWidth.match(/(\D0p?)|(^0p?)/gi)) {
                td.style.borderWidth = "1px";
            }
            if (td.style.borderColor === "" || td.style.borderColor === "transparent") {
                td.style.borderColor = "black";
            }
        });
    };

    static setFontFamilyOrOpenSans(node: Node, allowedFonts?: string[]) {
        // if it's not an Element node we exit
        if (node.nodeType !== 1) {
            return;
        }

        // if it has children, we go deeper
        if (node.hasChildNodes()) {
            [].forEach.call(node.childNodes, function (child) {
                if (child.nodeType === 1) { // we dig into HTML children only
                    Helper.setFontFamilyOrOpenSans(child, allowedFonts);
                } else if (child.nodeType === 3) {
                    let parent = child.parentNode;
                    if (parent && parent.nodeType === 1) {
                        parent = parent as HTMLElement;
                        let element = parent;
                        let fontFamily = element.style.fontFamily;
                        while (fontFamily === "" && element.parentElement) {
                            element = element.parentElement;
                            fontFamily = element.style.fontFamily;
                        }
                        if (fontFamily === "") {
                            fontFamily = "'Open Sans'";
                        } else if (allowedFonts && allowedFonts.length) {
                            let isFontAllowed = false;
                            allowedFonts.forEach(function (allowedFont) {
                                isFontAllowed = isFontAllowed || fontFamily.split(",").some(function (font) {
                                        return font.toLowerCase().trim().indexOf(allowedFont.toLowerCase()) !== -1;
                                    });
                            });
                            if (!isFontAllowed) {
                                fontFamily += ",'Open Sans'";
                            }
                        }
                        if (parent.tagName.toUpperCase() !== "SPAN") {
                            let span = document.createElement("SPAN");
                            span.style.fontFamily = fontFamily;
                            span.innerHTML = parent.innerHTML;
                            parent.innerHTML = "";
                            parent.appendChild(span);
                        } else {
                            parent.style.fontFamily = fontFamily;
                        }
                    }
                }
            });
        }
    };

    static tagsContainText(htmlText: string): boolean {
        let div = document.createElement("div");
        div.innerHTML = (htmlText || "").toString();
        let content = div.innerText.trim();
        content = content.replace(/\s/gi, ""); // remove any "spacing" characters
        content = content.replace(/[^\x00-\x7F]/gi, ""); // remove non ASCII characters
        return content !== "";
    }

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
        const nonStandardTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.ArtifactCollection,
            ItemTypePredefined.Collections,
            ItemTypePredefined.CollectionFolder
        ];

        return artifact && artifact.predefinedType != null && nonStandardTypes.indexOf(artifact.predefinedType) === -1;
    }

    public static hasArtifactEverBeenSavedOrPublished(artifact: Models.IArtifact): boolean {
        return artifact.id > 0;
    }

    public static isInt(n: number): boolean {
        return parseInt(n.toString(), 10) === n;
    }
}

