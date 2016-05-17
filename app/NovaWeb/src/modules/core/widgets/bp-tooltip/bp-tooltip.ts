export class BPTooltip implements ng.IDirective {
    public link: ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => void;
    //public template = '<div>{{name}}</div>';
    public scope = {
        bpTooltip: "@"
    };
    public restrict = "A";

    private tooltipContent: string;

    constructor(
        //list of other dependencies*/
    ) {
        this.tooltipContent = "";

        // It's important to add `link` to the prototype or you will end up with state issues.
        // See http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/#comment-2111298002 for more information.
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

            var realLink = function() {
                if ($element && $element.length) {
                    var elem = $element[0];
                    elem.removeAttribute("bp-tooltip");
                    if (self.tooltipContent) {
                        let tooltip = document.createElement("DIV");
                        tooltip.className = "bp-tooltip";

                        let tooltipContent = document.createElement("DIV");
                        tooltipContent.className = "bp-tooltip-content";
                        tooltipContent.innerHTML = self.tooltipContent;

                        tooltip.appendChild(tooltipContent);

                        elem.className += " bp-tooltip-trigger";
                        elem.appendChild(tooltip);

                        elem.addEventListener("mousemove", function fn(e) {
                            let tooltip = <HTMLElement>elem.querySelectorAll(".bp-tooltip")[0];
                            if (e.clientX > document.body.clientWidth / 2) {
                                tooltip.style.left = "";
                                tooltip.style.right = (document.body.clientWidth - e.clientX - 15) + "px";
                                removeClass(tooltip, "bp-tooltip-left-tip");
                                addClass(tooltip, "bp-tooltip-right-tip");
                            } else {
                                tooltip.style.right = "";
                                tooltip.style.left = (e.clientX - 8) + "px";
                                removeClass(tooltip, "bp-tooltip-right-tip");
                                addClass(tooltip, "bp-tooltip-left-tip");
                            }
                            if (e.clientY > document.body.clientHeight / 2) {
                                tooltip.style.top = "";
                                tooltip.style.bottom = (document.body.clientHeight - (e.clientY - 15)) + "px";
                                removeClass(tooltip, "bp-tooltip-top-tip");
                                addClass(tooltip, "bp-tooltip-bottom-tip");
                            } else {
                                tooltip.style.bottom = "";
                                tooltip.style.top = e.clientY + 26 + "px";
                                removeClass(tooltip, "bp-tooltip-bottom-tip");
                                addClass(tooltip, "bp-tooltip-top-tip");
                            }
                        });
                    }
                }
            };

            realLink();
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