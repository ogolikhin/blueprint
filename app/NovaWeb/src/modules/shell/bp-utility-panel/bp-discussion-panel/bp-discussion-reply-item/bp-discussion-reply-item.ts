import { ILocalizationService, IMessageService } from "../../../../core";
import { IReply, IArtifactDiscussions } from "../artifact-discussions.svc";

export class BPDiscussionReplyItem implements ng.IComponentOptions {
    public template: string = require("./bp-discussion-reply-item.html");
    public controller: Function = BPDiscussionReplyItemController;
    public bindings: any = {
        replyInfo: "=",
        artifactId: "=",
        canCreate: "=",
        discussionClosed: "=",
        deleteReply: "&"
    };
}

export class BPDiscussionReplyItemController {
    public replyInfo: IReply;
    public artifactId: number;
    public editing = false;
    public canCreate: boolean;
    public discussionClosed: boolean;
    public deleteReply: Function;

    public static $inject: [string] = [
        "localization",
        "$sce",
        "artifactDiscussions",
        "messageService"
    ];

    constructor(
        private localization: ILocalizationService,
        private $sce: ng.ISCEService,
        private _artifactDiscussionsRepository: IArtifactDiscussions,
        private messageService: IMessageService) {
    }

    public getTrustedCommentHtml() {
        if (this.replyInfo &&
            this.replyInfo.comment &&
            this.replyInfo.comment.length > 0) {
            return this.$sce.trustAsHtml(this.replyInfo.comment);
        } else {
            return "";
        }
    };

    public cancelCommentClick() {
        this.editing = false;
    }

    public editCommentClick() {
        if (this.canEdit()) {
            this.editing = true;
        }
    }

    public canEdit(): boolean {
        if (this.replyInfo) {
            return this.canCreate && !this.discussionClosed && this.replyInfo.canEdit;
        } else {
            return false;
        }
    }

    /* tslint:disable:no-unused-variable */
    public editReply(comment: string): ng.IPromise<IReply> {
        return this._artifactDiscussionsRepository.editDiscussionReply(this.artifactId, this.replyInfo.discussionId, this.replyInfo.replyId, comment)
            .then((reply: IReply) => {
                this.editing = false;
                this.replyInfo.comment = comment;
                return reply;
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return null;
            });
    }
    /* tslint:disable:no-unused-variable */
}