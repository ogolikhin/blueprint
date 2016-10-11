import {IBPToggleItemAction} from "../actions";

export class BPToolbarToggle implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarToggleController;
    public template: string = require("./bp-toolbar-toggle.html");
    public bindings: {[boundProperty: string]: string} = {
        actions: "<",
        currentValue: "=",
        disabled: "=?"
    };
}

export class BPToolbarToggleController implements ng.IComponentController {
    public actions: IBPToggleItemAction[];
    public currentValue: any;
    public disabled: boolean;
}