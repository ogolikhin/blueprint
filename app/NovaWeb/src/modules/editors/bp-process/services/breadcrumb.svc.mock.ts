import {IBreadcrumbService, IArtifactReference} from "./breadcrumb.svc";

export class BreadcrumbServiceMock implements IBreadcrumbService {
    public static $inject: string[] = [
        "$q"
    ];

    constructor(
        private $q: ng.IQService
    ) {
    }

    public getReferences(): ng.IPromise<IArtifactReference[]> {
        let deferred = this.$q.defer();
        deferred.resolve([]);

        return deferred.promise;
    }
}