import * as angular from "angular";
export class BPTooltip implements ng.IDirective {
    public restrict = "A";

    public link: Function = ($scope: ng.IScope, $element: ng.IAugmentedJQuery): void => {
        let observer;

        let tooltip = document.createElement("DIV");
        tooltip.className = "bp-tooltip";

        function updateTooltip(e: MouseEvent) {
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

        function createTooltip(e?: MouseEvent) {
            if (window["MutationObserver"]) {
                observer = new MutationObserver(function (mutations) {
                    mutations.forEach(function (mutation) {
                        let tooltipText = angular.element(mutation.target).attr("bp-tooltip");
                        angular.element(tooltip).children().html(tooltipText);
                        angular.element(tooltip).addClass("show");
                    });
                });
                observer.observe(this, {
                    attributes: true,
                    //attributeOldValue: true,
                    attributeFilter: ["bp-tooltip"]
                });
            }

            let tooltipText = angular.element(this).attr("bp-tooltip");

            // shouldDisplayTooltipForTruncated() only checks if tooltip should be displayed initially.
            // Doesn't account for edge case where text changes when mouse is already over the element
            if (tooltipText !== "" && shouldDisplayTooltipForTruncated(angular.element(this))) {
                let tooltipContent = document.createElement("DIV");
                tooltipContent.className = "bp-tooltip-content";
                tooltipContent.textContent = tooltipText;

                angular.element(tooltip).empty();
                tooltip.appendChild(tooltipContent);
                document.body.appendChild(tooltip);

                angular.element(this).addClass("bp-tooltip-trigger");
                angular.element(tooltip).addClass("show");
            }
        }

        // only checks the immediate text, not nested HTML elements
        function shouldDisplayTooltipForTruncated(element: ng.IAugmentedJQuery) {
            if (element.attr("bp-tooltip-truncated") === "true") {
                const elem = element[0];
                // the "- 1" allows some wiggle room in IE, as scrollWidth/Height round to the biggest integer
                // while offsetWidth/Height to the smallest
                let compensateWidth: boolean = false;
                if (Math.abs(elem.offsetWidth - elem.scrollWidth) <= 1) {
                    let realWidth = window.getComputedStyle(elem).width;
                    if (Math.abs(parseFloat(realWidth) - parseInt(realWidth, 10)) < 0.5) {
                        compensateWidth = true;
                    }
                }
                return (elem && (
                    elem.offsetWidth < elem.scrollWidth - (compensateWidth ? 1 : 0) ||
                    elem.offsetHeight < elem.scrollHeight - 3)
                );
            }
            return true;
        }

        function hideTooltip(e?: MouseEvent) {
            angular.element(tooltip).removeClass("show");
        }

        function removeTooltip(e?: MouseEvent) {
            angular.element(this).removeClass("bp-tooltip-trigger");
            angular.element(tooltip).remove();

            if (window["MutationObserver"] && observer) {
                observer.disconnect();
                observer = null;
            }
        }

        if ($element && $element.length) {
            if (angular.element(document.body).hasClass("is-touch")) {
                //disabled for touch devices (for now)
            } else {
                let elem = $element[0];

                elem.addEventListener("mousemove", updateTooltip);
                elem.addEventListener("mouseover", createTooltip);
                elem.addEventListener("mousedown", hideTooltip);
                elem.addEventListener("mouseout", removeTooltip);
                //elem.addEventListener("transitionend", hideTooltip);

                $scope.$on("$destroy", function () {
                    removeTooltip();

                    elem.removeEventListener("mousemove", updateTooltip);
                    elem.removeEventListener("mouseover", createTooltip);
                    elem.removeEventListener("mousedown", hideTooltip);
                    elem.removeEventListener("mouseout", removeTooltip);
                    //elem.removeEventListener("transitionend", hideTooltip);
                });
            }
        }
    };

    constructor() {
        //fixme: empty constructors are not needed
    }

    public static factory() {
        const directive = (//list of dependencies
        ) => new BPTooltip(
            //list of other dependencies
        );

        directive["$inject"] = [
            //list of other dependencies
        ];

        return directive;
    }
}
