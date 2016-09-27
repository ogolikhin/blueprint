import {INavigationState} from "./navigation-state";
import {ForwardNavigationOptions, BackNavigationOptions} from "./navigation-options";

export interface INavigationService {
    getNavigationState(): INavigationState;
    navigateToMain(): ng.IPromise<any>;
    navigateToArtifact(id: number, options?: ForwardNavigationOptions | BackNavigationOptions): ng.IPromise<any>;
}

export class NavigationService implements INavigationService {
    private delimiter: string = ",";

    public static $inject: [string] = [
        "$state"
    ];

    constructor(
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

    public navigateToArtifact(id: number, options?: ForwardNavigationOptions | BackNavigationOptions): ng.IPromise<any> {
        const state = "main.artifact";
        const parameters = { id: id };
        // Disables the inheritance of optional url parameters (such as "path")
        const stateOptions: ng.ui.IStateOptions = <ng.ui.IStateOptions>{ inherit: false };

        if (options) {
            this.updatePathParameter(options, parameters);
        }

        return this.$state.go(state, parameters, stateOptions);
    }

    private updatePathParameter(options: ForwardNavigationOptions | BackNavigationOptions, parameters: any) {
        let currentState = this.getNavigationState();

        if (!currentState.id) {
            return;
        }

        if (!currentState.path || currentState.path.length === 0) {
            if (options instanceof ForwardNavigationOptions && options.enableTracking) {
                parameters["path"] = `${currentState.id}`;
            }
        } else {
            if (options instanceof ForwardNavigationOptions && options.enableTracking) {
                parameters["path"] = `${currentState.path.join(this.delimiter)}${this.delimiter}${currentState.id}`;
            }

            if (options instanceof BackNavigationOptions && options.pathIndex > 0 && options.pathIndex < currentState.path.length) {
                parameters["path"] = `${currentState.path.slice(0, options.pathIndex).join(this.delimiter)}`;
            }
        }
    }
}