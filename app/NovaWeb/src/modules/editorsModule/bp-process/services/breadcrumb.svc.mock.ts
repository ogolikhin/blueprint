import {IBreadcrumbService, IPathItem} from "./breadcrumb.svc";

export class BreadcrumbServiceMock implements IBreadcrumbService {
    public static $inject: string[] = [
        "$q"
    ];

    constructor(private $q: ng.IQService) {
    }

    public getReferences(): ng.IPromise<IPathItem[]> {
        let deferred = this.$q.defer();
        deferred.resolve([]);

        return deferred.promise;
    }
}
