﻿import * as angular from "angular";
import { IMessageService, IWindowVisibility, ILocalizationService } from "../../core";
import { IUser, ISession } from "../../shell";
import { Models, Enums } from "../models";
import { IProjectManager, IArtifactManager } from "../../managers";

export class MainView implements ng.IComponentOptions {
    public template: string = require("./view.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = MainViewController;
    public transclude: boolean = true;
    public controllerAs = "$main";
}

export class MainViewController {
    private _subscribers: Rx.IDisposable[];
    
    static $inject: [string] = [
        "$state", 
        "session", 
        "projectManager", 
        "messageService", 
        "localization",
        "artifactManager",
        "windowVisibility"
    ];

    constructor(
        private $state: ng.ui.IState,
        private session: ISession,
        private projectManager: IProjectManager,
        private messageService: IMessageService,
        private localization: ILocalizationService,
        private artifactManager: IArtifactManager,
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
        this.artifactManager.dispose();
    }

    private onVisibilityChanged = (isHidden: boolean) => {
        document.body.classList.remove(isHidden ? "is-visible" : "is-hidden");
        document.body.classList.add(isHidden ? "is-hidden" : "is-visible");
    };

    private onProjectCollectionChanged = (projects: Models.IProject[]) => {
        this.isActive = Boolean(projects.length);
        this.toggle(Enums.ILayoutPanel.Left, Boolean(projects.length));
        this.toggle(Enums.ILayoutPanel.Right, Boolean(projects.length));
    };

    public isLeftToggled: boolean;
    public isRightToggled: boolean;
    public toggle = (id?: Enums.ILayoutPanel, state?: boolean) => {
        if (Enums.ILayoutPanel.Left === id) {
            this.isLeftToggled = angular.isDefined(state) ? state : !this.isLeftToggled;
        } else if (Enums.ILayoutPanel.Right === id) {
            this.isRightToggled = angular.isDefined(state) ? state : !this.isRightToggled;
        }
    };

    public isActive: boolean;
    
    public get currentUser(): IUser {
        return this.session.currentUser;
    }
}
