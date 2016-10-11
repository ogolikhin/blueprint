import {IBPAction} from "../actions";

export class BPToolbarElement implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarElementController;
    public template: string = require("./bp-toolbar-element.html");
    public bindings: { [boundProperty: string]: string } = {
        action: "<"
    };
}

export class BPToolbarElementController implements ng.IComponentController {
    public action: IBPAction;
}