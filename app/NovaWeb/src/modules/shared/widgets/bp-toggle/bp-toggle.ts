export interface IBPToggleController {
    toggle(): void;
}

export class BPToggleController implements IBPToggleController {
    public toggle() {
        return;
    }
}

export class BPToggleComponent implements ng.IComponentOptions {
    public template: string = require("./bp-toggle.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToggleController;
    public bindings: any = {};
}


