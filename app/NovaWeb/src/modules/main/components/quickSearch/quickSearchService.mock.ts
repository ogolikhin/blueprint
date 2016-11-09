import * as SearchModels from "./models/model";

export class QuickSearchServiceMock {
    static $inject = [
        "$q"
    ];
    metadataReturnedTotalCount: number;
    constructor(private $q: ng.IQService) {
        this.metadataReturnedTotalCount = 0;
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

    metadata(term: string, page: number = null, pageSize: number = null): ng.IPromise<SearchModels.ISearchMetadata> {
        const deferred = this.$q.defer();
        deferred.resolve({totalCount: this.metadataReturnedTotalCount});
        return deferred.promise;
    }
}
