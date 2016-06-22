import "angular";
//import {ILocalizationService} from "../core/localization";
import {IDialogService} from "../core/";
import {IProjectManager} from "./";
import {ISession} from "../shell/login/session.svc";
import {IUser} from "../shell/login/auth.svc";

export class MainViewComponent implements ng.IComponentOptions {
    public template: string = require("./main.view.html");
    public controller: Function = MainViewController;
    public transclude: boolean = true;
    public controllerAs = "main";
}

export interface IMainViewController {
}

export class MainViewController implements IMainViewController {
    static $inject: [string] = ["$state",  "projectManager", "dialogService", "session"];
    constructor(
        private $state: ng.ui.IState,
        private projectManager: IProjectManager,
        private dialogService: IDialogService,
        private session: ISession) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        this.projectManager.initialize();
    }

    public $onDestroy() {   
    }

    public get currentUser(): IUser {
        return this.session.currentUser;
    }
}
