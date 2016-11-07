import {INavigationState} from "./navigation-state";

export interface INavigationService {
    getNavigationState(): INavigationState;
    navigateToMain(redirect?: boolean): ng.IPromise<any>;
    navigateTo(params: INavigationParams): ng.IPromise<any>;
    navigateBack(pathIndex?: number): ng.IPromise<any>;
    reloadParentState();
}

export interface INavigationParams {
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
        const versionParameter = this.$state.params["version"];
        const pathParameter = this.$state.params["path"];

        const id: number = idParameter ? Number(idParameter) : undefined;
        const version: number = versionParameter ? Number(versionParameter) : undefined;
        const path: number[] = pathParameter ? pathParameter.split(this.delimiter).map((element) => Number(element)) : undefined;

        return <INavigationState>{
            id: id,
            version: version,
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

    public navigateTo(params: INavigationParams): ng.IPromise<any> {
        params.redirect = params.redirect || false;
        params.enableTracking = params.enableTracking || false;

        //id: number, redirect: boolean = false, enableTracking: boolean = false
        const deferred: ng.IDeferred<any> = this.$q.defer();
        const currentState = this.getNavigationState();
        const validationError: Error = this.validateArtifactNavigation(params, currentState);

        if (!!validationError) {
            deferred.reject(validationError);
            return deferred.promise;
        }

        const stateOptions: ng.ui.IStateOptions = {
            location: params.redirect ? "replace" : true,
            inherit: false
        };

        const routerParams = this.createRouterParams(params, currentState);

        return this.navigateToArtifactInternal(routerParams, stateOptions);
    }

    private createRouterParams(params: INavigationParams, currentState: INavigationState) {
        const parameters = {
            id: params.id
        };

        if (_.isFinite(params.version)) {
            parameters["version"] = params.version;
        }

        if (params.enableTracking && currentState.id) {
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

        const parameters = this.createNavigateBackRouterParams(path, pathIndex);
        return this.navigateToArtifactInternal(parameters);
    }

    private createNavigateBackRouterParams(path: number[], pathIndex?: number): any {
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
        const options: ng.ui.IStateOptions = stateOptions || {inherit: false};

        return this.$state.go(state, parameters, options);
    }

    private validateArtifactNavigation(params: INavigationParams, state: INavigationState): Error {
        if (params.id === state.id && params.version === state.version &&
            (!state.path || state.path.length === 0)) {
            return new Error(`Unable to navigate to artifact, navigating from the same artifact.`);
        }

        return undefined;
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

        return undefined;
    }
}
