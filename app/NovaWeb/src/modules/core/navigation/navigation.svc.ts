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

        if (context) {
            if (context.sourceArtifactId) {
                const pathName = "path";
                const path = this.$state.params[pathName];

                if (!path) {
                    parameters[pathName] = `${context.sourceArtifactId}`;
                } else {
                    parameters[pathName] = `${path},${context.sourceArtifactId}`;
                }
            }
        } else {
            options = { inherit: false };
        }

        return this.$state.go(this._artifactState, parameters, options);
    }
}