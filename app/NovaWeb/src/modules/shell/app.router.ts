import {ISession} from "./login/session.svc";
import {ISelectionManager} from "../managers";
import {INavigationService} from "../commonModule/navigation/navigation.service";
import {ILicenseService} from "./license/license.svc";
import {IClipboardService} from "../editorsModule/bp-process/services/clipboard.svc";
import {IMessageService} from "../main/components/messages/message.svc";
import {MessageType} from "../main/components/messages/message";
import {IProjectExplorerService} from "../main/components/bp-explorer/project-explorer.service";

export class AppRoutes {

    public static $inject = [
        "$stateProvider",
        "$urlRouterProvider",
        "$urlMatcherFactoryProvider"
    ];

    constructor($stateProvider: ng.ui.IStateProvider,
                $urlRouterProvider: ng.ui.IUrlRouterProvider,
                $urlMatcherFactoryProvider: any) {

        $urlMatcherFactoryProvider.caseInsensitive(true);

        // parses 'path' parameter into an array of IDs
        $urlMatcherFactoryProvider.type("navpath", {
            encode: (item: string[]): string => {
                return _.isArray(item) ? item.join(",") : undefined;
            },
            decode: (item: string): string[] => {
                return _.isString(item) ? item.split(",") : [];
            },
            equals: (val1, val2) => {
                return _.isEqual(val1, val2);
            },
            is: (item) => {
                return _.isObject(item);
            }
        });

        // pass through / to main state
        $urlRouterProvider.when("", "/main");

        // unrecognized routes go to error state
        $urlRouterProvider.otherwise("/error");

        // register states with the router
        $stateProvider
            .state("main", {
                url: "/main",
                template: "<bp-main-view></bp-main-view>",
                controller: MainStateController,
                resolve: {
                    authenticated: ["session", "isServerLicenseValid", "$q", (session: ISession, isServerLicenseValid: boolean, $q: ng.IQService) => {
                        return isServerLicenseValid ? session.ensureAuthenticated() : $q.when(false);
                    }],
                    isServerLicenseValid: ["licenseService", (licenseService: ILicenseService) => {
                        return licenseService.getServerLicenseValidity();
                    }]
                }
            })
            .state("logout", {
                controller: LogoutStateController,
                resolve: {
                    saved: ["selectionManager", (sm: ISelectionManager) => { return sm.autosave(); }]
                }
            })
            .state("error", {
                url: "/error",
                template: require("./error/error-page.html")
            })
            .state("licenseError", {
                url: "/invalidLicense",
                template: require("./error/error-license.html")
            });
    }
}

export class MainStateController {
    public static $inject = [
        "$rootScope",
        "$state",
        "$log",
        "selectionManager",
        "isServerLicenseValid",
        "messageService"
    ];

    constructor(private $rootScope: ng.IRootScopeService,
                private $state: ng.ui.IStateService,
                private $log: ng.ILogService,
                private selectionManager: ISelectionManager,
                private isServerLicenseValid: boolean,
                private messageService: IMessageService) {

        $rootScope.$on("$stateChangeStart", this.stateChangeStart);
        $rootScope.$on("$stateChangeSuccess", this.onStateChangeSuccess);

        if (!isServerLicenseValid) {
            $state.go("licenseError");
        }
    }

    private isLeavingState(stateName: string, from: string, to: string): boolean {
        return from.indexOf(stateName) > -1 && to.indexOf(stateName) === -1;
    }

    private onStateChangeSuccess = (event: ng.IAngularEvent, toState: ng.ui.IState, toParams, fromState: ng.ui.IState, fromParams) => {
        if (this.isLeavingState("main.item", fromState.name, toState.name)) {
            this.$log.info("Leaving artifact state, clearing selection...");
            this.selectionManager.clearAll();
        }

        if (["logout", "error", "licenseError"].indexOf(toState.name) !== -1) {
            this.messageService.clearMessages(true);
        } else if (toState.name === "main") { // initial state with no project open
            this.messageService.clearMessages(false, [MessageType.Deleted]);
        } else {
            this.messageService.clearMessages();
        }
    };

    private stateChangeStart = (event: ng.IAngularEvent, toState: ng.ui.IState, toParams: any, fromState: ng.ui.IState, fromParams) => {
        if (!this.isServerLicenseValid) {
            //Prevent leaving the license error state.
            if (toState.name !== "licenseError") {
                event.preventDefault();
                this.$state.go("licenseError");
            }
        }
    };
}

export class LogoutStateController {
    public static $inject = [
        "$log",
        "session",
        "projectExplorerService",
        "navigationService",
        "clipboardService"
    ];

    constructor(private $log: ng.ILogService,
                private session: ISession,
                private projectExplorerService: IProjectExplorerService,
                private navigation: INavigationService,
                private clipboardService: IClipboardService) {

        this.session.logout().then(() => {
            this.navigation.navigateToMain(true).finally(() => {
                this.projectExplorerService.removeAll();
                this.clipboardService.clearData();
            });
        });
    }
}
