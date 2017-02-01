import {BPDiagramController} from "./diagram.controller";

export class BPDiagram implements ng.IComponentOptions {
    public template: string = require("./diagram.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPDiagramController;
}