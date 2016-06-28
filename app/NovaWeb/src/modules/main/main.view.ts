import "angular";
//import {ILocalizationService} from "../core/localization";
import {IDialogService} from "../core/";
import {ISidebarController} from "./components/bp-sidebar-layout/bp-sidebar-layout";
import {IProjectManager, Models} from "./";
import {ISession} from "../shell/login/session.svc";
import {IUser} from "../shell/login/auth.svc";

export class MainViewComponent implements ng.IComponentOptions {
    public template: string = require("./main.view.html");
    public controller: Function = MainViewController;
    public transclude: boolean = true;
    public controllerAs = "$main";
}

export interface IMainViewController {
}

export class MainViewController implements IMainViewController {
    private _subscribers: Rx.IDisposable[];
    public layout: ISidebarController;
    static $inject: [string] = ["$state", "projectManager", "dialogService", "session"];
    constructor(
        private $state: ng.ui.IState,
        private manager: IProjectManager,
        private dialogService: IDialogService,
        private session: ISession) {
    }

    public $onInit() {
        this.manager.initialize();
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for project collection update
            this.manager.projectCollection.subscribeOnNext(this.doLayoutChange, this),
            this.manager.currentProject.subscribeOnNext(this.projectChanged, this),
        ];
}

    public $onDestroy() {   
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.manager.dispose();
    }

    private doLayoutChange = (projects: Models.IProject[]) => {
        this.isActive = !!projects;
        if (this.layout) {
            this.layout.isLeftToggled = !!projects;
        }
    }
    private projectChanged = (project: Models.IProject) => {
        this.isActive = !!project;
    }
    public isActive: boolean;
    
    public get currentUser(): IUser {
        return this.session.currentUser;
    }
}
