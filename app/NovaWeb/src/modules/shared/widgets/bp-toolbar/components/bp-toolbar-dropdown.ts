import {IBPDropdownMenuItemToolbarOption} from "../options/bp-toolbar-option";

export class BPToolbarDropdown implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarDropdownController;
    public template: string = require("./bp-toolbar-dropdown.html");
    public bindings: {[boundProperty: string]: string} = {
        options: "<",
        icon: "@",
        disabled: "=?",
        label: "@?",
        tooltip: "@?"
    };
}

export class BPToolbarDropdownController implements ng.IComponentController {
    public options: IBPDropdownMenuItemToolbarOption[];
    public icon: string;
    public disabled: boolean;
    public label?: string;
    public tooltip?: string;
}