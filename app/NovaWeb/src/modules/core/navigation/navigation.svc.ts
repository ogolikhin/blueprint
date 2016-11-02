import * as angular from "angular";
import {INavigationState} from "./navigation-state";

export interface INavigationService {
    getNavigationState(): INavigationState;
    navigateToMain(redirect?: boolean): ng.IPromise<any>;
    navigateTo(options: INavigationOptions): ng.IPromise<any>;
    navigateBack(pathIndex?: number): ng.IPromise<any>;
    reloadParentState();
}

export interface INavigationOptions {
    id: number;
    version?: number;
    redirect?: boolean;
    enableTracking?: boolean;
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

    public reloadParentState() {
        const immediateParentState = this.$state.$current["parent"];
        if (immediateParentState) {
            // <any> due to lack of updated types definition
            (<any>this.$state).reload(immediateParentState.name);
        } else {
            this.$state.reload();
        }
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

    public navigateTo(options: INavigationOptions): ng.IPromise<any> {
        options.redirect = options.redirect || false;
        options.enableTracking = options.enableTracking || false;

        //id: number, redirect: boolean = false, enableTracking: boolean = false
        const deferred: ng.IDeferred<any> = this.$q.defer();
        const currentState = this.getNavigationState();
        const validationError: Error = this.validateArtifactNavigation(options, currentState);

        if (!!validationError) {
            deferred.reject(validationError);
            return deferred.promise;
        }

        const stateOptions: ng.ui.IStateOptions = {
            location: options.redirect ? "replace" : true,
            inherit: false
        };

        const parameters = this.createParameters(options, currentState);

        return this.navigateToArtifactInternal(parameters, stateOptions);
    }

    private createParameters(options: INavigationOptions, currentState: INavigationState) {
        const parameters = { 
            id: options.id
        };

        if (options.enableTracking && currentState.id) {
            if (!currentState.path || currentState.path.length === 0) {
                parameters["path"] = `${currentState.id}`;
            } else {
                parameters["path"] = `${currentState.path.join(this.delimiter)}${this.delimiter}${currentState.id}`;
            }
        }

        return parameters;
    }

    public navigateBack(pathIndex?: number): ng.IPromise<any> {
        const deferred: ng.IDeferred<any> = this.$q.defer();
        const path: number[] = this.getNavigationState().path;
        const validationError: Error = this.validateBackNavigation(path, pathIndex);

        if (!!validationError) {
            deferred.reject(validationError);
            return deferred.promise;
        }

        const parameters = this.createNavigateBackParameters(path, pathIndex);
        return this.navigateToArtifactInternal(parameters);
    }

    private createNavigateBackParameters(path: number[], pathIndex?: number) {
        if (pathIndex == null) {
            // if path index is not defined set it to the index of the last element in navigation path
            pathIndex = path.length - 1;
        }
        const parameters = {
                id: path[pathIndex]
        };

        const newPath = path.slice(0, pathIndex).join(this.delimiter);

        if (newPath) {
            parameters["path"] = newPath;
        }
        return parameters;
    }

    private navigateToArtifactInternal(parameters: any, stateOptions?: ng.ui.IStateOptions): ng.IPromise<any> {
        const state = "main.item";
        // Disables the inheritance of optional url parameters (such as "path")
        const options: ng.ui.IStateOptions = stateOptions || { inherit: false };

        return this.$state.go(state, parameters, options);
    }

    private validateArtifactNavigation(options: INavigationOptions, navigationState: INavigationState): Error {
        if (options.id === navigationState.id && (!navigationState.path || navigationState.path.length === 0)) {
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
