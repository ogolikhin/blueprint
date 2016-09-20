import {INavigationContext, INavigationService} from "./navigation.svc";

export class NavigationServiceMock implements INavigationService {
    public static $inject: string[] = [
        "$q"
    ];

    constructor(
        private $q: ng.IQService
    ) {
    }

    public navigateToDefault(): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }

    public navigateToItem(id: number, context?: INavigationContext): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }
}