import {IProjectManager} from "../../../managers/project-manager/project-manager";
export class QuickSearchService {
    static $inject = [
        "$q",
        "$http",
        "$timeout",
        "$log",
        "projectManager"
    ];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $timeout: ng.ITimeoutService,
                private $log: ng.ILogService,
                private projectManager: IProjectManager) {
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
                deferred.resolve(result.data);
            },
            (error) => {
                deferred.reject(error);
            }
        );

        return deferred.promise;
    }
}
