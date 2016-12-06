﻿import {IWindowVisibility} from "../../core/services/window-visibility";
import {IUser, ISession} from "../../shell";
import {Models, Enums} from "../models";
import {IProjectManager} from "../../managers/project-manager";
import {IArtifactManager, IStatefulArtifact} from "../../managers/artifact-manager";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IUtilityPanelService} from "../../shell/bp-utility-panel/utility-panel.svc";
import {ILocalStorageService} from "../../core/local-storage/local-storage.svc";
import {IDialogService, IDialogSettings} from "../../shared";
import {BPTourController} from "../components/dialogs/bp-tour/bp-tour";

export class MainView implements ng.IComponentOptions {
    public template: string = require("./view.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = MainViewController;
    public transclude: boolean = true;
    public controllerAs = "$main";
}

export class MainViewController {
    
    static $inject: [string] = [
        "$state",
        "session",
        "projectManager",
        "messageService",
        "localization",
        "artifactManager",
        "windowVisibility",
        "utilityPanelService",
        "localStorageService",
        "dialogService"
    ];

    private _subscribers: Rx.IDisposable[];

    public isLeftToggled: boolean;
    public isActive: boolean;

    constructor(private $state: ng.ui.IState,
                private session: ISession,
                private projectManager: IProjectManager,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private artifactManager: IArtifactManager,
                private windowVisibility: IWindowVisibility,
                private utilityPanelService: IUtilityPanelService,
                private localStorageService: ILocalStorageService,
                private dialogService: IDialogService) {
    }

    public $onInit() {
        this.projectManager.initialize();
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onProjectCollectionChanged, this),
            this.windowVisibility.isHidden.subscribeOnNext(this.onVisibilityChanged, this)
        ]; 

        this.openTourFirstTime();
    }

    private openTourFirstTime(): void {
        if (this.currentUser) {
            const productTourKey = "ProductTour";
            const productTour = this.localStorageService.read(productTourKey);
            if (!productTour) {
                this.localStorageService.write(productTourKey, "true");
                this.dialogService.open(<IDialogSettings>{
                    template: require("../components/dialogs/bp-tour/bp-tour.html"),
                    controller: BPTourController,
                    backdrop: true,
                    css: "nova-tour"
                });
            }
        }
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

    private onVisibilityChanged = (isHidden: boolean) => {
        if (isHidden) {
            this.artifactManager.autosave(false).catch(() => {
                alert("Autosave has failed!");
            });
        }

        document.body.classList.remove(isHidden ? "is-visible" : "is-hidden");
        document.body.classList.add(isHidden ? "is-hidden" : "is-visible");
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
}
