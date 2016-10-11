interface IOption {
    value: any;
    label: string;
}

interface IBPSelectController {
    ngModelCtrl: ng.INgModelController;
    ngModel: any;
    options: IOption[];
    buttonLabel: string;
}

export class BPSelect implements ng.IComponentOptions {
    public template: string = require("./bp-select.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPSelectController;
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
    public options: IOption[];
    public selectedOption: IOption;

    constructor() {
    }

    public $onChanges(changesObj) {
        this.selectedOption = this.options.filter(o => o.value === this.ngModel)[0];
    }

    public onOptionSelect(option: IOption) {
        this.selectedOption = option;
        this.ngModel = option.value;
        this.ngModelCtrl.$setViewValue(option.value);
    }
}
