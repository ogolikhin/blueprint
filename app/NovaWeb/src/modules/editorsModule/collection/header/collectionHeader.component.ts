import {BpCollectionHeaderController} from "./collectionHeader.controller";

export class BpCollectionHeader implements ng.IComponentOptions {
    public template: string = require("../../../main/components/bp-artifact-info/bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpCollectionHeaderController;
}
