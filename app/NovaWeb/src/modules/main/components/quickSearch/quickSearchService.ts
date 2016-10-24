import {IProjectManager} from "../../../managers/project-manager/project-manager";
import {IMetaDataService} from "../../../managers/artifact-manager/metadata/metadata.svc";
import {IItemType} from "../../models/models";
import {Helper} from "../../../shared/utils/helper";
import {Models} from "../../models";

export class QuickSearchService {
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

    searchTerm;

    canSearch(): boolean {
        return !(this.projectManager.projectCollection.getValue().map(project => project.id).length > 0);
    }

    search(term: string) {
        this.$log.debug("seraching server for ", term);

        const MOCK_RESULTS = require("./quickSearch.mock.ts");

        const deferred = this.$q.defer();
        const request: ng.IRequestConfig = {
            method: "POST",
            url: `/svc/searchservice/FullTextSearch`,
            params: {},
            data: {
                "Query": term,
                "ProjectIds": this.projectManager.projectCollection.getValue().map(project => project.id)
            }
        };

        this.$http(request).then((result) => {
                let p = [];
                _.each((<any>result.data).fullTextSearchItems, (item) => {
                    p.push(this.metadataService.getArtifactItemType(item.projectId, item.itemTypeId).then((itemType: IItemType) => {
                        return _.extend(item, {
                            iconImageId: itemType.iconImageId,
                            predefinedType: itemType.predefinedType,
                            artifactClass : "icon-" + (Helper.toDashCase(Models.ItemTypePredefined[itemType.predefinedType] || "document"))
                        });
                    }));
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
}
