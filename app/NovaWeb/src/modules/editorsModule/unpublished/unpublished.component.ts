import {UnpublishedController} from "./unpublished.controller";

export class UnpublishedComponent implements ng.IComponentOptions {
    public template: string = require("./unpublished.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = UnpublishedController;
}
