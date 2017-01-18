import {IStatefulArtifact, IStatefulSubArtifact} from "../../../managers/artifact-manager";
import {IArtifactHistory, IArtifactHistoryVersion} from "./artifact-history.svc";
import {BPBaseUtilityPanelController} from "../bp-base-utility-panel";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {ILocalizationService} from "../../../core/localization/localization.service";
import {ArtifactStateEnum} from "../../../main/models/models";
import {Helper} from "../../../shared/utils/helper";

interface ISortOptions {
    value: boolean;
    label: string;
}

export class BPHistoryPanel implements ng.IComponentOptions {
    public template: string = require("./bp-history-panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPHistoryPanelController;
    public bindings = {
        context: "<"
    };
}

export class BPHistoryPanelController extends BPBaseUtilityPanelController {
    public artifactHistoryList: IArtifactHistoryVersion[] = [];
    public sortOptions: ISortOptions[];
    public sortAscending: boolean = false;
    public isLoading: boolean = false;

    private subscribers: Rx.IDisposable[];
    private loadLimit: number = 10;
    private artifactId: number;

    public static $inject: [string] = [
        "$q",
        "localization",
        "artifactHistory",
        "navigationService"
    ];

    constructor($q: ng.IQService,
                private localization: ILocalizationService,
                private artifactHistory: IArtifactHistory,
                private navigationService: INavigationService) {

        super($q);

        this.sortOptions = [
            {value: false, label: this.localization.get("App_UP_Filter_SortByLatest")},
            {value: true, label: this.localization.get("App_UP_Filter_SortByEarliest")}
        ];

        this.subscribers = [];
    }

    public $onDestroy() {
        super.$onDestroy();

        // clean up history list
        delete this.artifactHistoryList;
    }

    public changeSortOrder() {
        this.artifactHistoryList = [];
        this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending)
            .then((list: IArtifactHistoryVersion[]) => {
                this.artifactHistoryList = list;
            });
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {

        //Subscriber to support refresh case.
        this.subscribers = this.subscribers.filter(subscriber => {
            subscriber.dispose();
            return false;
        });

        this.clearHistoryList();

        if (artifact) {
            this.subscribers.push(
                artifact.getObservable()
                    .subscribe((artif) => {
                        this.onSelectionChangedHelper(artif, timeout);
                    }));
        }

        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private clearHistoryList() {
        this.artifactHistoryList = [];
    }

    private onSelectionChangedHelper = (artifact: IStatefulArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> => {
        this.artifactId = artifact.id;
        return this.getHistoricalVersions(this.loadLimit, 0, null, this.sortAscending, timeout)
            .then((list: IArtifactHistoryVersion[]) => {
                this.artifactHistoryList = list;
            });
    };

    public loadMoreHistoricalVersions(): ng.IPromise<IArtifactHistoryVersion[]> {
        let offset: number = this.artifactHistoryList.length;
        let lastItem: IArtifactHistoryVersion = null;

        if (this.artifactHistoryList.length) {
            lastItem = this.artifactHistoryList[this.artifactHistoryList.length - 1];
        }

        // if reached the end (version 1 or draft), don't try to load more
        if (lastItem && lastItem.versionId !== 1 && !this.isDeletedOrDraft(lastItem)) {
            return this.getHistoricalVersions(this.loadLimit, offset, null, this.sortAscending)
                .then((list: IArtifactHistoryVersion[]) => {
                    this.artifactHistoryList = this.artifactHistoryList.concat(list);
                    return list;
                });
        } else {
            return null;
        }
    }

    private isDeletedOrDraft(item: IArtifactHistoryVersion): boolean {
        return item.artifactState === ArtifactStateEnum.Draft
            || item.artifactState === ArtifactStateEnum.Deleted;
    }

    public getItemVersionId(item: IArtifactHistoryVersion) {
        return item && item.versionId !== Helper.draftVersion ? item.versionId : undefined;
    }

    private getHistoricalVersions(limit: number,
                                  offset: number, userId: string, asc: boolean,
                                  timeout?: ng.IPromise<void>): ng.IPromise<IArtifactHistoryVersion[]> {

        this.isLoading = true;
        return this.artifactHistory.getArtifactHistory(this.artifactId, limit, offset, userId, asc, timeout)
            .then((list: IArtifactHistoryVersion[]) => {
                return list;
            })
            .finally(() => {
                this.isLoading = false;
            });
    }
}
