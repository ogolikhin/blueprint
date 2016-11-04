import * as _ from "lodash";
import { INavigationService } from "../../../core/navigation";

export interface IBPGotoController {
    showSearch();
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

    public static $inject = [
        "$element",
        "navigationService"
    ];

    constructor(
        private $element: ng.IAugmentedJQuery,
        private navigationService: INavigationService
    ) {
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

    public showSearch() {
        this.$element.addClass("bp-goto--active");
        this.focusInputField();
    }

    public hideSearch() {
        this.$element.removeClass("bp-goto--active");
        this.blurInputField();
    }

    public onClearInput($event: MouseEvent) {
        $event.preventDefault();
        this.clearSearch();
    }

    public onKeypress($event: KeyboardEvent) {
        if ($event.which === 13) {
            const parsedValue = _.parseInt(this.gotoValue);
            if (!_.isNaN(parsedValue)) {
                this.navigationService.navigateTo({ id: parsedValue });
                this.clearSearch();
                this.hideSearch();
            }
        }
    }
}
