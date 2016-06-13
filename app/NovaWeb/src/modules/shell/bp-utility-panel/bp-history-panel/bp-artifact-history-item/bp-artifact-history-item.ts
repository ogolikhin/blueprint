import { IAppConstants } from "../../../../core";

export class BPArtifactHistoryItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-history-item.html");
    public controller: Function = BPArtifactHistoryItemController;
    public bindings: any = {
        artifactInfo: "="
    };
}

export class BPArtifactHistoryItemController {
    public static $inject: [string] = ["$log", "appConstants"];
    
    constructor(
        private $log: ng.ILogService,
        private appConstants: IAppConstants) {
    }
}
