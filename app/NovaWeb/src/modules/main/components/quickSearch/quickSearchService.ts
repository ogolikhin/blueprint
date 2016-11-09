import { IProjectManager } from "../../../managers/project-manager/project-manager";
import { IMetaDataService } from "../../../managers/artifact-manager/metadata/metadata.svc";
import { IItemType } from "../../models/models";
import { Models } from "../../models";
import * as SearchModels from "./models/model";

export interface IQuickSearchService {
    search(term: string, page?: number, pageSize?: number): ng.IPromise<SearchModels.ISearchResult>;
    metadata(term: string, page?: number, pageSize?: number): ng.IPromise<SearchModels.ISearchMetadata>;
    searchTerm: string;
    canSearch(): boolean;
}

export class QuickSearchService implements IQuickSearchService {
    static $inject = [
        "$q",
        "$http",
        "$timeout",
        "$log",
        "projectManager",
        "metadataService"

    ];

    constructor(private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $timeout: ng.ITimeoutService,
        private $log: ng.ILogService,
        private projectManager: IProjectManager,
        private metadataService: IMetaDataService) {
    }

    searchTerm: string;

    canSearch(): boolean {
        return !(this.projectManager.projectCollection.getValue().map(project => project.id).length > 0);
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

    metadata(term: string, page: number = null, pageSize: number = null): ng.IPromise<SearchModels.ISearchMetadata> {

        const deferred = this.$q.defer();
        const request: ng.IRequestConfig = {
            method: "POST",
            url: this.getMetadataUrl(page, pageSize),
            params: {},
            data: {
                "Query": term,
                "ProjectIds": this.projectManager.projectCollection.getValue().map(project => project.id)
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

    search(term: string, page: number = null, pageSize: number = null): ng.IPromise<SearchModels.ISearchResult> {
        this.$log.debug(`searching server for "${term}"`);

        //const MOCK_RESULTS = require("./quickSearch.mock.ts");

        const deferred = this.$q.defer();
        const request: ng.IRequestConfig = {
            method: "POST",
            url: this.getSearchUrl(page, pageSize),
            params: {},
            data: {
                "Query": term,
                "ProjectIds": this.projectManager.projectCollection.getValue().map(project => project.id)
            }
        };

        this.$http(request).then((result) => {
            let p = [];
            _.each((<SearchModels.ISearchResult>result.data).items, (item) => {
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

    private extendItem(item: SearchModels.ISearchItem, itemType: IItemType) {
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
