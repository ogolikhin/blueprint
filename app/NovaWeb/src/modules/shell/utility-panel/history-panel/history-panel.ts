﻿import { IAppConstants } from "../../../core";
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
    public static $inject: [string] = [
        "$log", 
        "artifactHistory",
        "eventManager",
        "$q",
        "appConstants"];

    private loadLimit: number = 10;
    private artifactId: number;
    private _listeners: string[];

    public artifactHistoryList: IArtifactHistoryVersion[] = [];
    public sortOptions: ISortOptions[];
    public sortAscending: boolean = false;
    public selectedArtifactVersion: IArtifactHistoryVersion;
    
    constructor(
        private $log: ng.ILogService,
        private _artifactHistoryRepository: IArtifactHistory,
        private eventManager: IEventManager,
        private $q: ng.IQService,
        private appConstants: IAppConstants) {

        this.sortOptions = [
            {value: false, label: "sort by latest"},
            {value: true, label: "sort by earliest"},
        ];

        // TODO: remove 2 lines below
        // this.artifactId = 306; //331;
        // this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending);
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

    public changeSortOrder() {
        this.artifactHistoryList = [];
        this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending);
    }

    private setArtifactId(artifact: Models.IArtifact) {
        this.artifactId = artifact.id;
        this.artifactHistoryList = [];
        this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending);
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

    private getHistoricalVersions(limit: number = this.loadLimit, offset: number = 0, userId: string = null, asc: boolean = false): ng.IPromise<void> {
        return this._artifactHistoryRepository.getArtifactHistory(this.artifactId, limit, offset, userId, asc).then((result) => {
            this.artifactHistoryList = this.artifactHistoryList.concat(result);
        });
    }
}
