import {INotificationService} from "../notification";
export class Helper {

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
}

