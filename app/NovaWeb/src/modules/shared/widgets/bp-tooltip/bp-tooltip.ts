import * as angular from "angular";

interface IBPTooltipScope extends ng.IScope {
    bpTooltip: string;
    bpTooltipTruncated: boolean;
    bpTooltipLimit: number;
}

export class BPTooltip implements ng.IDirective {
    public restrict = "A";

    public scope = {
        bpTooltip: "@",
        bpTooltipTruncated: "<?",
        bpTooltipLimit: "<?"
    };

    public link: Function = ($scope: IBPTooltipScope, $element: ng.IAugmentedJQuery): void => {
        let observer;

        const tooltip = document.createElement("DIV");
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

            const tooltipLimit = _.toLength($scope.bpTooltipLimit);
            let tooltipText = $scope.bpTooltip;
            if (tooltipLimit > 0 && tooltipLimit < tooltipText.length) {
                tooltipText = $scope.bpTooltip.substr(0, tooltipLimit) + "&#8230;";
            }

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

        function isTriggerJustAWrapper(elem: HTMLElement): boolean {
            if (elem.childElementCount === 0) {
                return false;
            }

            if (elem.childElementCount === 1) {
                return elem.textContent.trim() === elem.firstElementChild.textContent.trim();
            }

            return false;
        }

        // only checks the immediate text or the immediate (and only) child, not nested HTML elements
        function shouldDisplayTooltipForTruncated(element: ng.IAugmentedJQuery) {
            if ($scope.bpTooltipTruncated) {
                const elem = element[0];

                let clientRect = elem.getBoundingClientRect();
                const width = clientRect.width;
                const height = _.ceil(clientRect.height);
                // this allows to deal with inline elements, whose scrollWidth/Height is 0
                let scrollWidth = _.max([elem.scrollWidth, width]);
                let scrollHeight = _.max([elem.scrollHeight, height]);

                if (!isTriggerJustAWrapper(elem) &&
                    (width < scrollWidth || height < scrollHeight)) {
                    return true;
                }

                if (isTriggerJustAWrapper(elem)) {
                    const child = elem.firstElementChild as HTMLElement;
                    const computedStyle = window.getComputedStyle(child);
                    const availableWidth = _.round(width) - parseFloat(computedStyle.marginLeft) - parseFloat(computedStyle.marginRight);
                    const availableHeight = height - parseFloat(computedStyle.marginTop) - parseFloat(computedStyle.marginBottom);

                    clientRect = child.getBoundingClientRect();
                    scrollWidth = _.max([child.scrollWidth, _.round(clientRect.width)]);
                    scrollHeight = _.max([child.scrollHeight, _.ceil(clientRect.height)]);

                    return availableWidth < scrollWidth || availableHeight < scrollHeight;
                }

                return false;
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
