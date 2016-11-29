import {IBPButtonOrDropdownAction} from "../actions";

export class BPToolbarDotsMenu implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarDotsMenuController;
    public template: string = require("./bp-toolbar-dots-menu.html");
    public bindings: {[boundProperty: string]: string} = {
        icon: "@",
        actions: "<",
        disabled: "=?",
        tooltip: "@?"
    };
}

export class BPToolbarDotsMenuController implements ng.IComponentController {
    public icon: string;
    public actions: IBPButtonOrDropdownAction[];
    public disabled: boolean;
    public tooltip?: string;
}
