import {IMessageService} from "../main/components/messages/message.svc";
import {MessageType} from "../main/components/messages/message";

appRun.$inject = [
    "$rootScope",
    "$log",
    "$window",
    "$state",
    "messageService"
];

export function appRun($rootScope: ng.IRootScopeService,
                       $log: ng.ILogService,
                       $window: ng.IWindowService,
                       $state: ng.ui.IStateService,
                       messageService: IMessageService) {

    $rootScope.$on("$stateChangeStart", onStateChangeStart);
    $rootScope.$on("$stateChangeSuccess", onStateChangeSuccess);

    function onStateChangeStart(event: ng.IAngularEvent, toState: ng.ui.IState, toParams: any, fromState: ng.ui.IState, fromParams) {
        $log.info(
            "state transition: %c" + fromState.name + "%c -> %c" + toState.name + "%c " + JSON.stringify(toParams)
            , "color: blue", "color: black", "color: blue", "color: black"
        );
    }

    function onStateChangeSuccess(event: ng.IAngularEvent, toState: ng.ui.IState, toParams, fromState: ng.ui.IState, fromParams) {
        const resolves = $state.$current.locals.globals as any;
        updateAppTitle(resolves.title);

        if (isLeavingState("main.item", fromState.name, toState.name)) {
            this.$log.info("Leaving artifact state, clearing selection...");
            this.selectionManager.clearAll();
        }

        if (["logout", "error", "licenseError"].indexOf(toState.name) !== -1) {
            messageService.clearMessages(true);
        } else if (toState.name === "main") { // initial state with no project open
            messageService.clearMessages(false, [MessageType.Deleted]);
        } else {
            messageService.clearMessages();
        }
    }

    function isLeavingState(stateName: string, from: string, to: string): boolean {
        return from.indexOf(stateName) > -1 && to.indexOf(stateName) === -1;
    }

    function updateAppTitle(title: string) {
        if (title) {
            $window.document.title = title;
        } else {
            $window.document.title = "Storyteller";
        }
    }
}
