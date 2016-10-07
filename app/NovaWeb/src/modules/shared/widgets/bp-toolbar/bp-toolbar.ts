import {IBPToolbarOption} from "./options/bp-toolbar-option";

export class BPToolbar implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarController;
    public template: string = require("./bp-toolbar.html");
    public bindings: {[boundProperty: string]: string} = {
        options: "<"
    };
}

export class BPToolbarController implements ng.IComponentController {
    public options: IBPToolbarOption[];

    public $onInit(): void {
        this.options = this.options !== undefined ? this.options : [];
    }

    public $onDestroy(): void {
        delete this.options;
    }
}