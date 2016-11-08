import {Models, Enums} from "../../models";
import {IWindowManager, IMainWindow, ResizeCause} from "../../services";
import {IArtifactManager, IStatefulArtifact, IMetaDataService, IItemChangeSet} from "../../../managers/artifact-manager";
import {IProjectManager} from "../../../managers/project-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {
    IDialogSettings,
    IDialogService,
    IBPAction,
    BPButtonGroupAction
} from "../../../shared";
import {
    SaveAction,
    PublishAction,
    DiscardAction,
    RefreshAction,
    DeleteAction,
    OpenImpactAnalysisAction
} from "./actions";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {Message, MessageType} from "../../../core/messages/message";
import {IMessageService} from "../../../core/messages/message.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactInfoController;
    public transclude: boolean = true;
}

export class BpArtifactInfoController {
    static $inject: [string] = [
        "$scope",
        "$element",
        "artifactManager",
        "localization",
        "messageService",
        "dialogService",
        "windowManager",
        "loadingOverlayService",
        "navigationService",
        "projectManager",
        "metadataService"
    ];

    protected subscribers: Rx.IDisposable[];
    protected artifact: IStatefulArtifact;
    public isReadonly: boolean;
    public isChanged: boolean;
    public isLocked: boolean;
    public lockMessage: Message;
    public selfLocked: boolean;
    public isLegacy: boolean;
    public artifactName: string;
    public artifactType: string;
    public artifactClass: string;
    public artifactTypeId: number;
    public artifactTypeIconId: number;
    public artifactTypeDescription: string;
    public hasCustomIcon: boolean;
    public toolbarActions: IBPAction[];

    constructor(public $scope: ng.IScope,
                private $element: ng.IAugmentedJQuery,
                protected artifactManager: IArtifactManager,
                protected localization: ILocalizationService,
                protected messageService: IMessageService,
                protected dialogService: IDialogService,
                protected windowManager: IWindowManager,
                protected loadingOverlayService: ILoadingOverlayService,
                protected navigationService: INavigationService,
                protected projectManager: IProjectManager,
                protected metadataService: IMetaDataService) {
        this.initProperties();
        this.subscribers = [];
    }

    public $onInit() {

        const windowSub = this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this);
        this.subscribers.push(windowSub);

        this.artifact = this.artifactManager.selection.getArtifact();
        if (this.artifact) {
            this.subscribers.push(this.artifact.getObservable()
                                                .subscribeOnNext(this.onArtifactChanged));
            this.subscribers.push(this.artifact.getProperyObservable()
                                                .distinctUntilChanged(changes => changes.item && changes.item.name)                            
                                                .subscribeOnNext(this.onArtifactPropertyChanged));
        }
    }

    public $onDestroy() {
        this.initProperties();
        this.subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        delete this["subscribers"];
        delete this["artifact"];
    }

    protected onArtifactChanged = () => {
        if (this.artifact) {
            this.updateProperties(this.artifact);
            this.subscribeToStateChange(this.artifact);
        }
    };
    protected onArtifactPropertyChanged = (change: IItemChangeSet) => {
        if (this.artifact) {
            this.artifactName = change.item.name;
        }
    }

    protected subscribeToStateChange(artifact) {
        // watch for state changes (dirty, locked etc) and update header
        const stateObserver = artifact.artifactState.onStateChange.debounce(100).subscribe(
            (state) => {
                this.updateProperties(this.artifact);
            },
            (err) => {
                throw new Error(err);
            });

        this.subscribers.push(stateObserver);
    }

    private initProperties() {
        this.artifactName = null;
        this.artifactType = null;
        this.artifactTypeId = null;
        this.artifactTypeIconId = null;
        this.artifactTypeDescription = null;
        this.isLegacy = false;
        this.isReadonly = false;
        this.isChanged = false;
        this.isLocked = false;
        this.selfLocked = false;
        this.isLegacy = false;
        this.artifactClass = null;
        this.toolbarActions = [];

        if (this.lockMessage) {
            this.messageService.deleteMessageById(this.lockMessage.id);
            this.lockMessage = null;
        }
    }

    protected updateProperties(artifact: IStatefulArtifact) {
        this.initProperties();

        if (!artifact) {
            return;
        }

        this.updateToolbarOptions(artifact);

        this.artifactName = artifact.name;
        this.artifactTypeId = artifact.itemTypeId;
        this.artifactTypeIconId = artifact.itemTypeIconId;
        this.hasCustomIcon = _.isFinite(artifact.itemTypeIconId);

        this.isLegacy = artifact.predefinedType === Enums.ItemTypePredefined.Storyboard ||
            artifact.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
            artifact.predefinedType === Enums.ItemTypePredefined.UseCase ||
            artifact.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.UIMockup ||
            artifact.predefinedType === Enums.ItemTypePredefined.DomainDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.Glossary;

        if (artifact.itemTypeId === Models.ItemTypePredefined.Collections && artifact.predefinedType === Models.ItemTypePredefined.CollectionFolder) {
            this.artifactClass = "icon-" + _.kebabCase(Models.ItemTypePredefined[Models.ItemTypePredefined.Collections]);
        } else {
            this.artifactClass = "icon-" + _.kebabCase(Models.ItemTypePredefined[artifact.predefinedType]);
        }
        this.artifactType = artifact.itemTypeName;
        this.artifactTypeDescription = `${this.artifactType} - ${(artifact.prefix || "")}${artifact.id}`;

        this.isReadonly = artifact.artifactState.readonly;
        this.isChanged = artifact.artifactState.dirty;

        switch (artifact.artifactState.lockedBy) {
            case Enums.LockedByEnum.CurrentUser:
                this.selfLocked = true;
                break;

            case Enums.LockedByEnum.OtherUser:
                let msg = artifact.artifactState.lockOwner ? "Locked by " + artifact.artifactState.lockOwner : "Locked ";
                if (artifact.artifactState.lockDateTime) {
                    msg += " on " + this.localization.current.formatShortDateTime(artifact.artifactState.lockDateTime);
                }
                msg += ".";
                this.messageService.addMessage(this.lockMessage = new Message(MessageType.Lock, msg));
                break;

            default:
                break;
        }
    }

    public get artifactHeadingMinWidth() {
        let style = {};

        if (this.$element.length) {
            let container: HTMLElement = this.$element[0];
            let toolbar: Element = container.querySelector(".page-top-toolbar");
            let heading: Element = container.querySelector(".artifact-heading");
            let iconWidth: number = heading && heading.querySelector(".icon") ? heading.querySelector(".icon").scrollWidth : 0;
            let nameWidth: number = heading && heading.querySelector(".name") ? heading.querySelector(".name").scrollWidth : 0;
            let typeWidth: number = heading && heading.querySelector(".type-id") ? heading.querySelector(".type-id").scrollWidth : 0;
            let indicatorsWidth: number = heading && heading.querySelector(".indicators") ? heading.querySelector(".indicators").scrollWidth : 0;
            let headingWidth: number = iconWidth + (
                    typeWidth > nameWidth + indicatorsWidth ? typeWidth : nameWidth + indicatorsWidth
                ) + 20 + 5; // heading's margins + wiggle room
            if (heading && toolbar) {
                style = {
                    "min-width": (headingWidth > toolbar.clientWidth ? toolbar.clientWidth : headingWidth) + "px"
                };
            }
        }

        return style;
    }

    protected updateToolbarOptions(artifact: IStatefulArtifact): void {
        const deleteDialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Ok"),
            template: require("../../../shared/widgets/bp-dialog/bp-dialog.html"),
            header: this.localization.get("App_DialogTitle_Alert"),
            message: "Are you sure you would like to delete the artifact?"
        };

        this.toolbarActions.push(
            new BPButtonGroupAction(
                new SaveAction(artifact, this.localization, this.messageService, this.loadingOverlayService),
                new PublishAction(artifact, this.localization, this.messageService, this.loadingOverlayService),
                new DiscardAction(artifact, this.localization, this.messageService, this.loadingOverlayService),
                new RefreshAction(artifact, this.localization, this.projectManager, this.loadingOverlayService, this.metadataService),
                new DeleteAction(artifact, this.localization, this.messageService, this.projectManager, this.loadingOverlayService, this.dialogService)
            ),
            new OpenImpactAnalysisAction(artifact, this.localization)
        );
    }

    private onWidthResized(mainWindow: IMainWindow) {
        if (mainWindow.causeOfChange === ResizeCause.browserResize || mainWindow.causeOfChange === ResizeCause.sidebarToggle) {
            let sidebarWrapper: Element;
            //const sidebarSize: number = 270; // MUST match $sidebar-size in styles/modules/_variables.scss

            let sidebarSize = 0;
            if ((<HTMLElement>document.querySelector(".sidebar.left-panel"))) {
                sidebarSize = (<HTMLElement>document.querySelector(".sidebar.left-panel")).offsetWidth;
            }

            let sidebarsWidth: number = 20 * 2; // main content area padding
            sidebarWrapper = document.querySelector(".bp-sidebar-wrapper");

            if (sidebarWrapper) {
                for (let c = 0; c < sidebarWrapper.classList.length; c++) {
                    if (sidebarWrapper.classList[c].indexOf("-panel-visible") !== -1) {
                        sidebarsWidth += sidebarSize;
                    }
                }
            }

            if (this.$element.length) {
                let container: HTMLElement = this.$element[0];
                let toolbar: Element = container.querySelector(".page-top-toolbar");
                let heading: Element = container.querySelector(".artifact-heading");
                if (heading && toolbar) {
                    angular.element(heading).css("max-width", (document.body.clientWidth - sidebarsWidth) < 2 * toolbar.clientWidth ?
                        "100%" : "calc(100% - " + toolbar.clientWidth + "px)");
                }
            }
        }
    }
}
