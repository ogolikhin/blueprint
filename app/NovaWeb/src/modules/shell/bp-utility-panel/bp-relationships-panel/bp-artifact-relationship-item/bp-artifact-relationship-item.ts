﻿import { ILocalizationService } from "../../../../core";
import { Models } from "../../../../main";

export class BPArtifactRelationshipItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-relationship-item.html");
    public controller: Function = BPArtifactRelationshipItemController;
    public bindings: any = {
        artifact: "="
    };
}

export class BPArtifactRelationshipItemController {
    public static $inject: [string] = [
        "$log",
        "localization"
    ];   
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }
}
