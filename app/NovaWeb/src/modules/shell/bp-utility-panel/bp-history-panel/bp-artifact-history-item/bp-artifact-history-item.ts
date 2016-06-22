import { ILocalizationService } from "../../../../core";
import { Models } from "../../../../main";

export class BPArtifactHistoryItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-history-item.html");
    public controller: Function = BPArtifactHistoryItemController;
    public bindings: any = {
        artifactInfo: "="
    };
}

export class BPArtifactHistoryItemController {
    public static $inject: [string] = [
        "$log",
        "localization"
    ];
    
    public getArtifactState: Function = (state: Models.ArtifactStateEnum) => Models.ArtifactStateEnum[state];
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }
}
