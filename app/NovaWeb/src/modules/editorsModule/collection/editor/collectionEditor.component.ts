import {BpArtifactCollectionEditorController} from "./collectionEditor.controller";

export class BpArtifactCollectionEditor implements ng.IComponentOptions {
    public template: string = require("./collectionEditor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactCollectionEditorController;
}
