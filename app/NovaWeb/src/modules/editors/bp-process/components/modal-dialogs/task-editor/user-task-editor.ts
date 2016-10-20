import {SubArtifactEditorUserTaskModalController} from "./sub-artifact-editor-user-task-modal-controller";

export class UserTaskEditor implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = SubArtifactEditorUserTaskModalController;
    public template: string = require("./sub-artifact-user-task-editor-modal-template.html");
    public bindings: {[boundProperty: string]: string} = {
        resolve: "<",
        modalInstance: "<",
        close: "&?",
        dismiss: "&?"
    };
}