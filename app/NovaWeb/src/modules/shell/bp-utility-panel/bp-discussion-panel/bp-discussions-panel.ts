import {ILocalizationService} from "../../../core";
import {
    IArtifactManager,
    IStatefulArtifact,
    IStatefulSubArtifact
} from "../../../managers/artifact-manager";
import {IArtifactDiscussions, IDiscussionResultSet, IDiscussion, IReply} from "./artifact-discussions.svc";
import {IDialogService} from "../../../shared";
import {IBpAccordionPanelController} from "../../../main/components/bp-accordion/bp-accordion";
import {BPBaseUtilityPanelController} from "../bp-base-utility-panel";
import {Message, MessageType} from "../../../core/messages/message";
import {Helper} from "../../../shared/utils/helper";
import {IMessageService} from "../../../core/messages/message.svc";

export class BPDiscussionPanel implements ng.IComponentOptions {
    public template: string = require("./bp-discussions-panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPDiscussionPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPDiscussionPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "artifactDiscussions",
        "artifactManager",
        "messageService",
        "dialogService",
        "$q"
    ];

    private artifactId: number;
    private subArtifact: IStatefulSubArtifact;

    public artifactDiscussionList: IDiscussion[] = [];
    //public sortOptions: ISortOptions[];
    //public sortAscending: boolean = false;
    public isLoading: boolean = false;
    public canCreate: boolean = false;
    public canDelete: boolean = false;
    public artifactEverPublished: boolean = false;
    public showAddComment: boolean = false;
    public emailDiscussionsEnabled: boolean = false;
    private isVisible: boolean;
    private subscribers: Rx.IDisposable[];

    constructor(private localization: ILocalizationService,
                private artifactDiscussions: IArtifactDiscussions,
                protected artifactManager: IArtifactManager,
                private messageService: IMessageService,
                private dialogService: IDialogService,
                $q: ng.IQService,
                public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, artifactManager.selection, bpAccordionPanel);

        this.subscribers = [];

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
        delete this.artifactDiscussionList;
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        //Subscriber to support refresh case.
        this.subscribers = this.subscribers.filter(subscriber => {
            subscriber.dispose();
            return false;
        });
        this.clearDiscussions();
        if (subArtifact) {
            if (Helper.hasArtifactEverBeenSavedOrPublished(subArtifact)) {
                this.subscribers.push(
                    subArtifact.getObservable().subscribe((subArtif) => {
                        this.onSelectedItemModified(artifact, subArtif, timeout);
                    }));
            }
        } else if (artifact) {
            this.subscribers.push(
                artifact.getObservable().subscribe((artif) => {
                    this.onSelectedItemModified(artif, undefined, timeout);
                }));
        }
        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    protected onVisibilityChanged(isVisible: boolean): void {
        this.isVisible = isVisible;
    }

    private onSelectedItemModified = (artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>) => {
        if (this.isVisible) {
            if (Helper.canUtilityPanelUseSelectedArtifact(artifact)) {
                this.artifactId = artifact.id;
                this.subArtifact = subArtifact;
                //We always should artifact version
                this.artifactEverPublished = artifact.version > 0;
                return this.setDiscussions(timeout);
            }
        }
    }

    private clearDiscussions() {
        this.artifactId = undefined;
        this.subArtifact = undefined;
        this.artifactDiscussionList = [];
        this.showAddComment = false;
        this.setReadOnly();
    }

    private setReadOnly() {
        this.canCreate = false;
        this.canDelete = false;
        this.artifactEverPublished = false;
    }

    private setControllerFieldsAndFlags(discussionResultSet: IDiscussionResultSet) {
        this.artifactDiscussionList = discussionResultSet.discussions;
        this.canCreate = discussionResultSet.canCreate && this.artifactEverPublished;
        this.canDelete = discussionResultSet.canDelete;
        this.emailDiscussionsEnabled = discussionResultSet.emailDiscussionsEnabled;
    }

    private setDiscussions(timeout?: ng.IPromise<void>): ng.IPromise<void> {
        return this.getDiscussions(this.artifactId, this.subArtifact ? this.subArtifact.id : undefined, timeout)
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

    private getDiscussions(artifactId: number, subArtifactId: number = undefined, timeout?: ng.IPromise<void>): ng.IPromise<IDiscussionResultSet> {
        this.isLoading = true;
        return this.artifactDiscussions.getDiscussions(artifactId, subArtifactId, timeout)
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
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Comment"))
            .then(() => {
                this.artifactDiscussions.deleteReply(reply.itemId, reply.replyId).then((result: boolean) => {
                    this.getDiscussionReplies(discussion.discussionId)
                        .then((updatedReplies: IReply[]) => {
                            discussion.replies = updatedReplies;
                            discussion.repliesCount = updatedReplies.length;
                            discussion.expanded = true;
                        });
                }).catch((error) => {
                    this.messageService.addMessage(new Message(MessageType.Error, error.message));
                });
            });
    }

    public deleteCommentThread(discussion: IDiscussion) {
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Comment_Thread"))
            .then(() => {
                this.artifactDiscussions.deleteDiscussion(discussion.itemId, discussion.discussionId).then((result: boolean) => {
                    this.getDiscussions(this.artifactId, this.subArtifact ? this.subArtifact.id : undefined)
                        .then((discussionsResultSet: IDiscussionResultSet) => {
                            this.setControllerFieldsAndFlags(discussionsResultSet);
                        });
                }).catch((error) => {
                    this.messageService.addMessage(new Message(MessageType.Error, error.message));
                });
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
