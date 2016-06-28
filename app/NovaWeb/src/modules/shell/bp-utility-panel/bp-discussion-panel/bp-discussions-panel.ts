import { ILocalizationService } from "../../../core";
import { IProjectManager, Models} from "../../../main";
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
        "projectManager"
    ];

    //private loadLimit: number = 10;
    private artifactId: number;
    private _subscribers: Rx.IDisposable[];

    public artifactDiscussionList: IDiscussion[] = [];
    //public sortOptions: ISortOptions[];
    public sortAscending: boolean = false;
    public isLoading: boolean = false;
    public canCreate: boolean = false;
    public canDelete: boolean = false;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private _artifactDiscussionsRepository: IArtifactDiscussions,
        private projectManager: IProjectManager) {

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

        if (artifact !== null) {
            this.artifactId = artifact.id;
            this.getArtifactDiscussions()
                .then((discussionResultSet: IDiscussionResultSet) => {
                    this.artifactDiscussionList = discussionResultSet.discussions;
                    this.canCreate = discussionResultSet.canCreate;
                    this.canDelete = discussionResultSet.canDelete;
                });
        }
    }

    public expandCollapseDiscussion(discussion: IDiscussion): void {
        if (!discussion.expanded) {
            this.getDiscussionReplies(discussion.discussionId)
                .then((replies: IReply[]) => {
                    discussion.replies = replies;
                    discussion.expanded = true;
                });
        } else {
            discussion.expanded = false;
        }
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
            })
            .finally(() => {
                this.isLoading = false;
            });
    }
}
