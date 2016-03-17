import "angular";
import {AuthenticationRequired} from "../shell";

config.$inject = ["$stateProvider", "$urlRouterProvider"];
export function config($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider): void {
    $urlRouterProvider.otherwise("/main");
    $stateProvider.state("main", new MainState());
}

class MainCtrl {
    public static $inject: [string] = ["$log"];
    constructor(private $log: ng.ILogService) {
    }
}

class MainState extends AuthenticationRequired implements ng.ui.IState {
    public url = "/main";

    public template = require("./main.html");

    public controller = MainCtrl;
    public controllerAs = "main";
}