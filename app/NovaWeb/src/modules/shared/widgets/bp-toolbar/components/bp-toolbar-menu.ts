import {IBPButtonOrDropdownAction} from "../actions";

export class BPToolbarMenu implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarMenuController;
    public template: string = require("./bp-toolbar-menu.html");
    public bindings: {[boundProperty: string]: string} = {
        icon: "@",
        actions: "<",
        disabled: "=?",
        tooltip: "@?"
    };
}

export class BPToolbarMenuController implements ng.IComponentController {
    public icon: string;
    public actions: IBPButtonOrDropdownAction[];
    public disabled: boolean;
    public tooltip?: string;
}
