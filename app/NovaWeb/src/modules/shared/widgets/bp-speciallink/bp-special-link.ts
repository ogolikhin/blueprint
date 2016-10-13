import * as angular from "angular";
/*
 *Code copied from old Rapid Review Shell look at TargetBlankDirective.ts for reference.
 */
const jsRegEx = /^\s*javascript/i;

export var linkRules = {
    externalLinks: {
        matchedIfAllFalse: true,
        //if one of the matchers return true, execute action
        matchers: [
            ($scope, $element) => {
                return $element.attr("href") === "#";
            },
            ($scope, $element) => {
                return jsRegEx.test($element.attr("href"));
            }
        ],

        action: ($anchor) => {
            $anchor.attr("target", "_blank");
        }
    },

    dialogLinks: {
        matchers: [
            ($scope, $element) => {
                return !!($element.parent().attr("linkassemblyqualifiedname")) || !!($element.attr("linkassemblyqualifiedname"));
            }
        ],

        action: ($anchor, e, scope) => {
            e.preventDefault();

            //ignore mentioned user
            if ($anchor.attr("linkassemblyqualifiedname").toLowerCase().indexOf("richtextmentionlink") < 0) {
                if (console) {
                    console.log("Navigate to artifact is not implemented yet.");
                }
            }
        }
    }
};

export class BpSpecialLinkContainer implements ng.IDirective {

    public static factory() {
        const directive = () => new BpSpecialLinkContainer();
        return directive;
    }

    public restrict = "E";

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
        for (let ruleName in linkRules) {
            const rule = linkRules[ruleName];

            if (rule.matchedIfAllFalse) {
                if (!this.isMatched(rule.matchers, $scope, $anchor)) {
                    rule.action($anchor, e, $scope);
                }
            } else {
                if (this.isMatched(rule.matchers, $scope, $anchor)) {
                    rule.action($anchor, e, $scope);
                }
            }
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

    private isMatched(matchers: [any], $scope: ng.IScope, $anchor: JQuery) {
        for (let fn of matchers) {
            if (fn($scope, $anchor)) {
                return true;
            }
        }
    }
}
