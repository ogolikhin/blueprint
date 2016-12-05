﻿import {IWindowVisibility, VisibilityStatus} from "../../core/services/window-visibility";
import {IUser, ISession} from "../../shell";
import {Models, Enums} from "../models";
import {IProjectManager} from "../../managers/project-manager";
import {IArtifactManager, IStatefulArtifact} from "../../managers/artifact-manager";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IUtilityPanelService} from "../../shell/bp-utility-panel/utility-panel.svc";

export class MainView implements ng.IComponentOptions {
    public template: string = require("./view.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = MainViewController;
    public transclude: boolean = true;
    public controllerAs = "$main";
}

export class MainViewController {
    
    static $inject: [string] = [
        "$state",
        "$interval",
        "session",
        "projectManager",
        "messageService",
        "localization",
        "artifactManager",
        "windowVisibility",
        "utilityPanelService"
    ];

    private _subscribers: Rx.IDisposable[];

    public isLeftToggled: boolean;
    public isActive: boolean;
    private timer: any;
    private originalTitle: string;

    constructor(private $state: ng.ui.IState,
                private $interval: ng.IIntervalService,
                private session: ISession,
                private projectManager: IProjectManager,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private artifactManager: IArtifactManager,
                private windowVisibility: IWindowVisibility,
                private utilityPanelService: IUtilityPanelService) {
        this.originalTitle = document.title;
    }

    public $onInit() {
        this.projectManager.initialize();
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onProjectCollectionChanged, this),
            this.windowVisibility.visibilityObservable.distinctUntilChanged()
                .filter((it) => it === VisibilityStatus.Hidden || it === VisibilityStatus.Visible )
                .subscribeOnNext(this.onVisibilityChanged, this),
            this.windowVisibility.visibilityObservable
               //.filter((it) => it === VisibilityStatus.Hidden || it === VisibilityStatus.Blur )
                .distinctUntilChanged()
                .subscribeOnNext(this.onBlur, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        this.messageService.dispose();
        this.projectManager.dispose();
        this.artifactManager.dispose();
    }
    private onVisibilityChanged = (status: VisibilityStatus) => {
        document.body.classList.remove(status === VisibilityStatus.Visible ? "is-hidden" : "is-visible");
        document.body.classList.add(status === VisibilityStatus.Visible ? "is-visible" : "is-hidden");
    };

    
    private onBlur = (status: VisibilityStatus) => {
        if (status === VisibilityStatus.Hidden ) {
            this.artifactManager.autosave(false).catch(() => {
                    this.messageService.addError("Autosave has failed!");
                    this.setAlert("Error ***", 500);
                //window.alert("Autosave has failed!");
            });
        } else {
            this.clearAlert();
        }
    };

    private onProjectCollectionChanged = (projects: Models.IViewModel<IStatefulArtifact>[]) => {
        this.isActive = Boolean(projects.length);
        this.toggle(Enums.ILayoutPanel.Left, Boolean(projects.length));
        this.toggle(Enums.ILayoutPanel.Right, Boolean(projects.length));
    };

    public toggle = (id?: Enums.ILayoutPanel, state?: boolean) => {
        if (Enums.ILayoutPanel.Left === id) {
            this.isLeftToggled = angular.isDefined(state) ? state : !this.isLeftToggled;
        } else if (Enums.ILayoutPanel.Right === id) {
            this.isRightToggled = angular.isDefined(state) ? state : !this.isRightToggled;
        }
    };

    public get isRightToggled() {
        return this.utilityPanelService.isUtilityPanelOpened;
    }

    public set isRightToggled(value: boolean) {
        this.utilityPanelService.isUtilityPanelOpened = value;
    }

    public get currentUser(): IUser {
        return this.session.currentUser;
    }
 

 
    private setAlert(message: string, interval: number = 1) {
        document.title = message = message || "Error";
        this.timer = this.$interval(() => {
             document.title = (document.title === message) ? this.originalTitle : message;
        }, interval);      
    }
    private clearAlert() {
        if (this.timer) {
            this.$interval.cancel(this.timer);
            document.title = this.originalTitle;
        }
    }
}
