import {INavigationService, INavigationParams} from "./navigation.svc";
import {INavigationState} from "./navigation-state";
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

    public navigateTo(params: INavigationParams): ng.IPromise<any> {
        params.redirect = params.redirect || false;
        params.enableTracking = params.enableTracking || false;
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
