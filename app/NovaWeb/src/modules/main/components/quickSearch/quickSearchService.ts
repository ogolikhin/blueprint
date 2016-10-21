export class QuickSearchService {
    static $inject = [
        "$q",
        "$http"
    ];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService) {
    }

    searchTerm;

    search(term) {
        const MOCK_RESULTS = require("./quickSearch.mock.ts");


        const deferred = this.$q.defer();

        deferred.resolve(MOCK_RESULTS);
        return deferred.promise;
    }
}
