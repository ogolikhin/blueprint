import { ILocalizationService } from "../../../../core";
import { Models } from "../../../../main";

export class BPArtifactDiscussionItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-discussion-item.html");
    public controller: Function = BPArtifactDiscussionItemController;
    public bindings: any = {
        discussionInfo: "=",
        artifactInfo: "="
    };
}

export class BPArtifactDiscussionItemController {
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
