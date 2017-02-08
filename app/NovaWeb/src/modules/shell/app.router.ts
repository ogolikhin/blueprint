import {ISession} from "./login/session.svc";
import {IProjectManager, ISelectionManager} from "../managers";
import {INavigationService} from "../commonModule/navigation/navigation.service";
import {ILicenseService} from "./license/license.svc";
import {IClipboardService} from "../editorsModule/bp-process/services/clipboard.svc";

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
                    authenticated: ["session", "licenseService", "$q", (session: ISession, licenseService: ILicenseService, $q: ng.IQService) => {
                        return licenseService.getServerLicenseValidity().then((isServerLicenseValid) => {
                            return isServerLicenseValid ? session.ensureAuthenticated() : $q.when(false);
                        });
                    }],
                    isServerLicenseValid: ["licenseService", (licenseService: ILicenseService) => {
                        return licenseService.getServerLicenseValidity();
                    }]
                }
            })
            .state("logout", {
                controller: LogoutStateController,
                resolve: {
                    saved: ["selectionManager", (sm: ISelectionManager) => { return sm.autosave(); }]                    }
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
        "isServerLicenseValid",
        "selectionManager"
    ];

    constructor(private $rootScope: ng.IRootScopeService,
                private $state: ng.ui.IStateService,
                private $log: ng.ILogService,
                private isServerLicenseValid: boolean,
                private selectionManager: ISelectionManager) {

        $rootScope.$on("$stateChangeStart", this.stateChangeStart);
        $rootScope.$on("$stateChangeStart", this.stateChangeStart);

        if (!isServerLicenseValid) {
            $state.go("licenseError");
        }
    }

    private isLeavingState(stateName: string, from: string, to: string): boolean {
        return from.indexOf(stateName) > -1 && to.indexOf(stateName) === -1;
    }

    private stateChangeStart = (event: ng.IAngularEvent, toState: ng.ui.IState, toParams: any, fromState: ng.ui.IState, fromParams) => {
        if (!this.isServerLicenseValid) {
            //Prevent leaving the license error state.
            if (toState.name !== "licenseError") {
                event.preventDefault();
                this.$state.go("licenseError");
            }
        }
    };

    private stateChangeSuccess = (event: ng.IAngularEvent, toState: ng.ui.IState, toParams, fromState: ng.ui.IState, fromParams) => {
        if (this.isLeavingState("main.item", fromState.name, toState.name)) {
            this.$log.info("Leaving artifact state, clearing selection...");
            this.selectionManager.clearAll();
        }
    };
}

export class LogoutStateController {
    public static $inject = [
        "$log",
        "session",
        "projectManager",
        "navigationService",
        "clipboardService"
    ];

    constructor(private $log: ng.ILogService,
                private session: ISession,
                private projectManager: IProjectManager,
                private navigation: INavigationService,
                private clipboardService: IClipboardService) {

        this.session.logout().then(() => {
            this.navigation.navigateToMain(true).finally(() => {
                this.projectManager.removeAll();
                this.clipboardService.clearData();
            });
        });
    }
}
