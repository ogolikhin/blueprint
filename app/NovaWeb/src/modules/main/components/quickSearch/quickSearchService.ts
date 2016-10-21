export class QuickSearchService {
    static $inject = [
        "$q",
        "$http",
        "$timeout"
    ];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $timeout: ng.ITimeoutService) {
    }

    searchTerm;

    search(term) {
        const MOCK_RESULTS = require("./quickSearch.mock.ts");
        const deferred = this.$q.defer();

        this.$timeout(() => {
            deferred.resolve(MOCK_RESULTS);
        }, 1000);
        return deferred.promise;
    }
}
