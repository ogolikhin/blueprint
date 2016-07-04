import { ILocalizationService } from "../../../../core";
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

    public expanded: boolean = false;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }

    public expand() {
        this.expanded = !this.expanded;
    }

    public limitChars(str) {
        return str.substring(0, 100) + "...";

    }
}
