export interface INavigationContext {
    sourceArtifactId: number;
}

export interface INavigationService {
    navigateToMain(): ng.IPromise<any>;
    navigateToArtifact(id: number, context?: INavigationContext): ng.IPromise<any>;
}

export class NavigationService implements INavigationService {
    private _defaultState: string = "main";
    private _artifactState: string = "main.artifact";

    public static $inject: [string] = [
        "$state"
    ];

    constructor(
        private $state: ng.ui.IStateService
    ) {
    }

    public navigateToMain(): ng.IPromise<any> {
        return this.$state.go(this._defaultState);
    }

    public navigateToArtifact(id: number, context?: INavigationContext): ng.IPromise<any> {
        const parameters = { id: id };
        let options: ng.ui.IStateOptions;

        // Disables the inheritance of url parameters (such as "path")
        options = <ng.ui.IStateOptions>{ inherit: false };

        if (context) {
            this.populatePath(id, context.sourceArtifactId, parameters);
        }

        return this.$state.go(this._artifactState, parameters, options);
    }

    private populatePath(id: number, sourceArtifactId: number, parameters: any) {
        if (!sourceArtifactId || sourceArtifactId === id) {
            return;
        }

        const parameterName = "path";
        const delimiter = ",";

        const path = this.$state.params[parameterName];

        if (!path) {
            parameters[parameterName] = `${sourceArtifactId}`;
        } else {
            const pathElements = path.split(delimiter);

            if (pathElements.length > 0) {
                let lastArtifactId = Number(pathElements[pathElements.length - 1]);

                if (lastArtifactId !== sourceArtifactId) {
                    parameters[parameterName] = `${path}${delimiter}${sourceArtifactId}`;
                }
            }
        }
    }
}