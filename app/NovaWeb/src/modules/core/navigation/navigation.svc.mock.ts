import {INavigationState, INavigationService} from "./";

export class NavigationServiceMock implements INavigationService {
    public static $inject: string[] = [
        "$q"
    ];

    constructor(private $q: ng.IQService) {
    }

    public reloadParentState() {
        ;
    }

    public getNavigationState(): INavigationState {
        return null;
    }

    public navigateToMain(redirect?: boolean): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }

    public navigateTo(id: number, redirect?: boolean, enableTracking?: boolean): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }

    public navigateBack(pathIndex: number): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }
}
