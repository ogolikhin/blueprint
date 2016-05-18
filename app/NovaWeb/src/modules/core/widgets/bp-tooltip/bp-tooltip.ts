export class BPTooltip implements ng.IDirective {
    public link: ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => void;
    //public template = '<div>{{name}}</div>';
    public scope = {
        bpTooltip: "@"
    };
    public restrict = "A";

    private tooltipContent: string = "";
    private attachToBody: boolean = false;

    constructor(
        //list of other dependencies*/
    ) {
        BPTooltip.prototype.link = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => {
            var self = this;
            self.tooltipContent = $attrs[self["name"]];

            function hasClass(el, className) {
                if (el.classList) {
                    return el.classList.contains(className);
                } else {
                    return !!el.className.match(new RegExp("(\\s|^)" + className + "(\\s|$)"));
                }
            }

            function addClass(el, className) {
                if (el.classList) {
                    el.classList.add(className);
                } else if (!hasClass(el, className)) {
                    el.className += " " + className;
                }
            }

            function removeClass(el, className) {
                if (el.classList) {
                    el.classList.remove(className);
                } else if (hasClass(el, className)) {
                    var reg = new RegExp("(\\s|^)" + className + "(\\s|$)");
                    el.className = el.className.replace(reg, " ");
                }
            }

            function zIndexedParent(el) {
                let node = el.parentElement;
                while (node) {
                    let appliedStyle = window.getComputedStyle(node, null);
                    if (
                        appliedStyle &&
                        appliedStyle.zIndex &&
                        appliedStyle.zIndex !== "" &&
                        appliedStyle.zIndex !== "0" && // may be removed
                        appliedStyle.zIndex !== "auto" &&
                        !isNaN(parseInt(appliedStyle.zIndex, 10))
                    ) {
                        return true;
                    }

                    node = node.parentElement;
                }
                return false;
            }

            if ($element && $element.length && self.tooltipContent) {
                if (hasClass(document.body, "is-touch")) {
                    //disabled for touch devices (for now)
                } else {
                    let elem = $element[0];
                    elem.removeAttribute("bp-tooltip");

                    self.attachToBody = zIndexedParent(elem);

                    let tooltip = document.createElement("DIV");
                    tooltip.className = "bp-tooltip";

                    let tooltipContent = document.createElement("DIV");
                    tooltipContent.className = "bp-tooltip-content";
                    tooltipContent.innerHTML = self.tooltipContent;

                    tooltip.appendChild(tooltipContent);

                    elem.className += " bp-tooltip-trigger";

                    if (self.attachToBody) {
                        document.body.appendChild(tooltip);
                    } else {
                        elem.appendChild(tooltip);
                    }

                    elem.addEventListener("mousemove", function fn(e) {
                        if (e.clientX > document.body.clientWidth / 2) {
                            tooltip.style.left = "";
                            tooltip.style.right = (document.body.clientWidth - e.clientX - 15) + "px";
                            removeClass(tooltip, "bp-tooltip-left-tip");
                            addClass(tooltip, "bp-tooltip-right-tip");
                        } else {
                            tooltip.style.right = "";
                            tooltip.style.left = (e.clientX - 15) + "px";
                            removeClass(tooltip, "bp-tooltip-right-tip");
                            addClass(tooltip, "bp-tooltip-left-tip");
                        }
                        //if (e.clientY > document.body.clientHeight / 2) {
                        if (e.clientY > 80) {
                            tooltip.style.top = "";
                            tooltip.style.bottom = (document.body.clientHeight - (e.clientY - 20)) + "px";
                            removeClass(tooltip, "bp-tooltip-top-tip");
                            addClass(tooltip, "bp-tooltip-bottom-tip");
                        } else {
                            tooltip.style.bottom = "";
                            tooltip.style.top = e.clientY + 30 + "px";
                            removeClass(tooltip, "bp-tooltip-bottom-tip");
                            addClass(tooltip, "bp-tooltip-top-tip");
                        }
                    });

                    elem.addEventListener("mouseover", function fn(e) {
                        addClass(tooltip, "show");
                    });

                    elem.addEventListener("mouseout", function fn(e) {
                        removeClass(tooltip, "show");
                    });
                }
            }
        };
    }

    public static Factory() {
        var directive = (
            //list of dependencies
        ) => {
            return new BPTooltip (
                //list of other dependencies
            );
        };

        directive["$inject"] = [
            //list of other dependencies
        ];

        return directive;
    }
}