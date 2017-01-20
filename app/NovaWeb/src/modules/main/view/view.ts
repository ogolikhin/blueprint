import {IWindowVisibility, VisibilityStatus} from "../../core/services/window-visibility";
import {IStatefulArtifact} from "../../managers/artifact-manager";
import {IProjectManager} from "../../managers/project-manager";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IDialogService, IDialogSettings} from "../../shared";
import {ISession, IUser} from "../../shell";
import {IUtilityPanelService} from "../../shell/bp-utility-panel/utility-panel.svc";
import {BPTourController} from "../components/dialogs/bp-tour/bp-tour";
import {IViewModel} from "../models/models";
import {IMessageService} from "../components/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localization.service";
import {ILocalStorageService} from "../../core/localStorage/localStorage.service";

export class MainView implements ng.IComponentOptions {
    public template: string = require("./view.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = MainViewController;
}

export class MainViewController {
    private _subscribers: Rx.IDisposable[];
    public isLeftToggled: boolean;
    public isActive: boolean;
    public isLeftPanelExpanded: boolean;

    static $inject: [string] = [
        "$document",
        "session",
        "projectManager",
        "messageService",
        "localization",
        "selectionManager",
        "windowVisibility",
        "utilityPanelService",
        "localStorageService",
        "dialogService"
    ];

    constructor(private $document: ng.IDocumentService,
                private session: ISession,
                private projectManager: IProjectManager,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private selectionManager: ISelectionManager,
                private windowVisibility: IWindowVisibility,
                private utilityPanelService: IUtilityPanelService,
                private localStorageService: ILocalStorageService,
                private dialogService: IDialogService) {
    }

    public $onInit() {
        this.projectManager.initialize();
        this._subscribers = [
            this.projectManager.projectCollection.subscribeOnNext(this.onProjectCollectionChanged, this),
            this.windowVisibility.visibilityObservable.distinctUntilChanged().subscribeOnNext(this.onVisibilityChanged, this)
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
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        this.messageService.dispose();
        this.projectManager.dispose();
        this.selectionManager.dispose();
    }
    private onVisibilityChanged = (status: VisibilityStatus) => {
        this.$document[0].body.classList.remove(status === VisibilityStatus.Visible ? "is-hidden" : "is-visible");
        this.$document[0].body.classList.add(status === VisibilityStatus.Visible ? "is-visible" : "is-hidden");
    };

    private onProjectCollectionChanged(projects: IViewModel<IStatefulArtifact>[]) {
        this.isActive = Boolean(projects.length);
        this.toggleLeft(Boolean(projects.length));
        this.toggleRight(Boolean(projects.length));
    }

    public toggleLeft(state?: boolean) {
        this.isLeftToggled = _.isUndefined(state) ? !this.isLeftToggled : state;
    }

    public toggleExpandLeft() {
        this.isLeftPanelExpanded = !this.isLeftPanelExpanded;
    }

    public toggleRight(state?: boolean) {
        this.isRightToggled = _.isUndefined(state) ? !this.isRightToggled : state;
    }

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
