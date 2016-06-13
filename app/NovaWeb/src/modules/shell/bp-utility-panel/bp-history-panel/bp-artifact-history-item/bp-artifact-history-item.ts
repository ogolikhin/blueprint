import { IAppConstants, ILocalizationService } from "../../../../core";

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
        "localization",
        "appConstants"];
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private appConstants: IAppConstants) {
    }
}
