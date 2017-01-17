import * as angular from "angular";

export class BPTooltip implements ng.IDirective {
    public restrict = "A";

    // Cannot have isolated scope, as the directive can be placed on other components/elements with additional
    // directives and that will generate errors => [$compile:multidir] Multiple directives
    // public scope = {
    //     bpTooltip: "@",
    //     bpTooltipTruncated: "<?",
    //     bpTooltipLimit: "<?"
    // };

    public link: Function = ($scope: ng.IScope, $element: ng.IAugmentedJQuery): void => {
        const defaultLimit = 250; // default limit after which the tooltip will get automatically truncated

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
            // we make sure the tooltip has been added to the DOM before calling getBoundingClientRect to avoid this IE bug:
            // https://connect.microsoft.com/IE/feedback/details/829392
            if (tooltip.parentElement) {
                const rect = tooltip.getBoundingClientRect();
                if (rect.height < e.clientY - 20) {
                    tooltip.style.top = "";
                    tooltip.style.bottom = (document.body.clientHeight - (e.clientY - 20)) + "px";
                    angular.element(tooltip).removeClass("bp-tooltip-top-tip").addClass("bp-tooltip-bottom-tip");
                } else {
                    tooltip.style.bottom = "";
                    tooltip.style.top = e.clientY + 30 + "px";
                    angular.element(tooltip).removeClass("bp-tooltip-bottom-tip").addClass("bp-tooltip-top-tip");
                }
            }
        }

        function createTooltip(e?: MouseEvent) {
            if (window["MutationObserver"]) {
                observer = new MutationObserver((mutations) => {
                    mutations.forEach((mutation) => {
                        const tooltipText = angular.element(mutation.target).attr("bp-tooltip");
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

            const tooltipLimit = _.toLength($element.attr("bp-tooltip-limit")) || defaultLimit;
            let tooltipText = $element.attr("bp-tooltip");
            if (tooltipLimit < tooltipText.length) {
                tooltipText = tooltipText.slice(0, tooltipLimit) + "…";
            }

            // shouldDisplayTooltipForTruncated() only checks if tooltip should be displayed initially.
            // Doesn't account for edge case where text changes when mouse is already over the element
            if (tooltipText !== "" && shouldDisplayTooltipForTruncated()) {
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
        function shouldDisplayTooltipForTruncated() {
            if ($element.attr("bp-tooltip-truncated") === "true") {
                const elem = $element[0];

                let clientRect = elem.getBoundingClientRect();
                const width = clientRect.width;
                const height = _.ceil(clientRect.height) + 1; // to account for the different ways browsers calculate/approximate font heights
                // this allows to deal with inline elements, whose scrollWidth/Height is 0
                let scrollWidth = _.max([elem.scrollWidth, _.round(width)]);
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
                    scrollWidth = _.max([scrollWidth, child.scrollWidth, _.round(clientRect.width)]);
                    scrollHeight = _.max([scrollHeight, child.scrollHeight, _.ceil(clientRect.height)]);

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
                const elem = $element[0];

                elem.addEventListener("mousemove", updateTooltip);
                elem.addEventListener("mouseover", createTooltip);
                elem.addEventListener("mousedown", hideTooltip);
                elem.addEventListener("mouseout", removeTooltip);
                //elem.addEventListener("transitionend", hideTooltip);

                $scope.$on("$destroy", () => {
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
