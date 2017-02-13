import {IMetaDataService} from "../../../managers/artifact-manager/metadata/metadata.svc";
import {IItemType} from "../../models/models";
import {Models} from "../../models";
import {IProjectExplorerService} from "../bp-explorer/project-explorer.service";

export interface ISearchMetadata {
    totalCount: number;
    totalPages: number;
    pageSize: number;
    items: any[];
}

export interface ISearchItem {
    artifactId: number;
    createdBy: string;
    createdDateTime: string;
    createdUser: number;
    isSubartifact: boolean;
    itemId: number;
    itemTypeId: number;
    lastModifiedBy: string;
    lastModifiedDateTime: string;
    lastModifiedUser: number;
    name: string;
    projectId: number;
    propertyName: string;
    propertyTypeId: number;
    searchableValue: string;
    subartifactId: number;
    typeName: string;
    typePrefix: string;
    iconImageId: number;
    predefinedType: number;
    artifactClass: string;
}

export interface ISearchResult {
    items: ISearchItem[];
    page: number;
    pageItemCount: number;
    pageSize: number;
}
export interface IQuickSearchService {
    search(term: string, eventSource?: string, page?: number, pageSize?: number): ng.IPromise<ISearchResult>;
    metadata(term: string, page?: number, pageSize?: number): ng.IPromise<ISearchMetadata>;
    searchTerm: string;
    canSearch(): boolean;
}

export class QuickSearchService implements IQuickSearchService {
    static $inject = [
        "$q",
        "$http",
        "$timeout",
        "$log",
        "projectExplorerService",
        "metadataService"
    ];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $timeout: ng.ITimeoutService,
                private $log: ng.ILogService,
                private projectExplorerService: IProjectExplorerService,
                private metadataService: IMetaDataService) {
    }

    searchTerm: string;

    canSearch(): boolean {
        return !(this.projectExplorerService.projects.length > 0);
    }

    private appendParameters(url: string, page: number, pageSize: number): string {
        if (page) {
            url = url + `?page=${page}`;
            if (pageSize) {
                url = url + `&pageSize=${pageSize}`;
            }
        } else if (pageSize) {
            url = url + `?pageSize=${pageSize}`;
        }
        return url;
    }

    private getSearchUrl(page: number, pageSize: number): string {
        const url = `/svc/searchservice/itemsearch/fulltext/`;
        return this.appendParameters(url, page, pageSize);
    }

    private getMetadataUrl(page: number, pageSize: number): string {
        const url = `/svc/searchservice/itemsearch/fulltextmetadata/`;
        return this.appendParameters(url, page, pageSize);
    }

    private projectIds() {
        return _.map(this.projectExplorerService.projects, "model.id");
    }

    metadata(term: string, page: number = null, pageSize: number = null): ng.IPromise<ISearchMetadata> {

        const deferred = this.$q.defer();

        const request: ng.IRequestConfig = {
            method: "POST",
            url: this.getMetadataUrl(page, pageSize),
            params: {},
            data: {
                "Query": term,
                "ProjectIds": this.projectIds()
            }
        };

        this.$http(request).then(
            (result) => {
                deferred.resolve(result.data);
            },
            (error) => {
                deferred.reject(error);
            }
        );

        return deferred.promise;
    }

    search(term: string, eventSource: string, page: number = null, pageSize: number = null): ng.IPromise<ISearchResult> {
        this.$log.debug(`searching server for "${term}"`);


        const deferred = this.$q.defer();

        const request: ng.IRequestConfig = {
            method: "POST",
            url: this.getSearchUrl(page, pageSize),
            params: {},
            data: {
                "Query": term,
                "ProjectIds": this.projectIds()
            }
        };

        /*this.analytics.trackEvent("search", "quick search", eventSource, term, {
            projectIds: this.projectIds(),
            page: page,
            pageSize: pageSize
        });*/

        this.$http(request).then((result) => {
                let p = [];
                _.each((<ISearchResult>result.data).items, (item) => {
                    if (item.isSubartifact) {
                        p.push(this.metadataService.getSubArtifactItemType(item.projectId, item.itemTypeId).then((itemType: IItemType) => {
                            return this.extendItem(item, itemType);
                        }));
                    }
                    else {
                        p.push(this.metadataService.getArtifactItemType(item.projectId, item.itemTypeId).then((itemType: IItemType) => {
                            return this.extendItem(item, itemType);
                        }));
                    }
                });
                this.$q.all(p).then(() => {
                    deferred.resolve(result.data);
                });
            },
            (error) => {
                deferred.reject(error);
            }
        );

        return deferred.promise;
    }

    private extendItem(item: ISearchItem, itemType: IItemType) {
        if (!itemType) {
            return item;
        }
        return _.extend(item, {
            iconImageId: itemType.iconImageId,
            predefinedType: itemType.predefinedType,
            artifactClass: "icon-" + (_.kebabCase(Models.ItemTypePredefined[itemType.predefinedType] || "document"))
        });
    }
}
