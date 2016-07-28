import { ILocalizationService, IMessageService } from "../../../core";
import { IProjectManager, Models} from "../../../main";

import {IArtifactDiscussions, IDiscussionResultSet, IDiscussion, IReply} from "./artifact-discussions.svc";
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
        "projectManager",
        "messageService",
        "$q"
    ];

    //private loadLimit: number = 10;
    private artifactId: number;

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
        protected projectManager: IProjectManager,
        private messageService: IMessageService,
        private $q: ng.IQService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super(projectManager, bpAccordionPanel);

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

    protected setArtifactId = (artifact: Models.IArtifact) => {
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
