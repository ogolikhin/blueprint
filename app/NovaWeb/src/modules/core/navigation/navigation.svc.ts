export interface INavigationState {
    id?: number;
    path?: number[];
}

export interface INavigationOptions {
}

export class ForwardNavigationOptions implements INavigationOptions {
    enableTracking: boolean;
}

export class BackNavigationOptions implements INavigationOptions {
    index: number;
}

export interface INavigationService {
    getNavigationState(): INavigationState;
    navigateToMain(): ng.IPromise<any>;
    navigateToArtifact(id: number, options?: INavigationOptions): ng.IPromise<any>;
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

    public navigateToArtifact(id: number, options?: INavigationOptions): ng.IPromise<any> {
        const state = "main.artifact";
        const parameters = { id: id };
        // Disables the inheritance of optional url parameters (such as "path")
        const stateOptions: ng.ui.IStateOptions = <ng.ui.IStateOptions>{ inherit: false };

        if (options) {
            this.updatePathParameter(options, parameters);
        }

        return this.$state.go(state, parameters, stateOptions);
    }

    private updatePathParameter(options: INavigationOptions, parameters: any) {
        let currentState = this.getNavigationState();

        if (!currentState.id) {
            return;
        }

        let forwardOptions = <ForwardNavigationOptions>options;
        let backOptions = <BackNavigationOptions>options;

        if (!currentState.path || currentState.path.length === 0) {
            if (forwardOptions && forwardOptions.enableTracking) {
                parameters["path"] = `${currentState.id}`;
            }
        } else {
            if (forwardOptions && forwardOptions.enableTracking) {
                parameters["path"] = `${currentState.path.join(this.delimiter)}${this.delimiter}${currentState.id}`;
            }

            if (backOptions && backOptions.index > 0 && backOptions.index < currentState.path.length) {
                parameters["path"] = `${currentState.path.slice(0, backOptions.index).join(this.delimiter)}`;
            }
        }
    }
}