import {BpGlossaryController} from "./glossary.controller";

export class BpGlossary implements ng.IComponentOptions {
    public template: string = require("./glossary.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpGlossaryController;
}
