import { ILocalizationService } from "../../../../core";
import {IArtifactDiscussions, IDiscussion} from "../artifact-discussions.svc";
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
    public discussionInfo: IDiscussion;

    constructor() {
    }

    public newReplyClick(): void {
        this.discussionInfo.showAddReply = true;
    }
}
