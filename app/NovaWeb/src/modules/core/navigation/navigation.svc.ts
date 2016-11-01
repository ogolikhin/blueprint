import * as angular from "angular";
import {INavigationState} from "./navigation-state";

export interface INavigationService {
    getNavigationState(): INavigationState;
    navigateToMain(redirect?: boolean): ng.IPromise<any>;
    navigateTo(id: number, redirect?: boolean, enableTracking?: boolean): ng.IPromise<any>;
    navigateBack(pathIndex?: number): ng.IPromise<any>;
}

export class NavigationService implements INavigationService {
    private delimiter: string = ",";

    public static $inject: [string] = [
        "$q",
        "$state"
    ];

    constructor(private $q: ng.IQService,
                private $state: ng.ui.IStateService) {
    }

    public getNavigationState(): INavigationState {
        const idParameter = this.$state.params["id"];
        const pathParameter = this.$state.params["path"];

        const id: number = idParameter ? Number(idParameter) : null;
        const path: number[] = pathParameter ? pathParameter.split(this.delimiter).map((element) => Number(element)) : null;

        return <INavigationState>{
            id: id,
            path: path
        };
    }

    public navigateToMain(redirect: boolean = false): ng.IPromise<any> {
        const state: string = "main";
        const stateOptions: ng.ui.IStateOptions = {
            location: redirect ? "replace" : true
        };

        return this.$state.go(state, {}, stateOptions);
    }

    public navigateTo(id: number, redirect: boolean = false, enableTracking: boolean = false): ng.IPromise<any> {
        const deferred: ng.IDeferred<any> = this.$q.defer();
        const currentState = this.getNavigationState();
        const validationError: Error = this.validateArtifactNavigation(id, currentState);

        if (!!validationError) {
            deferred.reject(validationError);
            return deferred.promise;
        }

        const getParameters = () => {
            const parameters = {id: id};

            if (enableTracking && currentState.id) {
                if (!currentState.path || currentState.path.length === 0) {
                    parameters["path"] = `${currentState.id}`;
                } else {
                    parameters["path"] = `${currentState.path.join(this.delimiter)}${this.delimiter}${currentState.id}`;
                }
            }

            return parameters;
        };

        const stateOptions: ng.ui.IStateOptions = {
            location: redirect ? "replace" : true,
            inherit: false
        };

        return this.navigateToArtifactInternal(getParameters, stateOptions);
    }

    public navigateBack(pathIndex?: number): ng.IPromise<any> {
        const deferred: ng.IDeferred<any> = this.$q.defer();
        const path: number[] = this.getNavigationState().path;
        const validationError: Error = this.validateBackNavigation(path, pathIndex);

        if (!!validationError) {
            deferred.reject(validationError);
            return deferred.promise;
        }

        if (pathIndex == null) {
            // if path index is not defined set it to the index of the last element in navigation path
            pathIndex = path.length - 1;
        }

        const getParameters = () => {
            const parameters = {
                id: path[pathIndex]
            };

            const newPath = path.slice(0, pathIndex).join(this.delimiter);

            if (newPath) {
                parameters["path"] = newPath;
            }

            return parameters;
        };

        return this.navigateToArtifactInternal(getParameters);
    }

    private navigateToArtifactInternal(getParameters: () => any, stateOptions?: ng.ui.IStateOptions): ng.IPromise<any> {
        const state = "main.item";
        const parameters = getParameters();
        // Disables the inheritance of optional url parameters (such as "path")
        const options: ng.ui.IStateOptions = stateOptions || { inherit: false };

        return this.$state.go(state, parameters, options);
    }

    private validateArtifactNavigation(id: number, navigationState: INavigationState): Error {
        if (id === navigationState.id && (!navigationState.path || navigationState.path.length === 0)) {
            return new Error(`Unable to navigate to artifact, navigating from the same artifact.`);
        }

        return null;
    }

    private validateBackNavigation(path: number[], pathIndex: number): Error {
        if (!path || path.length === 0) {
            return new Error(`Unable to navigate back, no navigation history found.`);
        }

        if (!!pathIndex) {
            if (pathIndex < 0) {
                return new Error(`Unable to navigate back, pathIndex is out of range: ${pathIndex}.`);
            }

            if (pathIndex >= path.length) {
                return new Error(`Unable to navigate back, pathIndex is out of range: ${pathIndex}.`);
            }
        }

        return null;
    }
}
