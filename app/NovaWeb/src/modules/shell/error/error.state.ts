import "angular";

config.$inject = ["$stateProvider", "$urlRouterProvider"];
export function config($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider): void {
   // $urlRouterProvider.otherwise("/main"); //??
    $stateProvider.state("error", new ErrorState());
    $stateProvider.state("error:font", new ErrorState("font"));
}

class ErrorState implements ng.ui.IState {
    public url = "/error";
    public template = require("./error-page.html");

    constructor(errorType?: string) {
        if (errorType) {
            switch (errorType) {
                case "font":
                    this.url = "/error/font";
                    this.template = require("./error-font.html");
                    break;
            }
        }
    }
}
