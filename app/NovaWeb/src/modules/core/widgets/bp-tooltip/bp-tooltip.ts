export class BPTooltip implements ng.IDirective {
    public scope = {
        bpTooltip: "@"
    };
    public restrict = "A";

    private tooltipContent: string = "";
    private attachToBody: boolean = false;

    public link: Function = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes): void => {
        this.tooltipContent = $attrs["bpTooltip"];

        let tooltip = document.createElement("DIV");
        tooltip.className = "bp-tooltip";

        function zIndexedParent(el) {
            let node = el.parentElement;
            while (node) {
                let appliedStyle = window.getComputedStyle(node, null);
                if (
                    appliedStyle &&
                    appliedStyle.zIndex &&
                    appliedStyle.zIndex !== "" &&
                    appliedStyle.zIndex !== "0" && // may be removed
                    appliedStyle.zIndex !== "auto" && !isNaN(parseInt(appliedStyle.zIndex, 10))
                ) {
                    return true;
                }

                node = node.parentElement;
            }
            return false;
        }

        function updateTooltip(e) {
            if (e.clientX > document.body.clientWidth / 2) {
                tooltip.style.left = "";
                tooltip.style.right = (document.body.clientWidth - e.clientX - 15) + "px";
                angular.element(tooltip).removeClass("bp-tooltip-left-tip").addClass("bp-tooltip-right-tip");
            } else {
                tooltip.style.right = "";
                tooltip.style.left = (e.clientX - 15) + "px";
                angular.element(tooltip).removeClass("bp-tooltip-right-tip").addClass("bp-tooltip-left-tip");
            }
            if (e.clientY > 80) { // put the tooltip at the bottom only within 80px from the top of the window
                tooltip.style.top = "";
                tooltip.style.bottom = (document.body.clientHeight - (e.clientY - 20)) + "px";
                angular.element(tooltip).removeClass("bp-tooltip-top-tip").addClass("bp-tooltip-bottom-tip");
            } else {
                tooltip.style.bottom = "";
                tooltip.style.top = e.clientY + 30 + "px";
                angular.element(tooltip).removeClass("bp-tooltip-bottom-tip").addClass("bp-tooltip-top-tip");
            }
        }

        function showTooltip(e) {
            angular.element(tooltip).addClass("show");
        }

        function hideTooltip(e) {
            angular.element(tooltip).removeClass("show");
        }

        if ($element && $element.length && this.tooltipContent) {
            if (angular.element(document.body).hasClass("is-touch")) {
                //disabled for touch devices (for now)
            } else {
                let elem = $element[0];
                elem.removeAttribute("bp-tooltip");

                this.attachToBody = zIndexedParent(elem);

                let tooltipContent = document.createElement("DIV");
                tooltipContent.className = "bp-tooltip-content";
                tooltipContent.innerHTML = this.tooltipContent;

                tooltip.appendChild(tooltipContent);

                elem.className += " bp-tooltip-trigger";

                if (this.attachToBody) {
                    document.body.appendChild(tooltip);
                } else {
                    elem.appendChild(tooltip);
                }

                elem.addEventListener("mousemove", updateTooltip);
                elem.addEventListener("mouseover", showTooltip);
                elem.addEventListener("mousedown", hideTooltip);
                elem.addEventListener("mouseout", hideTooltip);
                //elem.addEventListener("transitionend", hideTooltip);

                $scope.$on("$destroy", function () {
                    elem.removeEventListener("mousemove", updateTooltip);
                    elem.removeEventListener("mouseover", showTooltip);
                    elem.removeEventListener("mousedown", hideTooltip);
                    elem.removeEventListener("mouseout", hideTooltip);
                    //elem.removeEventListener("transitionend", hideTooltip);
                    tooltip.remove();
                });
            }
        }
    };

    constructor(
        //list of other dependencies
    ) {
    }

    public static factory() {
        const directive = (
            //list of dependencies
        ) => new BPTooltip(
            //list of other dependencies
        );

        directive["$inject"] = [
            //list of other dependencies
        ];

        return directive;
    }
}
