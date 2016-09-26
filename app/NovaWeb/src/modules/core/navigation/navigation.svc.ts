export interface INavigationOptions {
    enableTracking: boolean;
}

export interface INavigationService {
    navigateToMain(): ng.IPromise<any>;
    navigateToArtifact(id: number, options?: INavigationOptions): ng.IPromise<any>;
}

export class NavigationService implements INavigationService {
    public static $inject: [string] = [
        "$state"
    ];

    constructor(
        private $state: ng.ui.IStateService
    ) {
    }

    public navigateToMain(): ng.IPromise<any> {
        const state: string = "main";
        return this.$state.go(state);
    }

    public navigateToArtifact(id: number, options?: INavigationOptions): ng.IPromise<any> {
        const state = "main.artifact";
        const parameters = { id: id };
        // Disables the inheritance of optional url parameters (such as "path")
        let stateOptions: ng.ui.IStateOptions = <ng.ui.IStateOptions>{ inherit: false };

        if (options && options.enableTracking) {
            this.updatePathParameter(parameters);
        }

        return this.$state.go(state, parameters, stateOptions);
    }

    private updatePathParameter(parameters: any) {
        const sourceArtifactId = this.$state.params["id"];

        if (!sourceArtifactId) {
            return;
        }

        const path = this.$state.params["path"];

        if (!path) {
            parameters["path"] = `${sourceArtifactId}`;
        } else {
            const delimiter = ",";
            parameters["path"] = `${path}${delimiter}${sourceArtifactId}`;
        }
    }
}