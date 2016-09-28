import {INavigationState} from "./navigation-state";

export interface INavigationService {
    getNavigationState(): INavigationState;
    navigateToMain(): ng.IPromise<any>;
    navigateToArtifact(id: number, enableTracking?: boolean): ng.IPromise<any>;
    navigateBack(pathIndex?: number): ng.IPromise<any>;
}

export class NavigationService implements INavigationService {
    private delimiter: string = ",";

    public static $inject: [string] = [
        "$q",
        "$state"
    ];

    constructor(
        private $q: ng.IQService,
        private $state: ng.ui.IStateService
    ) {
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

    public navigateToMain(): ng.IPromise<any> {
        const state: string = "main";
        return this.$state.go(state);
    }

    public navigateToArtifact(id: number, enableTracking: boolean = false): ng.IPromise<any> {
        const getParameters = () => {
            const parameters = { id: id };

            const currentState = this.getNavigationState();

            if (enableTracking && currentState.id) {
                if (!currentState.path || currentState.path.length === 0) {
                    parameters["path"] = `${currentState.id}`;
                } else {
                    parameters["path"] = `${currentState.path.join(this.delimiter)}${this.delimiter}${currentState.id}`;
                }
            }

            return parameters;
        };

        return this._navigateToArtifact(getParameters);
    }

    public navigateBack(pathIndex?: number): ng.IPromise<any> {
        const deferred: ng.IDeferred<any> = this.$q.defer();
        const path: number[] = this.getNavigationState().path;
        const validationError: string = this.validateBackNavigation(path, pathIndex);

        if (!!validationError) {
            deferred.reject(new Error(validationError));
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

        return this._navigateToArtifact(getParameters);
    }

    private _navigateToArtifact(getParameters: () => any): ng.IPromise<any> {
        const state = "main.artifact";
        const parameters = getParameters();
        // Disables the inheritance of optional url parameters (such as "path")
        const stateOptions: ng.ui.IStateOptions = <ng.ui.IStateOptions>{ inherit: false };

        return this.$state.go(state, parameters, stateOptions);
    }

    private validateBackNavigation(path: number[], pathIndex: number): string {
        if (!path || path.length === 0) {
            return `Unable to navigate back, no navigation history found.`;
        }

        if (!!pathIndex) {
            if (pathIndex < 0) {
                return `Unable to navigate back, pathIndex is out of range: ${pathIndex}.`;
            }

            if (pathIndex >= path.length) {
                return `Unable to navigate back, pathIndex is out of range: ${pathIndex}.`;
            }
        }

        return null;
    }
}