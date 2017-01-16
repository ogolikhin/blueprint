import {ILocalizationService} from "../../../core/localization/localizationService";
class BPCollapsibleCtrl {
    static $inject = ["$timeout", "$compile", "localization"];
    private element: HTMLElement;

    constructor(private $timeout: ng.ITimeoutService,
                private $compile: ng.ICompileService,
                private localization: ILocalizationService) {
        //controller constructor
    }

    public showMoreClick() {
        this.element.classList.remove("collapsible__collapsed");
        this.element.classList.add("collapsible__expanded");
        this.element.style.height = "";
    }

    public showLessClick(height: number) {
        this.element.classList.remove("collapsible__expanded");
        this.element.classList.add("collapsible__collapsed");
        this.element.style.height = `${height}px`;
        this.element.scrollTop = 0; // scroll to the top of the collapsed element (in case it had a vertical scrollbar)
    }
}

export class BPCollapsible implements ng.IDirective {
    static instance(): ng.IDirectiveFactory {
        const directive = () => new BPCollapsible();
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
        const desiredHeight = ($scope.bpCollapsible || 0) as number;
        ctrl.element = element;

        const showMore = angular.element(ctrl.$compile(`<div class="collapsible__show-more">` +
            `<button class="collapsible__button btn-link" ng-click="$ctrl.showMoreClick()">${ctrl.localization.get("App_Collapsible_ShowMore")}</button>` +
            `</div>`)($scope));
        const showLess = angular.element(ctrl.$compile(`<div class="collapsible__show-less">` +
            `<button class="collapsible__button btn-link" ng-click="$ctrl.showLessClick(${desiredHeight})">` +
            `${ctrl.localization.get("App_Collapsible_ShowLess")}</button></div>`)($scope));

        ctrl.$timeout(() => {
            //displays the 'show more' and 'show less' part if comment height is more than desired size + (%30 of desired size)
            if (element.offsetHeight > 1.3 * desiredHeight) {
                element.classList.add("collapsible__collapsed");
                element.style.height = `${desiredHeight}px`;
                element.appendChild(showMore[0]);
                element.appendChild(showLess[0]);
            } else {
                element.classList.add("collapsible__expanded");
            }
        });
    };
}

