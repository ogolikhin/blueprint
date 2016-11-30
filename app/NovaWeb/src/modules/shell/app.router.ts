import * as angular from "angular";
import {ISession} from "./login/session.svc";
import {IProjectManager, IArtifactManager, ISelectionManager} from "../managers";
import {INavigationService} from "../core/navigation/navigation.svc";
import {ILicenseService} from "./license/license.svc";
import {ClipboardService, IClipboardService} from "./../editors/bp-process/services/clipboard.svc";


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
                    saved: ["artifactManager", (am: IArtifactManager) => { return am.autosave(); }]                    } 
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

    public mainState = "main";

    public static $inject = [
        "$rootScope",
        "$window",
        "$state",
        "$log",
        "selectionManager",
        "isServerLicenseValid",
        "session",
        "projectManager",
        "navigationService"
        
    ];

    constructor(private $rootScope: ng.IRootScopeService,
                private $window: ng.IWindowService,
                private $state: angular.ui.IStateService,
                private $log: ng.ILogService,
                private selectionManager: ISelectionManager,
                private isServerLicenseValid: boolean,
                private session: ISession,
                private projectManager: IProjectManager,
                private navigation: INavigationService) {

        $rootScope.$on("$stateChangeStart", this.stateChangeStart);
        $rootScope.$on("$stateChangeSuccess", this.stateChangeSuccess);

        if (!isServerLicenseValid) {
            $state.go("licenseError");
        }
    }

    private stateChangeSuccess = (event: ng.IAngularEvent, toState: ng.ui.IState, toParams: any, fromState: ng.ui.IState, fromParams) => {
        this.updateAppTitle();
    };

    private stateChangeStart = (event: ng.IAngularEvent, toState: ng.ui.IState, toParams: any, fromState: ng.ui.IState, fromParams) => {
        this.$log.info(
            "state transition: %c" + fromState.name + "%c -> %c" + toState.name + "%c " + JSON.stringify(toParams)
            , "color: blue", "color: black", "color: blue", "color: black"
        );

        if (!this.isServerLicenseValid) {
            //Prevent leaving the license error state.
            if (toState.name !== "licenseError") {
                event.preventDefault();
                this.$state.go("licenseError");
            }
        } else if (toState.name === this.mainState) {
            this.$log.info("SelectionManager.clearAll()");
            this.selectionManager.clearAll();
        } 
    };

    private updateAppTitle() {
        const artifact = this.selectionManager.getArtifact();

        let title: string;
        if (artifact) {
            title = `${artifact.prefix}${artifact.id}: ${artifact.name}`;
        } else {
            title = "Storyteller";
        }
        this.$window.document.title = title;
    }
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