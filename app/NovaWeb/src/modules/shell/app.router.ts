import * as angular from "angular";
import {ISession} from "./login/session.svc";
import { IApplicationError, HttpStatusCode } from "../core";
import { INavigationService } from "../core/navigation";
import { IArtifactManager, IProjectManager } from "../managers";

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
                    authenticated: ["session", (session: ISession) => {
                        return session.ensureAuthenticated();
                    }]
                }
            })
            .state("error", {
                url: "/error",
                template: require("./error/error-page.html")
            });
    }
}

export class MainStateController {
    private stateChangeListener: Function;

    public static $inject = [
        "$rootScope",
        "$state",
        "$log"
    ];

    constructor(private $rootScope: ng.IRootScopeService,
                private $state: angular.ui.IStateService,
                private $log: ng.ILogService) {

        this.stateChangeListener = $rootScope.$on("$stateChangeStart", this.stateChangeHandler);
    }

    private stateChangeHandler = (event, toState, toParams, fromState, fromParams) => {
        this.$log.log(
                "--- $stateChangeStart: %c" + fromState.name + "%c -> %c" + toState.name + "%c " + JSON.stringify(toParams)
                , "color: red", "color: black", "color: red", "color: black"
            );
    }
}
