import * as SearchModels from "./models/model";

export class QuickSearchServiceMock {
    static $inject = [
        "$q"
    ];

    constructor(private $q: ng.IQService) {
    }

    searchTerm;

    canSearch(): boolean {
        return true;
    }

    search(term: string): ng.IPromise<SearchModels.ISearchResult> {
        const deferred = this.$q.defer();
        deferred.resolve(null);

        return deferred.promise;
    }
}
