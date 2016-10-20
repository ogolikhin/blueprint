import {SubArtifactEditorSystemTaskModalController} from "./sub-artifact-editor-system-task-modal-controller";

export class SystemTaskEditor implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = SubArtifactEditorSystemTaskModalController;
    public template: string = require("./sub-artifact-system-task-editor-modal-template.html");
    public bindings: {[boundProperty: string]: string} = {
        resolve: "<",
        modalInstance: "<",
        close: "&?",
        dismiss: "&?"
    };
}