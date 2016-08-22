export class BPCollapsible implements ng.IDirective {
    public restrict = "A";
    public scope = {
        bpCollapsible: "="
    };

    public static $inject = [
        "$timeout"
    ];

    constructor(private $timeout: ng.ITimeoutService) {
    }

    public static factory() {
        const directive = ($timeout: ng.ITimeoutService) => new BPCollapsible($timeout);

        directive["$inject"] = ["$timeout"];

        return directive;
    }

    public link: ng.IDirectiveLinkFn = ($scope: any, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) => {

        let showMore = angular.element("<div class='show-more'><span>Show more</span></div>");
        let showLess = angular.element("<div class='show-less'><span>Show less</span></div>");

        let showMoreClick = () => {
            $element.removeClass("collapsed");
            $element[0].style.height = "";
        };

        let showLessClick = () => {
            $element.addClass("collapsed");
            $element[0].style.height = $scope.bpCollapsible + "px";
        };

        showMore[0].addEventListener("click", showMoreClick);
        showLess[0].addEventListener("click", showLessClick);

        this.$timeout(() => {
            //displays the 'show more' and 'show less' part if comment height is more than desired size + (%30 of desired size)
            if ($element[0].offsetHeight > 1.3 * $scope.bpCollapsible) {
                $element.addClass("collapsed");
                $element[0].style.height = `${$scope.bpCollapsible}px`;
                $element.append(showMore[0]);
                $element.append(showLess[0]);
            }
        });

        $scope.$on("$destroy", () => {
            showMore[0].removeEventListener("click", showMoreClick);
            showLess[0].removeEventListener("click", showLessClick);
            showMore = null;
            showLess = null;
            showLessClick = null;
            showMoreClick = null;
        });
    };
}

