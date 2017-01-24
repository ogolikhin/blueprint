export interface INavigationPathItem {
    id: number;
    version?: number;
}

export interface INavigationState {
    id?: number;
    version?: number;
    path?: INavigationPathItem[];
}

export interface INavigationService {
    getNavigationState(): INavigationState;
    navigateToMain(redirect?: boolean): ng.IPromise<any>;
    navigateTo(params: INavigationParams): ng.IPromise<any>;
    navigateBack(pathIndex?: number): ng.IPromise<any>;
    getNavigateBackRouterPath(pathIndex?: number): string[];
    reloadParentState();
    reloadCurrentState();
    navigateToLogout(): ng.IPromise<any>;
}

export interface INavigationParams {
    id: number;
    version?: number;
    redirect?: boolean;
    enableTracking?: boolean;
}

export class NavigationService implements INavigationService {
    private pathItemDelimiter: string = ",";
    private pathVersionDelimiter: string = ":";

    public static $inject: [string] = [
        "$q",
        "$state"
    ];

    constructor(private $q: ng.IQService,
                private $state: ng.ui.IStateService) {
    }

    public reloadCurrentState() {
        const currentState = this.$state.current.name;
        this.$state.go(currentState, {}, {reload: currentState});
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
        const path: INavigationPathItem[] = this.getNavigationPathItems(pathParameter);

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

    public navigateToLogout(): ng.IPromise<any> {
        const state: string = "logout";
        const stateOptions: ng.ui.IStateOptions = {
            location: true
        };

        return this.$state.go(state, {}, stateOptions);
    }

    public navigateTo(params: INavigationParams): ng.IPromise<any> {
        params.redirect = params.redirect || false;
        params.enableTracking = params.enableTracking || false;

        const currentState = this.getNavigationState();

        if (this.isTheSameRoute(params, currentState)) {
            return this.$q.resolve();
        }

        const stateOptions: ng.ui.IStateOptions = {
            location: params.redirect ? "replace" : true,
            inherit: false
        };

        const routerParams = this.createRouterParams(params, currentState);

        return this.navigateToArtifactInternal(routerParams, stateOptions);
    }

    private getNavigationPathItems(path: string[]): INavigationPathItem[] {
        if (!path) {
            return [];
        }

        return path.map(item => {
            const pair: string[] = item.split(this.pathVersionDelimiter);

            return <INavigationPathItem>{
                id: parseInt(pair[0], 10),
                version: pair.length > 1 ? parseInt(pair[1], 10) : undefined
            };
        });
    }

    private getPathItemString(id: number, version?: number): string {
        if (version) {
            return `${id}${this.pathVersionDelimiter}${version}`;
        }

        return `${id}`;
    }

    private getPathString(path: INavigationPathItem[]): string {
        if (!path) {
            return "";
        }

        return path.map(item => this.getPathItemString(item.id, item.version)).join(this.pathItemDelimiter);
    }

    private createRouterParams(params: INavigationParams, currentState: INavigationState) {
        const parameters = {
            id: params.id
        };

        if (_.isFinite(params.version)) {
            parameters["version"] = params.version;
        }

        if (params.enableTracking && currentState.id) {
            let path: string = "";

            if (currentState.path && currentState.path.length > 0) {
                path = `${this.getPathString(currentState.path)}${this.pathItemDelimiter}`;
            }

            parameters["path"] = `${path}${this.getPathItemString(currentState.id, currentState.version)}`;
        }

        return parameters;
    }

    public navigateBack(pathIndex?: number): ng.IPromise<any> {
        const deferred: ng.IDeferred<any> = this.$q.defer();
        const path: INavigationPathItem[] = this.getNavigationState().path;
        const validationError: Error = this.validateBackNavigation(path, pathIndex);

        if (!!validationError) {
            deferred.reject(validationError);
            return deferred.promise;
        }

        const parameters = this.createNavigateBackRouterParams(path, pathIndex);
        return this.navigateToArtifactInternal(parameters);
    }

    private createNavigateBackRouterParams(path: INavigationPathItem[], pathIndex?: number): any {
        if (pathIndex == null) {
            // if path index is not defined set it to the index of the last element in navigation path
            pathIndex = path.length - 1;
        }

        const parameters = {
            id: path[pathIndex].id,
            version: path[pathIndex].version
        };

        const newPath = this.getNavigateBackRouterPath(pathIndex); //this.getPathString(path.slice(0, pathIndex));

        if (newPath) {
            parameters["path"] = newPath;
        }

        return parameters;
    }

    public getNavigateBackRouterPath(pathIndex?: number): string[] {
        const path: INavigationPathItem[] = this.getNavigationState().path;

        if (path && path.length > 0) {
            if (pathIndex == null) {
                // if path index is not defined set it to the index of the last element in navigation path
                pathIndex = path.length - 1;
            }

            const newPath = path.slice(0, pathIndex).map(item => this.getPathItemString(item.id, item.version));
            return newPath.length ? newPath : undefined;
        }

        return undefined;
    }

    private navigateToArtifactInternal(parameters: any, stateOptions?: ng.ui.IStateOptions): ng.IPromise<any> {
        const state = "main.item";
        // Disables the inheritance of optional url parameters (such as "path")
        const options: ng.ui.IStateOptions = stateOptions || {inherit: false};

        return this.$state.go(state, parameters, options);
    }

    private isTheSameRoute(params: INavigationParams, state: INavigationState): boolean {
        return params.id === state.id &&
               params.version === state.version &&
               (!state.path || state.path.length === 0);
    }

    private validateBackNavigation(path: INavigationPathItem[], pathIndex: number): Error {
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