import { ILocalizationService, IMessageService } from "../../../core";
import { ISelectionManager, Models} from "../../../main";
import { IArtifactDiscussions, IDiscussionResultSet, IDiscussion, IReply } from "./artifact-discussions.svc";
import { IDialogService } from "../../../shared";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";

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
        "messageService",
        "dialogService",
        "$q"
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
    public showAddComment: boolean = false;

    constructor(
        private localization: ILocalizationService,
        private _artifactDiscussionsRepository: IArtifactDiscussions,
        protected selectionManager: ISelectionManager,
        private messageService: IMessageService,
        private dialogService: IDialogService,
        private $q: ng.IQService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super(selectionManager, bpAccordionPanel);

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

    protected onSelectionChanged = (artifact: Models.IArtifact, subArtifact: Models.ISubArtifact) => {
        this.artifactDiscussionList = [];
        this.showAddComment = false;
        if (artifact && artifact.prefix && artifact.prefix !== "ACO" && artifact.prefix !== "_CFL") {
            this.artifactId = artifact.id;
            this.subArtifact = subArtifact;
            this.setDiscussions();
        } else {
            this.artifactId = null;
            this.subArtifact = null;
            this.artifactDiscussionList = [];
            this.canCreate = false;
            this.canDelete = false;
        }
    }

    private setDiscussions() {
        this.getArtifactDiscussions(this.artifactId, this.subArtifact ? this.subArtifact.id : null)
            .then((discussionResultSet: IDiscussionResultSet) => {
                this.artifactDiscussionList = discussionResultSet.discussions;
                this.canCreate = discussionResultSet.canCreate;
                this.canDelete = discussionResultSet.canDelete;
                if (this.artifactDiscussionList.length > 0) {
                    this.expandCollapseDiscussion(this.artifactDiscussionList[0]);
                }
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
    private addArtifactDiscussion(comment: string): ng.IPromise<IDiscussion> {
        this.isLoading = true;
        return this._artifactDiscussionsRepository.addDiscussion(this.artifactId, comment)
            .then((discussion: IDiscussion) => {
                this.cancelCommentClick();
                this.setDiscussions();
                return discussion;
            }).catch((error: any) => {
                if (error.statusCode && error.statusCode !== 1401) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return null;
            })
            .finally(() => {
                this.isLoading = false;
            });
    }
    /* tslint:disable:no-unused-variable */

    /* tslint:disable:no-unused-variable */
    private addDiscussionReply(discussion: IDiscussion, comment: string): ng.IPromise<IReply> {
        this.isLoading = true;
        return this._artifactDiscussionsRepository.addDiscussionReply(this.artifactId, discussion.discussionId, comment)
            .then((reply: IReply) => {
                this.setReplies(discussion);
                discussion.showAddReply = false;
                if (!discussion.expanded) {
                    this.expandCollapseDiscussion(discussion);
                }
                return reply;
            }).catch((error: any) => {
                if (error.statusCode && error.statusCode !== 1401) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return null;
            })
            .finally(() => {
                this.isLoading = false;
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

    private getArtifactDiscussions(artifactId: number, subArtifactId: number = null): ng.IPromise<IDiscussionResultSet> {
        this.isLoading = true;
        return this._artifactDiscussionsRepository.getArtifactDiscussions(artifactId, subArtifactId)
            .then((discussionResultSet: IDiscussionResultSet) => {
                return discussionResultSet;
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private getDiscussionReplies(discussionId: number): ng.IPromise<IReply[]> {
        this.isLoading = true;
        return this._artifactDiscussionsRepository.getReplies(this.artifactId, discussionId)
            .then((replies: IReply[]) => {
                return replies;
            }).catch((error: any) => {
                if (error.statusCode && error.statusCode !== 1401) {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                }
                return [];
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    public deleteReply(discussion: IDiscussion, reply: IReply) {
        this.dialogService.confirm("Comment will be deleted. Continue?").then((confirmed: boolean) => {
            if (confirmed) {
                this._artifactDiscussionsRepository.deleteReply(reply.itemId, reply.replyId).then((result: boolean) => {
                    this.getDiscussionReplies(discussion.discussionId)
                        .then((updatedReplies: IReply[]) => {
                            discussion.replies = updatedReplies;
                            discussion.repliesCount = updatedReplies.length;
                            discussion.expanded = true;
                        });
                });
            }
        });
    }

    public deleteCommentThread(discussion: IDiscussion) {
        this.dialogService.confirm("Comment Thread will be deleted. Continue?").then((confirmed: boolean) => {
            if (confirmed) {
                this._artifactDiscussionsRepository.deleteCommentThread(discussion.itemId, discussion.discussionId).then((result: boolean) => {
                    this.getArtifactDiscussions(discussion.itemId).then((discussionsResultSet: IDiscussionResultSet) => {
                        this.artifactDiscussionList = discussionsResultSet.discussions;
                        this.canDelete = discussionsResultSet.canDelete;
                        this.canCreate = discussionsResultSet.canCreate;
                    });
                });
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
