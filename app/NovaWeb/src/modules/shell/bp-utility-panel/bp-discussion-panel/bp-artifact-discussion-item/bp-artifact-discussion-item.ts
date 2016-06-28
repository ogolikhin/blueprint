import { ILocalizationService } from "../../../../core";
import {IArtifactDiscussions} from "../artifact-discussions.svc";
import { Models } from "../../../../main";

export class BPArtifactDiscussionItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-discussion-item.html");
    public controller: Function = BPArtifactDiscussionItemController;
    public bindings: any = {
        discussionInfo: "=",
        getReplies: "&"
    };
}

export class BPArtifactDiscussionItemController {
    public getReplies: Function;
    public static $inject: [string] = [
        "$log",
        "localization",
        "artifactDiscussions"
    ];
    
    public getArtifactState: Function = (state: Models.ArtifactStateEnum) => Models.ArtifactStateEnum[state];
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private _artifactDiscussionsRepository: IArtifactDiscussions) {
    }
}
