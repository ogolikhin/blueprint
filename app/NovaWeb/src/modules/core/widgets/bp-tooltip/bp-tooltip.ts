export class BPTooltip implements ng.IDirective {
    //public template = '<div>{{name}}</div>';
    public scope = {
        bpTooltip: "@"
    };
    public restrict = "A";

    private tooltipContent: string = "";
    private attachToBody: boolean = false;

    public link: Function = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes):void => {
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
            if (angular.element(document.body).hasClass("is-touch")) {
                //disabled for touch devices (for now)
            } else {
                function mouseMove(e) {
                    if (e.clientX > document.body.clientWidth / 2) {
                        tooltip.style.left = "";
                        tooltip.style.right = (document.body.clientWidth - e.clientX - 15) + "px";
                        angular.element(tooltip).removeClass("bp-tooltip-left-tip");
                        angular.element(tooltip).addClass("bp-tooltip-right-tip");
                    } else {
                        tooltip.style.right = "";
                        tooltip.style.left = (e.clientX - 15) + "px";
                        angular.element(tooltip).removeClass("bp-tooltip-right-tip");
                        angular.element(tooltip).addClass("bp-tooltip-left-tip");
                    }
                    if (e.clientY > 80) { // put the tooltip at the bottom only within 80px from the top of the window
                        tooltip.style.top = "";
                        tooltip.style.bottom = (document.body.clientHeight - (e.clientY - 20)) + "px";
                        angular.element(tooltip).removeClass("bp-tooltip-top-tip");
                        angular.element(tooltip).addClass("bp-tooltip-bottom-tip");
                    } else {
                        tooltip.style.bottom = "";
                        tooltip.style.top = e.clientY + 30 + "px";
                        angular.element(tooltip).removeClass("bp-tooltip-bottom-tip");
                        angular.element(tooltip).addClass("bp-tooltip-top-tip");
                    }
                }

                function mouseOver(e) {
                    angular.element(tooltip).addClass("show");
                }

                function mouseOut(e) {
                    angular.element(tooltip).removeClass("show");
                }

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

                elem.addEventListener("mousemove", mouseMove);
                elem.addEventListener("mouseover", mouseOver);
                elem.addEventListener("mouseout", mouseOut);

                $scope.$on('$destroy', function () {
                    elem.removeEventListener("mousemove", mouseMove);
                    elem.removeEventListener("mouseover", mouseOver);
                    elem.removeEventListener("mouseout", mouseOut);
                    tooltip.remove();
                });
            }
        }
    };

    constructor(
        //list of other dependencies*/
    ) {}

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