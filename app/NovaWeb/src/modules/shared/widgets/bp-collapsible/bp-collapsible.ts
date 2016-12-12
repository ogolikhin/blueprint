import {ILocalizationService} from "../../../core/localization/localizationService";
class BPCollapsibleCtrl {
    static $inject = ["$timeout", "localization"];

    constructor(private $timeout: ng.ITimeoutService, private localization: ILocalizationService) {
        //controller constructor
    }
}

export class BPCollapsible implements ng.IDirective {
    static instance(): ng.IDirectiveFactory {
        const directive = () => new BPCollapsible();
        directive.$inject = ["$timeout", "localization"];
        return directive;
    }

    public restrict = "A";
    public scope = {
        bpCollapsible: "="
    };
    public controller = BPCollapsibleCtrl;
    public controllerAs = "$ctrl";

    public link: ng.IDirectiveLinkFn = ($scope: any, $element: ng.IAugmentedJQuery, attr: ng.IAttributes, ctrl: any) => {
        const element = $element[0] as HTMLElement;

        let showMore = angular.element(`<div class="collapsible__show-more">` +
            `<button class="collapsible__button btn-link">${ctrl.localization.get("App_Collapsible_ShowMore")}</button></div>`);
        let showLess = angular.element(`<div class="collapsible__show-less">` +
            `<button class="collapsible__button btn-link">${ctrl.localization.get("App_Collapsible_ShowLess")}</button></div>`);

        let showMoreClick = () => {
            element.classList.remove("collapsible__collapsed");
            element.classList.add("collapsible__expanded");
            element.style.height = "";
        };

        let showLessClick = () => {
            element.classList.remove("collapsible__expanded");
            element.classList.add("collapsible__collapsed");
            element.style.height = $scope.bpCollapsible + "px";
            element.scrollTop = 0; // scroll to the top of the collapsed element (in case it had a vertical scrollbar)
        };

        showMore[0].addEventListener("click", showMoreClick);
        showLess[0].addEventListener("click", showLessClick);

        ctrl.$timeout(() => {
            //displays the 'show more' and 'show less' part if comment height is more than desired size + (%30 of desired size)
            if (element.offsetHeight > 1.3 * $scope.bpCollapsible) {
                element.classList.add("collapsible__collapsed");
                element.style.height = `${$scope.bpCollapsible}px`;
                element.appendChild(showMore[0]);
                element.appendChild(showLess[0]);
            } else {
                element.classList.add("collapsible__expanded");
            }
        });

        $scope.$on("$destroy", () => {
            showMore[0].removeEventListener("click", showMoreClick);
            showLess[0].removeEventListener("click", showLessClick);
            showMore = undefined;
            showLess = undefined;
            showLessClick = undefined;
            showMoreClick = undefined;
        });
    };
}

