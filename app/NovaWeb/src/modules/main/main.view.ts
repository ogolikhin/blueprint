import "angular";
import {ILocalizationService} from "../core/localization";


export class MainViewComponent implements ng.IComponentOptions {
    public template: string = require("./main.view.html");
    public controller: Function = MainViewController;
    public transclude: boolean = true;
}

export interface IMainViewController {
}

export class MainViewController implements IMainViewController {

    public static $inject: [string] = ["$scope", "localization",  "$log"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private $log: ng.ILogService) {
    }

}
