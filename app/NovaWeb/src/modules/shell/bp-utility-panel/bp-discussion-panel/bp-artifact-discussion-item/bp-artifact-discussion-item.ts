import {IDiscussion} from "../artifact-discussions.svc";
import { ILocalizationService } from "../../../../core";

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

    public static $inject: [string] = [
        "$element",
        "$scope",
        "localization",
        "$sce"
    ];

    constructor(
        private element: ng.IAugmentedJQuery,
        private scope: ng.IScope,
        private localization: ILocalizationService,
        private $sce: ng.ISCEService) {
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
        //this.discussionInfo.showAddReply = true;
    }

    public getTrustedCommentHtml() {
        if (this.discussionInfo) {
            return this.$sce.trustAsHtml(this.discussionInfo.comment);
        } else {
            return "";
        }
        //return this.discussionInfo.comment;
    };
}
