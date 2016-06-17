import "angular";
//import {ILocalizationService} from "../core/localization";
import {IDialogService} from "../core/";
import {IProjectManager, Models} from "./";
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
    private _listeners: string[];
    private _currentArtifact: string;
    public get currentArtifact() { 
        return this._currentArtifact;
    }

    static $inject: [string] = ["$state",  "projectManager", "dialogService", "session"];
    constructor(
        private $state: ng.ui.IState,
        private projectManager: IProjectManager,
        private dialogService: IDialogService,
        private session: ISession) {
    }

    public $onInit() {
        this.projectManager.initialize();
        this.projectManager.currentArtifact.asObservable().subscribe(this.displayArtifact);
    }
    public $onDestroy() {
    }

    private displayArtifact = (artifact: Models.IArtifact) => {
        this._currentArtifact = artifact ? `${(artifact.prefix || "")}${artifact.id}: ${artifact.name}` : null;
    }

    public get currentUser(): IUser {
        return this.session.currentUser;
    }
}
