import "angular";
import { IMessageService, IWindowVisibility } from "../core";
import { IUser, ISession } from "../shell";
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
    static $inject: [string] = ["$state", "session", "projectManager", "messageService", "windowVisibility"];
    constructor(
        private $state: ng.ui.IState,
        private session: ISession,
        private projectManager: IProjectManager,
        private messageService: IMessageService,
        private windowVisibility: IWindowVisibility) {
    }

    public $onInit() {
        this.projectManager.initialize();
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onProjectCollectionChanged, this),
            this.windowVisibility.isHidden.subscribeOnNext(this.onVisibilityChanged, this)
        ];
}
    
    public $onDestroy() {   
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.messageService.dispose();
        this.projectManager.dispose();
    }

    private onVisibilityChanged = (isHidden: boolean) => {
        document.body.classList.remove(isHidden ? "is-visible" : "is-hidden");
        document.body.classList.add(isHidden ? "is-hidden" : "is-visible");
    };

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

    public isActive: boolean;
    
    public get currentUser(): IUser {
        return this.session.currentUser;
    }
}
