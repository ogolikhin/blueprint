import {DecisionEditorController} from "./decisionEditor.controller";

export class DecisionEditor implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = DecisionEditorController;
    public template: string = require("./decisionEditor.html");
    public bindings: {[boundProperty: string]: string} = {
        resolve: "<",
        modalInstance: "<",
        close: "&?",
        dismiss: "&?"
    };
}
