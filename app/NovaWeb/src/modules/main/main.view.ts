import "angular";
import {ILocalizationService} from "../core/localization";
import * as pSvc from "./services/project.svc";


export class MainViewComponent implements ng.IComponentOptions {
    public template: string = require("./main.view.html");
    public controller: Function = MainViewController;
    public transclude: boolean = true;
}

export interface IMainViewController {
}

export class MainViewController implements IMainViewController {
    public projectExplorer: any;

    public static $inject: [string] = ["$scope", "localization", "projectService", "$element", "$log", "$timeout"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private service: pSvc.IProjectService,
        private $element,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService
    ) {
    }
}
