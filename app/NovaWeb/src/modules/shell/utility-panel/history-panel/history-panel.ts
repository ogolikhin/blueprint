import {IEventManager, EventSubscriber} from "../../../core/event-manager";
import {IArtifactHistory, IArtifactHistoryVersion} from "./artifact-history.svc";
import * as Models from "../../../main/models/models";

interface ISortOptions {
    value: boolean;
    label: string;
}

export class HistoryPanel implements ng.IComponentOptions {
    public template: string = require("./history-panel.html");
    public controller: Function = HistoryPanelController;
}

export class HistoryPanelController {
    public static $inject: [string] = ["$log", "artifactHistory", "eventManager"];

    private artifactId: number;
    private _listeners: string[];

    public artifactHistoryList: IArtifactHistoryVersion[] = [];
    public sortOptions: ISortOptions[];
    public sortByLatest: boolean = true;
    
    constructor(
        private $log: ng.ILogService,
        private _artifactHistoryRepository: IArtifactHistory,
        private eventManager: IEventManager) {

        this.sortOptions = [
            {value: true, label: "sort by latest"},
            {value: false, label: "sort by earliest"},
        ];

        // TODO: remove 2 lines below
        // this.artifactId = 306; //331;
        // this.getHistoricalVersions(10, 0);
    }

    public $onInit() {
        this._listeners = [
            this.eventManager.attach(EventSubscriber.ProjectManager, "artifactchanged", this.setArtifactId.bind(this))
        ];
    }
    public $onDestroy() {
        this._listeners.map(function (it) {
            this.eventManager.detachById(it);
        }.bind(this));
    }

    private setArtifactId(artifact: Models.IArtifact) {
        this.artifactId = artifact.id;
        this.artifactHistoryList = [];
        this.getHistoricalVersions(10, 0);
    }

    public loadMoreHistoricalVersions(): ng.IPromise<void> {
        let offset: number = this.artifactHistoryList.length;
        return this.getHistoricalVersions(10, offset);
    }

    private getHistoricalVersions(limit: number = 10, offset: number = 0): ng.IPromise<void> {
        return this._artifactHistoryRepository.getArtifactHistory(this.artifactId, limit, offset).then((result) => {
            this.artifactHistoryList = this.artifactHistoryList.concat(result);
        });
    }
}
