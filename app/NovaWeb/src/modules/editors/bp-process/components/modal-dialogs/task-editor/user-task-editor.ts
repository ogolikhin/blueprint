import {UserTaskModalController} from "./user-task-modal-controller";

export class UserTaskEditor implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = UserTaskModalController;
    public template: string = require("./user-task-modal-template.html");
    public bindings: {[boundProperty: string]: string} = {
        resolve: "<",
        modalInstance: "<",
        close: "&?",
        dismiss: "&?"
    };
}
