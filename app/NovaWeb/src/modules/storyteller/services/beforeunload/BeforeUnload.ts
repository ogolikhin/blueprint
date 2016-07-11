module Shell {
    export class OnUnloadConfirmation {
        public message: string;        
    }

    export class OnStateChangeConfirmation {
        public canChangeState: ng.IPromise<boolean>;
        public state: any;
    }

    export class BeforeUnload implements IBeforeUnload {
        public static onBeforeUnloadEvent = "onBeforeUnload";
        public static onBeforeStateChangeEvent = "onBeforeStateChange";
        public static onUnloadEvent = "onUnload";

        public static $inject = ["$rootScope", "$window", "$state", "$urlRouter"];
        constructor(private $rootScope: ng.IRootScopeService, private $window: ng.IWindowService, private $state: ng.ui.IStateService,
                    private $urlRouter: ng.ui.IUrlRouterService) {

            $window.onbeforeunload = (ev: BeforeUnloadEvent) => {
                var confirmation = new OnUnloadConfirmation();
                var event = $rootScope.$broadcast(BeforeUnload.onBeforeUnloadEvent, confirmation);

                if (event.defaultPrevented) {
                    ev.returnValue = confirmation.message;
                    // returning message for android tables...
                    return confirmation.message;
                }
                // cannot use return
            };

            $window.onunload = () => $rootScope.$broadcast(BeforeUnload.onUnloadEvent);
        }

        private canChangeState(toState:any): ng.IPromise<boolean> {
            var confirmation = new OnStateChangeConfirmation();
            confirmation.state = toState;

            var event = this.$rootScope.$broadcast(BeforeUnload.onBeforeStateChangeEvent, confirmation);

            if (event.defaultPrevented) {
                return confirmation.canChangeState;
            }

            return null;
        }

        public blockStateChangeIfRequired(event: ng.IAngularEvent, toState, toParams) {
            var canChangeState = this.canChangeState(toState);
            if (canChangeState) {
                event.preventDefault();

                canChangeState.then(changeState => {
                    if (changeState) {
                        this.$state.go(toState, toParams);
                    } else {
                        this.$urlRouter.sync();
                    }
                });
            }
        }
    }

    angular.module("Shell").service("beforeUnload", BeforeUnload);
}
