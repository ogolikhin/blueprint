import "angular";


config.$inject = ["$stateProvider", "$urlRouterProvider"];
export function config($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider): void {
   // $urlRouterProvider.otherwise("/main"); //??
    $stateProvider.state("error", new ErrorState());
}

class ErrorState implements ng.ui.IState {
    public url = "/error";
    public template = require("./error/errorPage.html");
}