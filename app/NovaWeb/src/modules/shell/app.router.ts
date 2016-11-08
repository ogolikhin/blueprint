import * as angular from "angular";
import {ISession} from "./login/session.svc";
import { IArtifactManager } from "../managers";
import { ILicenseService } from "./license/license.svc";
import { IMessageService } from "../core";

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
    private stateChangeListener: Function;
    public mainState = "main";

    public static $inject = [
        "$rootScope",
        "$state",
        "$log",
        "artifactManager",
        "isServerLicenseValid",
        "messageService"
    ];

    constructor(private $rootScope: ng.IRootScopeService,
                private $state: angular.ui.IStateService,
                private $log: ng.ILogService,
                private artifactManager: IArtifactManager,
                private isServerLicenseValid: boolean,
                private messageService: IMessageService) {

       $rootScope.$on("$stateChangeStart", this.stateChangeStart);

        if (!isServerLicenseValid) {
            $state.go("licenseError");
        }

    }

    private stateChangeStart = (event: ng.IAngularEvent, toState: ng.ui.IState, toParams: any, fromState: ng.ui.IState, fromParams) => {
        // clear messages when the routing state changes 
        this.messageService.clearMessages();

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
            this.artifactManager.selection.clearAll();
        }
    }
}
