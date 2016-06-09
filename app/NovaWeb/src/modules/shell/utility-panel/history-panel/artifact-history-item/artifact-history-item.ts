export class ArtifactHistoryItem implements ng.IComponentOptions {
    public template: string = require("./artifact-history-item.html");
    public controller: Function = ArtifactHistoryItemController;
    public bindings: any = {
        artifactInfo: "="
    };
}

export class ArtifactHistoryItemController {
    public static $inject: [string] = ["$log"];
    
    constructor(private $log: ng.ILogService) {
    }
}
