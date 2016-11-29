import {IBPButtonDropdownItemAction} from "../actions";

export class BPToolbarButtonDropdown implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarButtonDropdownController;
    public template: string = require("./bp-toolbar-button-dropdown.html");
    public bindings: {[boundProperty: string]: string} = {
        actions: "<",
        icon: "@",
        disabled: "=?",
        label: "@?",
        tooltip: "@?"
    };
}

export class BPToolbarButtonDropdownController implements ng.IComponentController {
    public actions: IBPButtonDropdownItemAction[];
    public icon: string;
    public disabled: boolean;
    public label?: string;
    public tooltip?: string;
}
