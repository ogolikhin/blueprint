module Shell {
    export interface IBeforeUnload {
        blockStateChangeIfRequired(event: ng.IAngularEvent, toState, toParams);
    }
}
