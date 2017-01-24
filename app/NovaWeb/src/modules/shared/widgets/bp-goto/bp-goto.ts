import * as _ from "lodash";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";

export interface IBPGotoController {
    showOrDoSearch($event: MouseEvent);
    hideSearch();
    onKeypress($event: KeyboardEvent);
    onClearInput($event: MouseEvent);
}

export class BPGotoComponent implements ng.IComponentOptions {
    public template: string = require("./bp-goto.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPGotoController;
}

export class BPGotoController implements ng.IComponentController, IBPGotoController {
    public numbersOnlyPattern = /[^0-9]/g;
    public gotoValue: string;
    public tooltip: string;

    public static $inject = [
        "$element",
        "navigationService",
        "localization"
    ];

    constructor(private $element: ng.IAugmentedJQuery,
                private navigationService: INavigationService, private localization: ILocalizationService) {
        this.tooltip = this.localization.get("GO_TO_tooltip");
    }

    private focusInputField() {
        const inputField = this.$element.find("input")[0];
        inputField.focus();
    }

    private blurInputField() {
        const inputField = this.$element.find("input")[0];
        inputField.blur();
    }

    private clearSearch() {
        this.gotoValue = "";
        this.focusInputField();
    }

    public showOrDoSearch($event: MouseEvent) {
        if (this.$element.hasClass("bp-goto--active") === false) {
            $event.preventDefault();
            this.$element.addClass("bp-goto--active");
            this.tooltip = "GO TO Artifact";
            this.focusInputField();
        } else {
            this.performSearch();
        }
    }

    public hideSearch() {
        this.$element.removeClass("bp-goto--active");
        this.tooltip = this.localization.get("GO_TO_tooltip");
        this.blurInputField();
    }

    public onClearInput($event: MouseEvent) {
        $event.preventDefault();
        this.clearSearch();
    }

    public onKeypress($event: KeyboardEvent) {
        if ($event.which === 13) {
            this.performSearch();
        }
    }

    private performSearch() {
        const parsedValue = _.parseInt(this.gotoValue);
        if (!_.isNaN(parsedValue)) {
            this.navigationService.navigateTo({id: parsedValue});
            this.clearSearch();
            this.hideSearch();
        }
    }
}
