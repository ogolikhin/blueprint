import {INavigationState, INavigationService, INavigationOptions} from "./";

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
        const deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }

    public navigateTo(options: INavigationOptions): ng.IPromise<any> {
        options.redirect = options.redirect || false;
        options.enableTracking = options.enableTracking || false;
        const deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }

    public navigateBack(pathIndex: number): ng.IPromise<any> {
        const deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }
}
