import * as angular from "angular";
import {Models, Enums} from "../../models";
import {IWindowManager, IMainWindow, ResizeCause} from "../../services";
import {IMessageService, Message, MessageType, ILocalizationService} from "../../../core";
import {ILoadingOverlayService} from "../../../core/loading-overlay";
import {IArtifactManager, IStatefulArtifact, IMetaDataService} from "../../../managers/artifact-manager";
import {IProjectManager} from "../../../managers/project-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {
    Helper,
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

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactInfoController;
    public transclude: boolean = true;
    public bindings: any = {
        context: "<"
    };
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
    public artifactTypeIcon: number;
    public artifactTypeDescription: string;
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
        // const stateSub = this.artifactManager.selection.artifactObservable
        //     // cannot always skip 1 and rely on the artifact observable having 2 values (initial and new)
        //     // this is true when navigating to artifact X from artifact X via breadcrumb (loop)
        //     // .skip(1) // skip the first (initial) value

        //     .filter((artifact: IStatefulArtifact) => artifact != null)
        //     .distinctUntilChanged(artifact => artifact.id)
        //     .flatMap((artifact: IStatefulArtifact) => {
        //         this.artifact = artifact;
        //         return artifact.getObservable();
        //     })
        //     .subscribeOnNext(this.onStateChanged);

        this.subscribers.push(windowSub);
    }

    public $onChanges(obj: any) {
        this.artifactManager.get(obj.context.currentValue).then((artifact) => {
            if (artifact) {
                this.artifact = artifact;
                const artifactObserver = artifact.getObservable()
                    .subscribe(this.onArtifactChanged, this.onError);

                this.subscribers.push(artifactObserver);
            }
        });
    }

    public $onDestroy() {
        this.initProperties();
        this.subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        delete this.subscribers;
    }

    protected onArtifactChanged = () => {
        this.updateProperties(this.artifact);
        this.subscribeToStateChange(this.artifact);
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

    public onError = (error: any) => {
        if (this.artifact.artifactState.deleted || this.artifact.artifactState.misplaced) {
            //Occurs when refreshing an artifact that's been moved/deleted; do nothing
        } else {
            this.messageService.addError(error);
        }

        this.onArtifactChanged();
    }

    private initProperties() {
        this.artifactName = null;
        this.artifactType = null;
        this.artifactTypeId = null;
        this.artifactTypeIcon = null;
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

        this.artifactName = artifact.name || "";

        artifact.metadata.getItemType().then(itemType => {
            this.artifactTypeId = itemType.id;
            this.artifactType = itemType.name || Models.ItemTypePredefined[itemType.predefinedType] || "";

            if (itemType.iconImageId && angular.isNumber(itemType.iconImageId)) {
                this.artifactTypeIcon = itemType.iconImageId;
            }

            this.artifactTypeDescription = `${this.artifactType} - ${(artifact.prefix || "")}${artifact.id}`;

            this.artifactClass = "icon-" + (Helper.toDashCase(Models.ItemTypePredefined[itemType.predefinedType] || "document"));

            this.isLegacy = itemType.predefinedType === Enums.ItemTypePredefined.Storyboard ||
                itemType.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
                itemType.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
                itemType.predefinedType === Enums.ItemTypePredefined.UseCase ||
                itemType.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
                itemType.predefinedType === Enums.ItemTypePredefined.UIMockup ||
                itemType.predefinedType === Enums.ItemTypePredefined.DomainDiagram ||
                itemType.predefinedType === Enums.ItemTypePredefined.Glossary;

        });

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
                new DiscardAction(artifact, this.localization),
                new RefreshAction(artifact, this.localization, this.projectManager, this.loadingOverlayService, this.metadataService),
                new DeleteAction(artifact, this.localization, this.dialogService, deleteDialogSettings)
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
