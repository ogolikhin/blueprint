import {IBPAction} from "../actions";

export class BPToolbar implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarController;
    public template: string = require("./bp-toolbar.html");
    public bindings: {[boundProperty: string]: string} = {
        actions: "<"
    };
}

export class BPToolbarController implements ng.IComponentController {
    public actions: IBPAction[];

    public $onInit(): void {
        this.actions = this.actions !== undefined ? this.actions : [];
    }

    public $onDestroy(): void {
        delete this.actions;
    }
}