import {BpArtifactDetailsEditorController} from "./artifactDetailsEditor.controller";

export class BpArtifactDetailsEditor implements ng.IComponentOptions {
    public template: string = require("./detailsEditor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactDetailsEditorController;
}
