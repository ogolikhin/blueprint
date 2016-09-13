export class BpProcessTypeToggle implements ng.IComponentOptions {
    public template: string = require("./bp-process-type-toggle.html");
    public controller: Function = BpProcessTypeToggleController;
    public controllerAs: string = "$ctrl";
}

class BpProcessTypeToggleController implements ng.IComponentController {
    constructor() {
    }
}