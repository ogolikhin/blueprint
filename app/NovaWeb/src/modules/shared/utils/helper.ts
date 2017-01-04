﻿import {Models} from "../../main";
import {ItemTypePredefined} from "../../main/models/enums";

export class Helper {
    static draftVersion = 2147483647;
    static maxAttachmentFilesizeDefault = 10485760; // 10MB

    static get ELLIPSIS_SYMBOL() {
         return String.fromCharCode(8230);
    }

    static get UID(): string {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0;
            const v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    static limitChars(str: string, limit: number = 100): string {
        if (str) {
            if (str.length > limit) {
                return str.substring(0, limit - 1) + Helper.ELLIPSIS_SYMBOL;
            }
            return str;
        }
        return "";
    }

    static escapeQuot = (stringToEscape: string): string => {
        if (stringToEscape) {
            return stringToEscape.replace(/"/g, "&quot;");
        }

        return "";
    };

    static stripHTMLTags = (stringToSanitize: string): string => {
        const stringSanitizer = window.document.createElement("DIV");
        stringSanitizer.innerHTML = stringToSanitize;
        return stringSanitizer.textContent || stringSanitizer.innerText || "";
    };

    static escapeHTMLText = (stringToEscape: string): string => {
        const stringEscaper = window.document.createElement("TEXTAREA");
        stringEscaper.textContent = stringToEscape;
        return stringEscaper.innerHTML;
    };

    static stripWingdings(htmlText: string): string {
        let _htmlText = htmlText || "";
        let wingdingsRegEx = /font-family:[ ]?['"]?Wingdings['"]?/gi;
        return _htmlText.replace(wingdingsRegEx, "");
    };

    static autoLinkURLText(node: Node) {
        /* tslint:disable:max-line-length */
        // const urlPattern: RegExp = /(?:(?:ht|f)tp(?:s?)\:\/\/|~\/|\/)?(?:\w+:\w+@)?((?:(?:[-\w\d{1-3}]+\.)+(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|edu|co\.uk|ac\.uk|it|fr|tv|museum|asia|local|travel|[a-z]{2}))|((\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)(\.(\b25[0-5]\b|\b[2][0-4][0-9]\b|\b[0-1]?[0-9]?[0-9]\b)){3}))(?::[\d]{1,5})?(?:(?:(?:\/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|\/)+|\?|#)?(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?:#(?:[-\w~!$ |\/.,*:;=]|%[a-f\d]{2})*)?/gi;
        const urlPattern: RegExp = /((www\.)|(https?|ftp):\/\/(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}|([a-zA-Z][-a-zA-Z0-9@:%_\+~#=]{2,256}(\.[a-z]{2,6})?)))(:\d{2,5})?\b([-a-zA-Z0-9@:%_\+.~#?&\/\/=]*)/gi;
        const protocolPattern: RegExp = /((?:ht|f)tp(?:s?)\:\/\/)/;
        /* tslint:enable:max-line-length */

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

    static hasNonTextTags(htmlText: string): boolean {
        const nonTextTags = new RegExp(/<(img|table)>?/gi);
        return nonTextTags.test(htmlText || "");
    }

    static tagsContainText(htmlText: string): boolean {
        const div = document.createElement("div");
        div.innerHTML = (htmlText || "").toString();
        let content = div.innerText.trim();
        content = content.replace(/\s/gi, ""); // remove any "spacing" characters
        content = content.replace(/[^\x00-\x7F]/gi, ""); // remove non ASCII characters
        return content !== "";
    }

    static stripTinyMceBogusChars(html: string): string {
        const bogusRegEx = /<br data-mce-bogus="1">/gi;
        const zeroWidthNoBreakSpaceRegEx = /[\ufeff\u200b]/g;

        let _html = html || "";
        _html = _html.replace(bogusRegEx, "");
        _html = _html.replace(zeroWidthNoBreakSpaceRegEx, "");

        return _html;
    }

    static getHtmlBodyContent(html: string): string {
        // this method is for cleaning extra tags added by SilverLight on Rich Text Areas
        let content = html || "";
        content = content.replace(/<span class="mceNonEditable">(.*)<\/span>/gi, "$1");
        content = content.replace(/mceNonEditable/gi, "");
        content = content.replace(/(<a [^>]*linkassemblyqualifiedname[^>]*>.*<\/a>)/gi, `<span class="mceNonEditable">$1</span>`);

        const div = document.createElement("div");
        div.innerHTML = content;
        return div.innerHTML;
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

    public static removeAttributeFromNode(node: Node, attribute: string) {
        let result: string;
        const walk_the_Node = function walk(node, func) {
            func(node);
            node = node.firstChild;
            while (node) {
                walk(node, func);
                node = node.nextSibling;
            }
        };
        walk_the_Node(node, function (element) {
            if (element.removeAttribute && element.hasAttribute(attribute)) {
                element.removeAttribute(attribute);
            }
        });
    };

    public static stripHtmlTags(content: string, tags: string[]): string {
        const ngContent = angular.element(content);
        tags.forEach(tag => {
            ngContent.find(tag).remove();
        });
        const div = document.createElement("div");
        div.appendChild(ngContent[0]);

        return div.innerHTML;
    }

    public static stripExternalImages(content: HTMLElement) {
        const externalUrl = new RegExp("^(?:[a-z]+:)?\/\/", "i");
        const ngContent = angular.element(content);

        const images = ngContent.find("img");
        angular.forEach(images, image => {
            const url = image.getAttribute("src");
            if (externalUrl.test(url)) {
                angular.element(image).remove();
            }
        });
    }
}
