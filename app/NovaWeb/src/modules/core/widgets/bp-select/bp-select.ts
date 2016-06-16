// import { ILocalizationService } from "../../../core";

interface IBPSelectController {
    ngModelCtrl: ng.INgModelController;
    ngModel: any;
    options: any[];
    buttonLabel: string;
}

export class BPSelect implements ng.IComponentOptions {
    public template: string = require("./bp-select.html");
    public controller: Function = BPSelectController;
    public require: any = {
        ngModelCtrl: "ngModel"
    };
    public bindings: any = {
        ngModel: "=",
        options: "=",
        buttonLabel: "@?"
    };
}

export class BPSelectController implements IBPSelectController {
    static $inject = [];

    public ngModelCtrl: ng.INgModelController;
    public ngModel: any;
    public buttonLabel: string;
    public options: any[];
    public selectedOption: any;

    constructor() { }

    public $onChanges(changesObj) {
        this.selectedOption = this.options.filter(o => o.value === this.ngModel)[0];
    }

    public onOptionSelect(option: any) {
        this.selectedOption = option;
        this.ngModel = option.value;
        this.ngModelCtrl.$setViewValue(option.value);
    }
}
