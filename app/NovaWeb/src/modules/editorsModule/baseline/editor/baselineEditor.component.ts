import {BpArtifactBaselineEditorController} from "./baselineEditor.controller";

export class BpArtifactBaselineEditor implements ng.IComponentOptions {
    public template: string = require("./baselineEditor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactBaselineEditorController;
}
