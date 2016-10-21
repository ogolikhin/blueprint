import {SystemTaskModalController} from "./system-task-modal-controller";

export class SystemTaskEditor implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = SystemTaskModalController;
    public template: string = require("./system-task-modal-template.html");
    public bindings: {[boundProperty: string]: string} = {
        resolve: "<",
        modalInstance: "<",
        close: "&?",
        dismiss: "&?"
    };
}