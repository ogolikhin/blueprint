import * as _ from "lodash";
import { INavigationService } from "../../../core/navigation";

export interface IBPGotoController {
    showSearch();
    clearSearch();
    onKeypress($event: KeyboardEvent);
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

    public showSearch() {
        this.$element.addClass("active");
        
        const inputField = this.$element.find("input")[0];
        inputField.focus();
    }

    public clearSearch() {
        this.$element.removeClass("active");
        this.gotoValue = "";
    }

    public onKeypress($event: KeyboardEvent) {
        if ($event.which === 13) {
            const parsedValue = _.parseInt(this.gotoValue);
            if (!_.isNaN(parsedValue)) {
                this.navigationService.navigateTo(parsedValue);
                this.clearSearch();
            }
        }
    }
}
