import "angular";
//import {ILocalizationService} from "../core/localization";
//import { IDialogService } from "../core/";
import { IMessageService, Message, IUser, ISession } from "../shell";
import { IProjectManager, Models, Enums } from "./";

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
    static $inject: [string] = ["$state", "session", "projectManager", "messageService"];
    constructor(
        private $state: ng.ui.IState,
        private session: ISession,
        private projectManager: IProjectManager,
        private messageService: IMessageService) {
    } 

    public $onInit() {
        this.projectManager.initialize();
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onProjectCollectionChanged, this),
            this.projectManager.currentProject.subscribeOnNext(this.onProjectChanged, this),
        ];
}
    
    public $onDestroy() {   
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.messageService.dispose();
        this.projectManager.dispose();
    }

    private onProjectCollectionChanged = (projects: Models.IProject[]) => {
        this.isActive = Boolean(projects.length);
        this.toggle(Enums.ILayoutPanel.Left, Boolean(projects.length));
        this.toggle(Enums.ILayoutPanel.Right, Boolean(projects.length));
    }

    public isLeftToggled: boolean;
    public isRightToggled: boolean;
    public toggle = (id?: Enums.ILayoutPanel, state?: boolean) => {
        if (Enums.ILayoutPanel.Left === id) {
            this.isLeftToggled = angular.isDefined(state) ? state : !this.isLeftToggled;
        } else if (Enums.ILayoutPanel.Right === id) {
            this.isRightToggled = angular.isDefined(state) ? state : !this.isRightToggled;
        }
    }

    private onProjectChanged = (project: Models.IProject) => {
        this.isActive = !!project;
    }
    public isActive: boolean;
    
    public get currentUser(): IUser {
        return this.session.currentUser;
    }
}
