import {INavigationContext, INavigationService} from "./navigation.svc";

export class NavigationServiceMock implements INavigationService {
    public static $inject: string[] = [
        "$q"
    ];

    constructor(
        private $q: ng.IQService
    ) {
    }

    public navigateToMain(): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }

    public navigateToArtifact(id: number, context?: INavigationContext): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }
}