import {IDiscussion, IArtifactDiscussions} from "../artifact-discussions.svc";
import { ILocalizationService, IMessageService } from "../../../../core";

export class BPArtifactDiscussionItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-discussion-item.html");
    public controller: Function = BPArtifactDiscussionItemController;
    public bindings: any = {
        discussionInfo: "=",
        getReplies: "&",
        canCreate: "=",
        cancelComment: "&",
        artifactId: "=",
        deleteCommentThread: "&",
        discussionEdited: "&"
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
        "$element",
        "$scope",
        "artifactDiscussions",
        "localization",
        "$sce",
        "messageService"
    ];

    constructor(
        private element: ng.IAugmentedJQuery,
        private scope: ng.IScope,
        private _artifactDiscussionsRepository: IArtifactDiscussions,
        private localization: ILocalizationService,
        private $sce: ng.ISCEService,
        private messageService: IMessageService) {
        if (this.discussionInfo) {
            let commentContainer = document.createElement("DIV");
            this.addTargetBlankToComment(commentContainer);
            this.scope.$on("$destroy", () => {
                angular.element(commentContainer).remove();
            });
        }
    }

    private addTargetBlankToComment(commentContainer: HTMLElement) {
        commentContainer.innerHTML = this.discussionInfo.comment;
        angular.element(commentContainer).find("a").attr("target", "_blank");
        this.discussionInfo.comment = commentContainer.innerHTML;
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
        return this._artifactDiscussionsRepository.editDiscussion(this.artifactId, this.discussionInfo.discussionId, comment)
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