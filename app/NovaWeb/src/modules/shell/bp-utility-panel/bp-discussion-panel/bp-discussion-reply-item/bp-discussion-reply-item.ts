import { ILocalizationService } from "../../../../core";
import {IReply} from "../artifact-discussions.svc";

export class BPDiscussionReplyItem implements ng.IComponentOptions {
    public template: string = require("./bp-discussion-reply-item.html");
    public controller: Function = BPDiscussionReplyItemController;
    public bindings: any = {
        replyInfo: "="
    };
}

export class BPDiscussionReplyItemController {
    public replyInfo: IReply;

    public static $inject: [string] = [
        "localization",
        "$sce"
    ];

    constructor(
        private localization: ILocalizationService,
        private $sce: ng.ISCEService) {
    }

    public getTrustedCommentHtml() {
        if (this.replyInfo &&
            this.replyInfo.comment &&
            this.replyInfo.comment.length > 0) {
            return this.$sce.trustAsHtml(this.replyInfo.comment);
        } else {
            return "";
        }
        //return this.discussionInfo.comment;
    };
}
