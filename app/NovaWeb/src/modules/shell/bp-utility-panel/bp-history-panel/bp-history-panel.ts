import { ILocalizationService } from "../../../core";
import { IProjectManager, Models} from "../../../main";
import {IArtifactHistory, IArtifactHistoryVersion} from "./artifact-history.svc";

interface ISortOptions {
    value: boolean;
    label: string;
}

export class BPHistoryPanel implements ng.IComponentOptions {
    public template: string = require("./bp-history-panel.html");
    public controller: Function = BPHistoryPanelController;
}

export class BPHistoryPanelController {
    public static $inject: [string] = [
        "$log", 
        "localization",
        "artifactHistory",
        "projectManager"
    ];

    private loadLimit: number = 10;
    private artifactId: number;
    private _subscribers: Rx.IDisposable[];

    public artifactHistoryList: IArtifactHistoryVersion[] = [];
    public sortOptions: ISortOptions[];
    public sortAscending: boolean = false;
    public selectedArtifactVersion: IArtifactHistoryVersion;
    public isLoading: boolean = false;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private _artifactHistoryRepository: IArtifactHistory,
        private projectManager: IProjectManager) {

        this.sortOptions = [
            { value: false, label: this.localization.get("App_UP_Filter_SortByLatest") },
            { value: true, label: this.localization.get("App_UP_Filter_SortByEarliest") },
        ];
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        let selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact
//            .distinctUntilChanged( (v: Models.IArtifact) => v ? v.id : -1) // do not reload if same id is re-selected
            .asObservable()
            .subscribe(this.setArtifactId);

        this._subscribers = [ selectedArtifactSubscriber ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });

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

    private setArtifactId = (artifact: Models.IArtifact) => {
        this.artifactHistoryList = [];

        if (artifact !== null) {
            this.artifactId = artifact.id;
            this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending)
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

    private getHistoricalVersions(limit: number, offset: number, userId: string, asc: boolean): ng.IPromise<IArtifactHistoryVersion[]> {
        this.isLoading = true;
        return this._artifactHistoryRepository.getArtifactHistory(this.artifactId, limit, offset, userId, asc)
            .then( (list: IArtifactHistoryVersion[]) => {
                return list;
            })
            .finally( () => {
                this.isLoading = false;
            });
    }
}
