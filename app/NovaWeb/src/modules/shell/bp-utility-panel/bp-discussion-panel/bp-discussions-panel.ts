import { ILocalizationService } from "../../../core";
import { IProjectManager, Models} from "../../../main";
import {IMessageService} from "../../../shell";
import {IArtifactDiscussions, IDiscussionResultSet, IDiscussion, IReply} from "./artifact-discussions.svc";

export class BPDiscussionPanel implements ng.IComponentOptions {
    public template: string = require("./bp-discussions-panel.html");
    public controller: Function = BPDiscussionPanelController;
}

export class BPDiscussionPanelController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "artifactDiscussions",
        "projectManager",
        "messageService",
        "$q"
    ];

    //private loadLimit: number = 10;
    private artifactId: number;
    private _subscribers: Rx.IDisposable[];

    public artifactDiscussionList: IDiscussion[] = [];
    //public sortOptions: ISortOptions[];
    //public sortAscending: boolean = false;
    public isLoading: boolean = false;
    public canCreate: boolean = false;
    public canDelete: boolean = false;
    public showAddComment: boolean = false;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private _artifactDiscussionsRepository: IArtifactDiscussions,
        private projectManager: IProjectManager,
        private messageService: IMessageService,
        private $q: ng.IQService) {

        //this.sortOptions = [
        //    { value: false, label: this.localization.get("App_UP_Filter_SortByLatest") },
        //    { value: true, label: this.localization.get("App_UP_Filter_SortByEarliest") },
        //];
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        let selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact
            .distinctUntilChanged((v: Models.IArtifact) => v ? v.id : -1) // do not reload if same id is re-selected
            .asObservable()
            .subscribe(this.setArtifactId);

        this._subscribers = [selectedArtifactSubscriber];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });

        // clean up history list
        this.artifactDiscussionList = null;
    }

    private setArtifactId = (artifact: Models.IArtifact) => {
        this.artifactDiscussionList = [];
        this.showAddComment = false;
        if (artifact && artifact.prefix && artifact.prefix !== "ACO" && artifact.prefix !== "_CFL") {
            this.artifactId = artifact.id;
            this.getArtifactDiscussions()
                .then((discussionResultSet: IDiscussionResultSet) => {
                    this.artifactDiscussionList = discussionResultSet.discussions;
                    this.canCreate = discussionResultSet.canCreate;
                    this.canDelete = discussionResultSet.canDelete;
                });
        } else {
            this.artifactId = null;
            this.artifactDiscussionList = [];
            this.canCreate = false;
            this.canDelete = false;
        }
    }

    public expandCollapseDiscussion(discussion: IDiscussion): void {
        if (!discussion.expanded) {
            this.getDiscussionReplies(discussion.discussionId)
                .then((replies: IReply[]) => {
                    discussion.replies = replies;
                    discussion.repliesCount = replies.length;
                    discussion.expanded = true;
                });
        } else {
            discussion.expanded = false;
        }
    }

    public newCommentClick(): void {
        //this.showAddComment = true;
    }

    public cancelCommentClick(): void {
        this.showAddComment = false;
    }

    public cancelReplyClick(discussion: IDiscussion): void {
        discussion.showAddReply = false;
    }

    private getArtifactDiscussions(): ng.IPromise<IDiscussionResultSet> {
        this.isLoading = true;
        return this._artifactDiscussionsRepository.getArtifactDiscussions(this.artifactId)
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
}
