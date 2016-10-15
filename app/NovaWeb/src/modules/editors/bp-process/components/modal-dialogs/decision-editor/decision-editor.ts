import {DecisionEditorController} from "./decision-editor-controller";

export class DecisionEditor implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = DecisionEditorController;
    public template: string = require("./decision-editor.html");
    public bindings: {[boundProperty: string]: string} = {
        resolve: "<",
        close: "&?",
        dismiss: "&?"
    };
}