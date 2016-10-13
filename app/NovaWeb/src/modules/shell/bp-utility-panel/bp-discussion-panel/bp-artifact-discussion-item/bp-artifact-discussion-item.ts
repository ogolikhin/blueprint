import {IDiscussion, IArtifactDiscussions} from "../artifact-discussions.svc";
import {ILocalizationService, IMessageService} from "../../../../core";

export class BPArtifactDiscussionItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-discussion-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPArtifactDiscussionItemController;
    public bindings: any = {
        discussionInfo: "=",
        getReplies: "&",
        canCreate: "=",
        cancelComment: "&",
        artifactId: "=",
        deleteCommentThread: "&",
        discussionEdited: "&",
        emailDiscussionsEnabled: "="
    };
}

export class BPArtifactDiscussionItemController {
    public cancelComment: Function;
    public getReplies: Function;
    public discussionInfo: IDiscussion;
    public canCreate: boolean;
    public editing = false;
    public artifactId: number;
    public deleteCommentThread: Function;
    public discussionEdited: Function;

    public static $inject: [string] = [
        "artifactDiscussions",
        "localization",
        "$sce",
        "messageService"
    ];

    constructor(private artifactDiscussions: IArtifactDiscussions,
                private localization: ILocalizationService,
                private $sce: ng.ISCEService,
                private messageService: IMessageService) {
    }

    public newReplyClick(): void {
        if (this.discussionInfo.isClosed === false && this.canCreate) {
            this.cancelComment();
            this.discussionInfo.showAddReply = true;
        }
    }

    public getTrustedCommentHtml() {
        if (this.discussionInfo &&
            this.discussionInfo.comment &&
            this.discussionInfo.comment.length > 0) {
            return this.$sce.trustAsHtml(this.discussionInfo.comment);
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
        if (this.discussionInfo) {
            return this.canCreate && !this.discussionInfo.isClosed && this.discussionInfo.canEdit;
        } else {
            return false;
        }
    }

    /* tslint:disable:no-unused-variable */
    public editDiscussion(comment: string): ng.IPromise<IDiscussion> {
        return this.artifactDiscussions.editDiscussion(this.artifactId, this.discussionInfo.discussionId, comment)
            .then((discussion: IDiscussion) => {
                this.editing = false;
                this.discussionInfo.comment = comment;
                this.discussionEdited();
                return discussion;
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return null;
            });
    }

    /* tslint:disable:no-unused-variable */
}
