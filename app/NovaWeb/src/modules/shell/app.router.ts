import * as angular from "angular";
import {ISession} from "./login/session.svc";

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
                resolve: {
                    authenticated: ["session", (session: ISession) => {
                        return session.ensureAuthenticated();
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
                // resolve: {
                //     authenticated: ["session", (session: ISession) => {
                //         return session.ensureAuthenticated();
                //     }]
                // }
            });
    }
}
