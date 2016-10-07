import {IBPButtonToolbarOption} from "../options/bp-toolbar-option";

export class BPToolbarToggle implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarToggleController;
    public template: string = require("./bp-toolbar-toggle.html");
    public bindings: {[boundProperty: string]: string} = {
        options: "<",
        disabled: "=?"
    };
}

export class BPToolbarToggleController implements ng.IComponentController {
    public options: IBPButtonToolbarOption[];
    public disabled: boolean;
}