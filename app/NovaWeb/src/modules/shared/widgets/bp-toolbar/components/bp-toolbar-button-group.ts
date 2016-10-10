import {IBPButtonAction} from "../actions";

export class BPToolbarButtonGroup implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarButtonGroupController;
    public template: string = require("./bp-toolbar-button-group.html");
    public bindings: { [boundProperty: string]: string } = {
        actions: "<"
    };
}

export class BPToolbarButtonGroupController implements ng.IComponentController {
    public actions: IBPButtonAction[];
}