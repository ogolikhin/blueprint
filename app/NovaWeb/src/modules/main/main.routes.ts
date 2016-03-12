import "angular";
import {IAbout} from "./components/about/about.service";

config.$inject = ["$stateProvider", "$urlRouterProvider"];
export function config($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider): void {
    $urlRouterProvider.otherwise("/main");
    $stateProvider.state("main", new MainState());
}

class MainCtrl {
    public static $inject: [string] = ["$log", "about"];
    constructor(private $log: ng.ILogService, private about: IAbout) {
    }

    public leftToggled: boolean = false;

    public toggleLeft(): void {
        this.$log.debug("MainCtrl.toggleLeft");
        this.leftToggled = !this.leftToggled;
    }

    public showAbout(): void {
        this.$log.debug("MainCtrl.about");

        this.about.show();
    }
}

class MainState implements ng.ui.IState {
    public url = "/main";

    public template = require("./main.html");

    public controller = MainCtrl;
    public controllerAs = "main";
}