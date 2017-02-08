appRun.$inject = [
    "$rootScope",
    "$log",
    "$window",
    "$state"
];

export function appRun($rootScope: ng.IRootScopeService,
                       $log: ng.ILogService,
                       $window: ng.IWindowService,
                       $state: ng.ui.IStateService) {

    $rootScope.$on("$stateChangeStart", onStateChangeStart);
    $rootScope.$on("$stateChangeSuccess", onStateChangeSuccess);

    function onStateChangeStart(event: ng.IAngularEvent, toState: ng.ui.IState, toParams: any, fromState: ng.ui.IState, fromParams) {
        $log.debug(
            "state transition: %c" + fromState.name + "%c -> %c" + toState.name + "%c " + JSON.stringify(toParams)
            , "color: blue", "color: black", "color: blue", "color: black"
        );
    }

    function onStateChangeSuccess(event: ng.IAngularEvent, toState: ng.ui.IState, toParams, fromState: ng.ui.IState, fromParams) {
        const resolves = $state.$current.locals.globals as any;
        updateAppTitle(resolves.title);
    }

    function updateAppTitle(title: string) {
        if (title) {
            $window.document.title = title;
        } else {
            $window.document.title = "Storyteller";
        }
    }
}
