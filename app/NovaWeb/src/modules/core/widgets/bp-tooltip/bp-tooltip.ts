import {Helper} from "../../utils/helper";

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
                if (Helper.hasCssClass(document.body, "is-touch")) {
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
                            Helper.removeCssClass(tooltip, "bp-tooltip-left-tip");
                            Helper.addCssClass(tooltip, "bp-tooltip-right-tip");
                        } else {
                            tooltip.style.right = "";
                            tooltip.style.left = (e.clientX - 15) + "px";
                            Helper.removeCssClass(tooltip, "bp-tooltip-right-tip");
                            Helper.addCssClass(tooltip, "bp-tooltip-left-tip");
                        }
                        //if (e.clientY > document.body.clientHeight / 2) {
                        if (e.clientY > 80) {
                            tooltip.style.top = "";
                            tooltip.style.bottom = (document.body.clientHeight - (e.clientY - 20)) + "px";
                            Helper.removeCssClass(tooltip, "bp-tooltip-top-tip");
                            Helper.addCssClass(tooltip, "bp-tooltip-bottom-tip");
                        } else {
                            tooltip.style.bottom = "";
                            tooltip.style.top = e.clientY + 30 + "px";
                            Helper.removeCssClass(tooltip, "bp-tooltip-bottom-tip");
                            Helper.addCssClass(tooltip, "bp-tooltip-top-tip");
                        }
                    });

                    elem.addEventListener("mouseover", function fn(e) {
                        Helper.addCssClass(tooltip, "show");
                    });

                    elem.addEventListener("mouseout", function fn(e) {
                        Helper.removeCssClass(tooltip, "show");
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