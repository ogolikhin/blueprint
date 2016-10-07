export class BPToolbarButton implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarButtonController;
    public template: string = require("./bp-toolbar-button.html");
    public bindings: {[boundProperty: string]: string} = {
        click: "&",
        icon: "@",
        disabled: "=?",
        label: "@?",
        tooltip: "@?"
    };
}

export class BPToolbarButtonController implements ng.IComponentController {
    public click: () => void;
    public icon: string;
    public disabled: boolean;
    public label?: string;
    public tooltip?: string;
}