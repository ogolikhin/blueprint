import {IBPToolbarOption} from "../options/bp-toolbar-option";

export class BPToolbarElement implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarElementController;
    public template: string = require("./bp-toolbar-element.html");
    public bindings: { [boundProperty: string]: string } = {
        option: "<"
    };
}

export class BPToolbarElementController implements ng.IComponentController {
    public option: IBPToolbarOption;
}