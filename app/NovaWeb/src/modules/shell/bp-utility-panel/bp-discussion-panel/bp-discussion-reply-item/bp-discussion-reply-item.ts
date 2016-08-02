import { ILocalizationService } from "../../../../core";
import {IReply, IArtifactDiscussions} from "../artifact-discussions.svc";

export class BPDiscussionReplyItem implements ng.IComponentOptions {
    public template: string = require("./bp-discussion-reply-item.html");
    public controller: Function = BPDiscussionReplyItemController;
    public bindings: any = {
        replyInfo: "=",
        artifactId: "="
    };
}

export class BPDiscussionReplyItemController {
    public replyInfo: IReply;
    public artifactId: number;
    public editing = false;

    public static $inject: [string] = [
        "localization",
        "$sce",
        "artifactDiscussions",
    ];

    constructor(
        private localization: ILocalizationService,
        private $sce: ng.ISCEService,
        private _artifactDiscussionsRepository: IArtifactDiscussions) {
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

    public cancelCommentClick() {
        this.editing = false;
    }

    public editCommentClick() {
        this.editing = true;
    }

    private editReply(comment: string): ng.IPromise<IReply> {
        return this._artifactDiscussionsRepository.editDiscussionReply(this.artifactId, this.replyInfo.discussionId, this.replyInfo.replyId, comment)
            .then((discussion: IReply) => {
                this.editing = false;
                this.replyInfo.comment = comment;
                return discussion;
            });
    }
}