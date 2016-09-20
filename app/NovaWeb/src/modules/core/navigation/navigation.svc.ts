export interface INavigationContext {
    previousItemId: number;
}

export interface INavigationService {
    navigateToDefault(): ng.IPromise<any>;
    navigateToItem(id: number, context?: INavigationContext): ng.IPromise<any>;
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

    public navigateToDefault(): ng.IPromise<any> {
        return this.$state.go(this._defaultState);
    }

    public navigateToItem(id: number, context?: INavigationContext): ng.IPromise<any> {
        const parameters = { id: id };
        let options: ng.ui.IStateOptions;

        if (context) {
            if (context.previousItemId) {
                const pathName = "path";
                const path = this.$state.params[pathName];

                if (!path) {
                    parameters[pathName] = `${context.previousItemId}`;
                } else {
                    parameters[pathName] = `${path},${context.previousItemId}`;
                }
            }
        } else {
            options = { inherit: false };
        }

        return this.$state.go(this._artifactState, parameters, options);
    }
}