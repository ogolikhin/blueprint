import {BpGeneralArtifactEditorController} from "./artifactGeneralEditor.controller";

export class BpArtifactGeneralEditorComponent implements ng.IComponentOptions {
    public template: string = require("./generalEditor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpGeneralArtifactEditorController;
}
