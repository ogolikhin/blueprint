import {IBPButtonToolbarOption} from "../options/bp-toolbar-option";

export class BPToolbarButtonGroup implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarButtonGroupController;
    public template: string = require("./bp-toolbar-button-group.html");
    public bindings: { [boundProperty: string]: string } = {
        options: "<"
    };
}

export class BPToolbarButtonGroupController implements ng.IComponentController {
    public options: IBPButtonToolbarOption[];
}