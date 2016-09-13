export class BpProcessTypeToggle implements ng.IComponentOptions {
    public template: string = require("./bp-process-type-toggle.html");
    public controller: Function = BpProcessTypeToggleController;
    public controllerAs: string = "$ctrl";
}

interface IProcessTypeToggleOption {
    id: number;
    value: string;
    iconClass: string;
    isChecked: boolean;
}

class BpProcessTypeToggleController implements ng.IComponentController {
    public options: IProcessTypeToggleOption[];

    constructor() {
        this.options = [
            {
                id: 0,
                value: "business",
                iconClass: "fonticon2-user-user",
                isChecked: false
            },
            {
                id: 1,
                value: "userToSystem",
                iconClass: "fonticon2-user-system",
                isChecked: false
            }
        ];
    }
}