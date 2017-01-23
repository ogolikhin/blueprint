import {INavigationService} from "../../../commonModule/navigation/navigation.service";
export interface IBpLinksHelper {
    hasExternalLink($element: ng.IAugmentedJQuery): boolean;
    hasBlueprintLink($element: ng.IAugmentedJQuery): boolean;
    isRichTextMentionLink($element: ng.IAugmentedJQuery): boolean;
    isValidLink($element: ng.IAugmentedJQuery): boolean;
    getItemId($element: ng.IAugmentedJQuery): number;
}

export class BpLinksHelper implements IBpLinksHelper {

    private jsRegEx = /^\s*javascript/i;

    public hasExternalLink($element: ng.IAugmentedJQuery) {
        return !$element.attr("ui-sref") && $element.attr("href") !== "#" && !this.jsRegEx.test($element.attr("href"));
    }

    public hasBlueprintLink($element: ng.IAugmentedJQuery) {
        return !!($element.parent().attr("linkassemblyqualifiedname")) || !!($element.attr("linkassemblyqualifiedname"));
    }

    public isRichTextMentionLink($element: ng.IAugmentedJQuery) {
        return $element.attr("linkassemblyqualifiedname").toLowerCase().indexOf("richtextmentionlink") < 0;
    }

    public getItemId($element: ng.IAugmentedJQuery) {
        return $element.attr("subartifactid") ? Number($element.attr("subartifactid")) : Number($element.attr("artifactid"));
    }

    public isValidLink($element: ng.IAugmentedJQuery) {
        return $element.attr("isvalid").toLowerCase() === "true";
    }

}

export class BpSpecialLinkContainer implements ng.IDirective {

    public static factory() {
        const directive = (navigationService: INavigationService, bpLinksHelper: IBpLinksHelper) =>
            new BpSpecialLinkContainer(navigationService, bpLinksHelper);
        directive.$inject = ["navigationService", "bpLinksHelper"];
        return directive;
    }

    public restrict = "E";

    constructor(private navigationService: INavigationService, private bpLinksHelper: IBpLinksHelper) {
    }

    public link: ng.IDirectiveLinkFn = ($scope: ng.IScope, $element: ng.IAugmentedJQuery) => {

        $element.on("click", (e) => this.onElementClick(e, $scope));

    };

    private onElementClick(e: JQueryEventObject, $scope: ng.IScope) {
        const $target = angular.element(e.target);
        //sometimes $anchor is not $target
        const $anchor = this.getSelfOrImmediateParent($target);
        if (!$anchor) {
            return;
        }

        if (this.bpLinksHelper.hasBlueprintLink($anchor)) {
            e.preventDefault();

            //navigate to internal link
            if (this.bpLinksHelper.isRichTextMentionLink($anchor) && this.bpLinksHelper.isValidLink($anchor)) {
                const id = this.bpLinksHelper.getItemId($anchor);
                this.navigationService.navigateTo({id: id});
            }
            return;
        }

        if (this.bpLinksHelper.hasExternalLink($anchor)) {
            $anchor.attr("target", "_blank");
            return;
        }
    }

    private getSelfOrImmediateParent($target: ng.IAugmentedJQuery) {
        if ($target[0].tagName === "A") {
            return $target;
        }
        const $parent = $target.parent();
        const element = $parent[0];
        if (element && element.tagName === "A") {
            return $parent;
        }
        return null;
    }

}
