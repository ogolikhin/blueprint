import { ILocalizationService, IStateManager } from "../../../core";
import { ISelectionManager, Models} from "../../../main";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { IArtifactHistory, IArtifactHistoryVersion } from "./artifact-history.svc";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";

interface ISortOptions {
    value: boolean;
    label: string;
}

export class BPHistoryPanel implements ng.IComponentOptions {
    public template: string = require("./bp-history-panel.html");
    public controller: Function = BPHistoryPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPHistoryPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "$q",
        "localization",
        "artifactHistory",
        "selectionManager",
        "stateManager"
    ];

    private loadLimit: number = 10;
    private artifactId: number;

    public artifactHistoryList: IArtifactHistoryVersion[] = [];
    public sortOptions: ISortOptions[];
    public sortAscending: boolean = false;
    public selectedArtifactVersion: IArtifactHistoryVersion;
    public isLoading: boolean = false;
    
    constructor(
        $q: ng.IQService,
        private localization: ILocalizationService,
        private artifactHistory: IArtifactHistory,
        protected selectionManager: ISelectionManager,
        protected stateManager: IStateManager,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, selectionManager, stateManager, bpAccordionPanel);

        this.sortOptions = [
            { value: false, label: this.localization.get("App_UP_Filter_SortByLatest") },
            { value: true, label: this.localization.get("App_UP_Filter_SortByEarliest") },
        ];
    }

    public $onInit() {
        super.$onInit();
    }

    public $onDestroy() {
        super.$onDestroy();

        // clean up history list
        this.artifactHistoryList = null;
    }

    public changeSortOrder() {
        this.artifactHistoryList = [];
        this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending)
            .then( (list: IArtifactHistoryVersion[]) => {
                this.artifactHistoryList = list;
            });
    }

    protected onSelectionChanged(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        if (artifact == null) {
            this.artifactHistoryList = [];
            return super.onSelectionChanged(artifact, subArtifact, timeout);
        }
        if (this.artifactId !== artifact.id) {
            this.artifactHistoryList = [];
            this.artifactId = artifact.id;
            return this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending, timeout)
                .then( (list: IArtifactHistoryVersion[]) => {
                    this.artifactHistoryList = list;
                });
        }
    }

    public loadMoreHistoricalVersions(): ng.IPromise<IArtifactHistoryVersion[]> {
        let offset: number = this.artifactHistoryList.length;
        let lastItem: IArtifactHistoryVersion = null;

        if (this.artifactHistoryList.length) {
            lastItem = this.artifactHistoryList[this.artifactHistoryList.length - 1];
        }

        // if reached the end (version 1 or draft), don't try to load more
        if (lastItem && lastItem.versionId !== 1 && !this.isDeletedOrDraft(lastItem)) {
            return this.getHistoricalVersions(this.loadLimit, offset, null, this.sortAscending)
                .then( (list: IArtifactHistoryVersion[]) => {
                    this.artifactHistoryList = this.artifactHistoryList.concat(list);
                    return list;
                });
        } else {
            return null;
        }
    }

    private isDeletedOrDraft(item: IArtifactHistoryVersion): boolean {
        return item.artifactState === Models.ArtifactStateEnum.Draft 
                || item.artifactState === Models.ArtifactStateEnum.Deleted;
    }

    public selectArtifactVersion(artifactHistoryItem: IArtifactHistoryVersion): void {
        this.selectedArtifactVersion = artifactHistoryItem;
    }

    private getHistoricalVersions(
        limit: number,
        offset: number, userId:
        string, asc: boolean,
        timeout?: ng.IPromise<void>): ng.IPromise<IArtifactHistoryVersion[]> {

        this.isLoading = true;
        return this.artifactHistory.getArtifactHistory(this.artifactId, limit, offset, userId, asc, timeout)
            .then( (list: IArtifactHistoryVersion[]) => {
                return list;
            })
            .finally( () => {
                this.isLoading = false;
            });
    }
}
