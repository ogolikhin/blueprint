import {INavigationState, ForwardNavigationOptions, BackNavigationOptions, INavigationService} from "./";

export class NavigationServiceMock implements INavigationService {
    public static $inject: string[] = [
        "$q"
    ];

    constructor(
        private $q: ng.IQService
    ) {
    }

    public getNavigationState(): INavigationState {
        return null;
    }

    public navigateToMain(): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }

    public navigateToArtifact(id: number, options?: ForwardNavigationOptions | BackNavigationOptions): ng.IPromise<any> {
        let deferred = this.$q.defer();
        deferred.resolve();

        return deferred.promise;
    }
}