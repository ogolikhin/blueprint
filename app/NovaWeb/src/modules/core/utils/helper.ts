export class Helper {
//    static Guid(): string {
//        function s4(): string {
//            return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
//        }
//        return s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4();
//    }

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

    static hasCssClass = (element: Element, className: string): boolean => {
        if (element.classList) {
            return element.classList.contains(className);
        } else {
            return !!element.className.match(new RegExp("(\\s|^)" + className + "(\\s|$)"));
        }
    };

    static addCssClass = (element: Element, className: string) => {
        if (element.classList) {
            element.classList.add(className);
        } else if (!Helper.hasCssClass(element, className)) {
            element.className += " " + className;
        }
    };

    static removeCssClass = (element: Element, className: string) => {
        if (element.classList) {
            element.classList.remove(className);
        } else if (Helper.hasCssClass(element, className)) {
            var reg = new RegExp("(\\s|^)" + className + "(\\s|$)");
            element.className = element.className.replace(reg, " ");
        }
    };

    static findAncestorByCssClass = (element: Element, className: string): any => {
        while ((element = element.parentElement) && !element.classList.contains(className)) {
        }
        return element;
    };
}
