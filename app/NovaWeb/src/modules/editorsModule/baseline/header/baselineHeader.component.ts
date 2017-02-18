import {BpBaselineHeaderController} from "./baselineHeader.controller";

export class BpBaselineHeader implements ng.IComponentOptions {
    public template: string = require("../../../main/components/bp-artifact-info/bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpBaselineHeaderController;
}
