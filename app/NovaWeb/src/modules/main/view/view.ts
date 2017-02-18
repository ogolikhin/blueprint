import {IWindowVisibility, VisibilityStatus} from "../../commonModule/services/windowVisibility";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IDialogService, IDialogSettings} from "../../shared";
import {ISession} from "../../shell";
import {IUtilityPanelService} from "../../shell/bp-utility-panel/utility-panel.svc";
import {BPTourController} from "../components/dialogs/bp-tour/bp-tour";
import {IMessageService} from "../components/messages/message.svc";
import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {ILocalStorageService} from "../../commonModule/localStorage/localStorage.service";
import {IProjectExplorerService} from "../components/bp-explorer/project-explorer.service";
import {ExplorerNodeVM} from "../models/tree-node-vm-factory";
import {IChangeSet, ChangeTypeEnum} from "../../managers/artifact-manager/changeset/changeset";

export class MainView implements ng.IComponentOptions {
    public template: string = require("./view.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = MainViewController;
}

export class MainViewController {
    private _subscribers: Rx.IDisposable[];
    private previousOpenProjectCount: number;
    public isLeftToggled: boolean;
    public isLeftPanelExpanded: boolean;

    static $inject: [string] = [
        "$document",
        "session",
        "projectExplorerService",
        "messageService",
        "localization",
        "selectionManager",
        "windowVisibility",
        "utilityPanelService",
        "localStorageService",
        "dialogService"
    ];

    constructor(private $document: ng.IDocumentService,
                public session: ISession,
                private projectExplorerService: IProjectExplorerService,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private selectionManager: ISelectionManager,
                private windowVisibility: IWindowVisibility,
                private utilityPanelService: IUtilityPanelService,
                private localStorageService: ILocalStorageService,
                private dialogService: IDialogService) {
    }

    public $onInit() {
        this._subscribers = [
            this.projectExplorerService.projectsChangeObservable
                .filter(change => change.type === ChangeTypeEnum.Update)
                .subscribeOnNext(this.onProjectCollectionChanged, this),
            this.windowVisibility.visibilityObservable.distinctUntilChanged().subscribeOnNext(this.onVisibilityChanged, this)
        ];

        this.previousOpenProjectCount = 0;

        this.openTourFirstTime();
    }

    private openTourFirstTime(): void {
        if (this.session.currentUser) {
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
        this._subscribers.forEach((subscriber: Rx.IDisposable) => subscriber.dispose());
        this._subscribers = [];
        this.messageService.dispose();
        this.selectionManager.dispose();
    }

    private onVisibilityChanged = (status: VisibilityStatus) => {
        this.$document[0].body.classList.remove(status === VisibilityStatus.Visible ? "is-hidden" : "is-visible");
        this.$document[0].body.classList.add(status === VisibilityStatus.Visible ? "is-visible" : "is-hidden");
    };

    private onProjectCollectionChanged(projectsUpdate: IChangeSet) {
        const projects = projectsUpdate.value as ExplorerNodeVM[];
        if (projects.length === 0) {
            //Close the panel if no projects are open.
            this.toggleLeft(false);
            this.toggleRight(false);
        } else if (projects.length > this.previousOpenProjectCount) {
            //Open the panel only if a project was opened.
            //Rationale: This method is also called if a project is closed,
            // but we don't want to do anything if the project count went from 3 to 2
            this.toggleLeft(true);
            this.toggleRight(true);
        }

        this.previousOpenProjectCount = projects.length;
    }

    /*
     * Toggle visibility of left (explorer) side panel
     * @param state optionally set visibility isntead of toggling (true = visible)
     */
    public toggleLeft(state?: boolean) {
        this.isLeftToggled = _.isUndefined(state) ? !this.isLeftToggled : state;
    }

    /*
     * Toggle expansion (width) of left panel.
     */
    public toggleExpandLeft() {
        this.isLeftPanelExpanded = !this.isLeftPanelExpanded;
    }

    /*
     * Toggle visibility of right (properties) side panel
     * @param state optionally set visibility isntead of toggling (true = visible)
     */
    public toggleRight(state?: boolean) {
        this.isRightToggled = _.isUndefined(state) ? !this.isRightToggled : state;
    }

    public get isRightToggled() {
        return this.utilityPanelService.isUtilityPanelOpened;
    }

    public set isRightToggled(value: boolean) {
        this.utilityPanelService.isUtilityPanelOpened = value;
    }
}
