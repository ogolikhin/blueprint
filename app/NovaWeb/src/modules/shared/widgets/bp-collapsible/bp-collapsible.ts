export class BPCollapsible implements ng.IDirective {
    public restrict = "A";
    public scope = {
        bpCollapsible: "="
    };

    constructor() {
    }

    public static directive: any[] = [
        () => {
            return new BPCollapsible();
        }];

    public link: ng.IDirectiveLinkFn = ($scope: any, $element: ng.IAugmentedJQuery, attr: ng.IAttributes, $timeout: ng.ITimeoutService) => {

        let showMore = angular.element("<div class='show-more'><span>Show more</span></div>");
        let showLess = angular.element("<div class='show-less'><span>Show less</span></div>");

        let showMoreClick = () => {
            $element.removeClass("collapsed");
        };

        let showLessClick = () => {
            $element.addClass("collapsed");
        };

        showMore[0].addEventListener("click", showMoreClick);
        showLess[0].addEventListener("click", showLessClick);

        $element.ready(function () {
            if ($element[0].offsetHeight > $scope.bpCollapsible) {
                $element.addClass("collapsed");
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

