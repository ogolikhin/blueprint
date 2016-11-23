import {ISearchMetadata, ISearchResult} from "./quickSearchService";

export class QuickSearchServiceMock {
    static $inject = [
        "$q"
    ];
    metadataReturned: ISearchMetadata;
    constructor(private $q: ng.IQService) {
        this.metadataReturned = {items: [], pageSize: 10, totalCount: 0, totalPages: 0};
    }

    searchTerm;

    canSearch(): boolean {
        return true;
    }

    search(term: string): ng.IPromise<ISearchResult> {
        const deferred = this.$q.defer();
        deferred.resolve(null);

        return deferred.promise;
    }

    metadata(term: string, page: number = null, pageSize: number = null): ng.IPromise<ISearchMetadata> {
        const deferred = this.$q.defer();
        deferred.resolve(this.metadataReturned);
        return deferred.promise;
    }
}
