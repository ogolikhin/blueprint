import { IAppConstants, ILocalizationService } from "../../../core";
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
        "projectManager",
        "$q",
        "appConstants"];

    private loadLimit: number = 10;
    private artifactId: number;
    private _subscribers: Rx.IDisposable[];

    public artifactHistoryList: IArtifactHistoryVersion[] = [];
    public sortOptions: ISortOptions[];
    public sortAscending: boolean = false;
    public selectedArtifactVersion: IArtifactHistoryVersion;
    public artifactHistoryListObserver;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private _artifactHistoryRepository: IArtifactHistory,
        private projectManager: IProjectManager,
        private $q: ng.IQService,
        private appConstants: IAppConstants) {

        this.sortOptions = [
            { value: false, label: this.localization.get("App_UP_Filter_SortByLatest") },
            { value: true, label: this.localization.get("App_UP_Filter_SortByEarliest") },
        ];

        this.artifactHistoryListObserver = this._artifactHistoryRepository.artifactHistory;
        
        this.projectManager.currentArtifact.asObservable().subscribe(this.setArtifactId);
        console.log("about to make a request to get value");

        // TODO: remove 2 lines below
        this.artifactId = 306; //331;
        this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending);
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        this._subscribers = [
            this.artifactHistoryListObserver.subscribe((value) => {
                console.log("updated value: " + value);
                this.artifactHistoryList = this.artifactHistoryList.concat(value);
            })
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        (this._subscribers || []).map((it: Rx.IDisposable) => it.dispose());
    }


    public changeSortOrder() {
        this.artifactHistoryList = [];
        this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending);
    }

    private setArtifactId = (artifact: Models.IArtifact) => {
        this.artifactHistoryList = [];

        if (artifact !== null) {
            this.artifactId = artifact.id;
            this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending);
        }
    }

    public loadMoreHistoricalVersions(): ng.IPromise<void> {
        let offset: number = this.artifactHistoryList.length;
        let lastItem: IArtifactHistoryVersion = null;

        if (this.artifactHistoryList.length) {
            lastItem = this.artifactHistoryList[this.artifactHistoryList.length - 1];
        }

        // if reached the end (version 1 or draft), don't try to load more
        if (lastItem && lastItem.versionId !== 1 && lastItem.versionId !== this.appConstants.draftVersion) {
            return this.getHistoricalVersions(this.loadLimit, offset, null, this.sortAscending);
        } else {
            let deferred: ng.IDeferred<any> = this.$q.defer();
            deferred.resolve([]);
            return deferred.promise;
        }
    }

    public selectArtifactVersion(artifactHistoryItem: IArtifactHistoryVersion): void {
        this.selectedArtifactVersion = artifactHistoryItem;
    }

    private getHistoricalVersions(limit: number, offset: number, userId: string, asc: boolean): ng.IPromise<void> {
        return this._artifactHistoryRepository.getArtifactHistory(this.artifactId, limit, offset, userId, asc).then((result) => {
            //this.artifactHistoryList = this.artifactHistoryList.concat(result);
        });
    }
}
