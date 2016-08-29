﻿import { ILocalizationService, IMessageService, IStateManager } from "../../../core";
import { ISelectionManager, Models, IArtifactService} from "../../../main";
import { IArtifactDiscussions, IDiscussionResultSet, IDiscussion, IReply } from "./artifact-discussions.svc";
import { IDialogService } from "../../../shared";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { Message, MessageType} from "../../../core/messages/message";
import { Helper } from "../../../shared/utils/helper";

export class BPDiscussionPanel implements ng.IComponentOptions {
    public template: string = require("./bp-discussions-panel.html");
    public controller: Function = BPDiscussionPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPDiscussionPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "artifactDiscussions",
        "selectionManager",
        "stateManager",
        "messageService",
        "dialogService",
        "$q",
        "artifactService"
    ];

    //private loadLimit: number = 10;
    private artifactId: number;
    private subArtifact: Models.ISubArtifact;

    public artifactDiscussionList: IDiscussion[] = [];
    //public sortOptions: ISortOptions[];
    //public sortAscending: boolean = false;
    public isLoading: boolean = false;
    public canCreate: boolean = false;
    public canDelete: boolean = false;
    public artifactEverPublished: boolean = false;
    public showAddComment: boolean = false;
    public emailDiscussionsEnabled: boolean = false;

    constructor(
        private localization: ILocalizationService,
        private artifactDiscussions: IArtifactDiscussions,
        protected selectionManager: ISelectionManager,
        protected stateManager: IStateManager,
        private messageService: IMessageService,
        private dialogService: IDialogService,
        $q: ng.IQService,
        private artifactService: IArtifactService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, selectionManager, stateManager, bpAccordionPanel);

        //this.sortOptions = [
        //    { value: false, label: this.localization.get("App_UP_Filter_SortByLatest") },
        //    { value: true, label: this.localization.get("App_UP_Filter_SortByEarliest") },
        //];
    }

    public $onInit() {
        super.$onInit();
    }

    public $onDestroy() {
        super.$onDestroy();

        // clean up history list
        this.artifactDiscussionList = null;
    }

    protected onSelectionChanged(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        this.artifactDiscussionList = [];
        this.showAddComment = false;
        if (Helper.canUtilityPanelUseSelectedArtifact(artifact)) {
            this.artifactId = artifact.id;
            this.subArtifact = subArtifact;
            if (artifact.version) {
                return this.setEverPublishedAndDiscussions(artifact.version, timeout);
            } else {
                return this.artifactService.getArtifact(artifact.id, timeout).then((result: Models.IArtifact) => {
                    artifact = result;
                    this.setEverPublishedAndDiscussions(artifact.version, timeout);
                }).catch((error: any) => {
                    if (error) {
                        this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                    }
                    artifact = null;
                });
            }
        } else {
            this.artifactId = null;
            this.subArtifact = null;
            this.artifactDiscussionList = [];
            this.canCreate = false;
            this.canDelete = false;
            this.artifactEverPublished = false;
        }
        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private setControllerFieldsAndFlags(discussionResultSet: IDiscussionResultSet) {
        this.artifactDiscussionList = discussionResultSet.discussions;
        this.canCreate = discussionResultSet.canCreate && this.artifactEverPublished;
        this.canDelete = discussionResultSet.canDelete;
        this.emailDiscussionsEnabled = discussionResultSet.emailDiscussionsEnabled;
    }

    private setEverPublishedAndDiscussions(artifactVersion, timeout: ng.IPromise<void>): ng.IPromise<void> {
        //We should not check the subartifact version to make sure it's published
        this.artifactEverPublished = artifactVersion > 0;
        return this.setDiscussions(timeout);
    }

    private setDiscussions(timeout?: ng.IPromise<void>): ng.IPromise<void> {
        return this.getArtifactDiscussions(this.artifactId, this.subArtifact ? this.subArtifact.id : null, timeout)
            .then((discussionResultSet: IDiscussionResultSet) => {
                this.setControllerFieldsAndFlags(discussionResultSet);
            });
    }

    private setReplies(discussion: IDiscussion) {
            this.getDiscussionReplies(discussion.discussionId)
                .then((replies: IReply[]) => {
                    discussion.replies = replies;
                    discussion.repliesCount = replies.length;
            });
    }

    public expandCollapseDiscussion(discussion: IDiscussion): void {
        if (!discussion.expanded) {
            this.setReplies(discussion);
                    discussion.expanded = true;
        } else {
            discussion.expanded = false;
        }
    }

    /* tslint:disable:no-unused-variable */
    public addArtifactDiscussion(comment: string): ng.IPromise<IDiscussion> {
        let artifactId = this.subArtifact ? this.subArtifact.id : this.artifactId;
        return this.artifactDiscussions.addDiscussion(artifactId, comment)
            .then((discussion: IDiscussion) => {
                this.cancelCommentClick();
                this.setDiscussions();
                return discussion;
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return null;
            });
    }
    /* tslint:disable:no-unused-variable */

    /* tslint:disable:no-unused-variable */
    public addDiscussionReply(discussion: IDiscussion, comment: string): ng.IPromise<IReply> {
        let artifactId = this.subArtifact ? this.subArtifact.id : this.artifactId;
        return this.artifactDiscussions.addDiscussionReply(artifactId, discussion.discussionId, comment)
            .then((reply: IReply) => {
                this.setReplies(discussion);
                discussion.showAddReply = false;
                if (!discussion.expanded) {
                    discussion.expanded = true;
                }
                return reply;
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return null;
            });
    }
    /* tslint:disable:no-unused-variable */

    public newCommentClick(): void {
        if (this.canCreate) {
            this.hideAddReplies();
            this.showAddComment = true;
        }
    }

    public cancelCommentClick(): void {
        this.showAddComment = false;
        this.hideAddReplies();
    }

    private hideAddReplies() {
        this.artifactDiscussionList.filter((discussion) => discussion.showAddReply === true).forEach((discussion) => discussion.showAddReply = false);
    }

    public cancelReplyClick(discussion: IDiscussion): void {
        discussion.showAddReply = false;
    }

    private getArtifactDiscussions(artifactId: number, subArtifactId: number = null, timeout?: ng.IPromise<void>): ng.IPromise<IDiscussionResultSet> {
        this.isLoading = true;
        return this.artifactDiscussions.getArtifactDiscussions(artifactId, subArtifactId, timeout)
            .then((discussionResultSet: IDiscussionResultSet) => {
                return discussionResultSet;
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private getDiscussionReplies(discussionId: number): ng.IPromise<IReply[]> {
        this.isLoading = true;
        return this.artifactDiscussions.getReplies(this.artifactId, discussionId)
            .then((replies: IReply[]) => {
                return replies;
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return [];
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    public deleteReply(discussion: IDiscussion, reply: IReply) {
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Comment")).then((confirmed: boolean) => {
            if (confirmed) {
                this.artifactDiscussions.deleteReply(reply.itemId, reply.replyId).then((result: boolean) => {
                    this.getDiscussionReplies(discussion.discussionId)
                        .then((updatedReplies: IReply[]) => {
                            discussion.replies = updatedReplies;
                            discussion.repliesCount = updatedReplies.length;
                            discussion.expanded = true;
                        });
                }).catch((error) => { this.messageService.addMessage(new Message(MessageType.Error, error.message)); });
            }
        });
    }

    public deleteCommentThread(discussion: IDiscussion) {
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Comment_Thread")).then((confirmed: boolean) => {
            if (confirmed) {
                this.artifactDiscussions.deleteCommentThread(discussion.itemId, discussion.discussionId).then((result: boolean) => {
                    this.getArtifactDiscussions(discussion.itemId).then((discussionsResultSet: IDiscussionResultSet) => {
                        this.setControllerFieldsAndFlags(discussionsResultSet);
                    });
                }).catch((error) => { this.messageService.addMessage(new Message(MessageType.Error, error.message)); });
            }
        });
    }

    public discussionEdited(discussion: IDiscussion) {
        if (this.artifactDiscussionList.length > 1) {
            const currentIndex = this.artifactDiscussionList.indexOf(discussion);
            if (currentIndex > 0) {
                this.artifactDiscussionList.splice(currentIndex, 1);
                this.artifactDiscussionList.splice(0, 0, discussion);
            }
        }
    }
}
