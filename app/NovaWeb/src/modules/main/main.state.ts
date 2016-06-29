    import "angular";
import {AuthenticationRequired} from "../shell/authentication";

config.$inject = ["$stateProvider", "$urlRouterProvider"];
export function config($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider): void {
    $urlRouterProvider.otherwise("/main");
    $stateProvider.state("main", new MainState());
}

class MainState extends AuthenticationRequired implements ng.ui.IState {
    public url = "/main";
    public template = "<bp-main-view></bp-main-view>";
}