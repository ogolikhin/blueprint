import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {ItemTypePredefined, LockedByEnum} from "../../models/enums";
import {IWindowManager, IMainWindow, ResizeCause} from "../../services";
import {
    IArtifactState,
    IStatefulArtifact,
    IMetaDataService,
    IItemChangeSet
} from "../../../managers/artifact-manager";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";
import {
    IDialogService,
    IBPAction,
    IBPDropdownAction,
    IBPButtonOrDropdownAction,
    BPButtonGroupAction,
    BPMenuAction,
    BPButtonOrDropdownAction,
    BPButtonOrDropdownSeparator
} from "../../../shared";
import {
    SaveAction,
    PublishAction,
    DiscardAction,
    RefreshAction,
    DeleteAction,
    OpenImpactAnalysisAction,
    MoveCopyAction,
    AddToCollectionAction
} from "./actions";
import {ILoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IMainBreadcrumbService} from "../bp-page-content/mainbreadcrumb.svc";
import {ICollectionService} from "../../../editorsModule/collection/collection.service";
import {Enums} from "../../models";
import {IItemInfoService} from "../../../commonModule/itemInfo/itemInfo.service";
import {IMessageService} from "../messages/message.svc";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {IProjectExplorerService} from "../bp-explorer/project-explorer.service";

enum InfoBannerEnum {
    None = 0,
    Historical = 1,
    Deleted = 2,
    Locked = 3,
    NoPermissions = 4
}

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactInfoController;
}

export class BpArtifactInfoController {
    static $inject: [string] = [
        "$q",
        "$scope",
        "$element",
        "$timeout",
        "selectionManager",
        "localization",
        "messageService",
        "dialogService",
        "windowManager",
        "loadingOverlayService",
        "navigationService",
        "projectExplorerService",
        "metadataService",
        "mainbreadcrumbService",
        "collectionService",
        "itemInfoService"
    ];

    protected subscribers: Rx.IDisposable[] = [];
    protected artifact: IStatefulArtifact;
    public breadcrumbLinks: IBreadcrumbLink[];

    public isLegacy: boolean;
    public isReadonly: boolean;
    public noPermissions: boolean;
    public isDeleted: boolean;
    public deletedMessage: string;
    public isChanged: boolean;
    public isLocked: boolean;
    public selfLocked: boolean;
    public lockedMessage: string;
    public isHistorical: boolean;
    public historicalMessage: string;

    public artifactName: string;
    public artifactType: string;
    public artifactClass: string;
    public artifactTypeId: number;
    public artifactTypeIconId: number;
    public artifactTypeDescription: string;
    public hasCustomIcon: boolean;
    public toolbarActions: IBPAction[] = [];
    public collapsedToolbarActions: IBPAction[] = [];
    public additionalMenuActions: IBPButtonOrDropdownAction[] = [];
    public isToolbarCollapsed: boolean = true;

    constructor(public $q: ng.IQService,
                public $scope: ng.IScope,
                private $element: ng.IAugmentedJQuery,
                private $timeout: ng.ITimeoutService,
                protected selectionManager: ISelectionManager,
                protected localization: ILocalizationService,
                protected messageService: IMessageService,
                protected dialogService: IDialogService,
                protected windowManager: IWindowManager,
                protected loadingOverlayService: ILoadingOverlayService,
                protected navigationService: INavigationService,
                protected projectExplorerService: IProjectExplorerService,
                protected metadataService: IMetaDataService,
                protected mainBreadcrumbService: IMainBreadcrumbService,
                protected collectionService: ICollectionService,
                public itemInfoService: IItemInfoService) {
        this.initProperties();

        this.breadcrumbLinks = [];
    }

    public $onInit() {
        this.artifact = this.selectionManager.getArtifact();

        if (this.artifact) {
            this.createToolbarActions();

            this.subscribers.push(
                this.artifact.getObservable()
                    .subscribeOnNext(this.onArtifactLoaded, this),
                this.artifact.artifactState.onStateChange
                    .debounce(100)
                    .subscribeOnNext(this.onArtifactStateChanged, this),
                this.artifact.getPropertyObservable()
                    .distinctUntilChanged(changes => changes.item && changes.item.name)
                    .subscribeOnNext(this.onArtifactPropertyChanged, this)
            );
        }

        this.subscribers.push(
            this.windowManager.mainWindow
                .subscribeOnNext(this.onWidthResized, this)
        );
    }

    public $onDestroy() {
        this.initProperties();

        this.subscribers.forEach(subscriber => {
            subscriber.dispose();
        });

        this.subscribers = undefined;
        this.artifact = undefined;
    }

    public get infoBanner(): InfoBannerEnum {
        if (this.isHistorical) {
            return InfoBannerEnum.Historical;
        }
        if (this.isDeleted) {
            return InfoBannerEnum.Deleted;
        }
        if (this.noPermissions) {
            return InfoBannerEnum.NoPermissions;
        }
        if (this.isLocked && !this.selfLocked) {
            return InfoBannerEnum.Locked;
        }

        return InfoBannerEnum.None;
    }

    protected onArtifactLoaded(): void {
        if (this.artifact) {
            this.updateProperties(this.artifact);

            if (this.artifact.artifactState.historical && !this.artifact.artifactState.deleted) {
                this.isHistorical = true;
                let msg = this.localization.get("Artifact_InfoBanner_Historical");
                msg = msg.replace("{0}", this.artifact.version.toString());
                msg = msg.replace("{1}", this.artifact.lastEditedBy.displayName);
                msg = msg.replace("{2}", this.localization.current.formatShortDateTime(this.artifact.lastEditedOn));
                this.historicalMessage = msg;
            }
        }
    };

    protected onArtifactStateChanged(state: IArtifactState): void {
        if (state) {
            this.initStateProperties();
            this.updateStateProperties(state);

            this.$scope.$applyAsync();
        }
    }

    protected onArtifactPropertyChanged(change: IItemChangeSet): void {
        if (this.artifact) {
            this.artifactName = change.item.name;
        }
    }

    private initProperties() {
        this.artifactName = null;
        this.artifactType = null;
        this.artifactTypeId = null;
        this.artifactTypeIconId = null;
        this.artifactTypeDescription = null;
        this.artifactClass = null;
        this.isLegacy = false;
        this.isHistorical = false;
        this.historicalMessage = null;

        this.initStateProperties();
    }

    private initStateProperties() {
        this.isReadonly = false;
        this.isChanged = false;
        this.noPermissions = false;
        this.isDeleted = false;
        this.deletedMessage = null;
        this.isLocked = false;
        this.selfLocked = false;
        this.lockedMessage = null;
    }

    private updateProperties(artifact: IStatefulArtifact): void {
        this.initProperties();

        this.artifactName = artifact.name;
        this.artifactTypeId = artifact.itemTypeId;
        this.artifactTypeIconId = artifact.itemTypeIconId;
        this.hasCustomIcon = _.isFinite(artifact.itemTypeIconId);
        this.noPermissions = (artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;

        this.isLegacy = artifact.predefinedType === ItemTypePredefined.Storyboard ||
            artifact.predefinedType === ItemTypePredefined.GenericDiagram ||
            artifact.predefinedType === ItemTypePredefined.BusinessProcess ||
            artifact.predefinedType === ItemTypePredefined.UseCase ||
            artifact.predefinedType === ItemTypePredefined.UseCaseDiagram ||
            artifact.predefinedType === ItemTypePredefined.UIMockup ||
            artifact.predefinedType === ItemTypePredefined.DomainDiagram ||
            artifact.predefinedType === ItemTypePredefined.Glossary;

        if (artifact.itemTypeId === ItemTypePredefined.BaselinesAndReviews && artifact.predefinedType === ItemTypePredefined.BaselineFolder) {
            this.artifactClass = "icon-" + _.kebabCase(ItemTypePredefined[ItemTypePredefined.BaselinesAndReviews]);
        } else if (artifact.itemTypeId === ItemTypePredefined.Collections && artifact.predefinedType === ItemTypePredefined.CollectionFolder) {
            this.artifactClass = "icon-" + _.kebabCase(ItemTypePredefined[ItemTypePredefined.Collections]);
        } else {
            this.artifactClass = "icon-" + _.kebabCase(ItemTypePredefined[artifact.predefinedType]);
        }

        this.artifactType = artifact.itemTypeName;
        this.artifactTypeDescription = `${this.artifactType} - ${(artifact.prefix || "")}${artifact.id}`;

        this.updateStateProperties(artifact.artifactState);
    }

    protected updateStateProperties(state: IArtifactState): void {
        this.isReadonly = state.readonly;
        this.isChanged = state.dirty;

        this.isDeleted = state.deleted;
        if (this.isDeleted) {
            let msg = this.localization.get("Artifact_InfoBanner_DeletedByOn");
            msg = msg.replace("{0}", state.deletedByDisplayName);
            msg = msg.replace("{1}", this.localization.current.formatShortDateTime(state.deletedDateTime));
            this.deletedMessage = msg;
        }

        switch (state.lockedBy) {
            case LockedByEnum.CurrentUser:
                this.isLocked = true;
                this.selfLocked = true;
                break;

            case LockedByEnum.OtherUser:
                this.isLocked = true;
                this.selfLocked = false;
                let msg: string;
                if (state.lockOwner && state.lockDateTime) {
                    msg = this.localization.get("Artifact_InfoBanner_LockedByOn");
                    msg = msg.replace("{0}", state.lockOwner);
                    msg = msg.replace("{1}", this.localization.current.formatShortDateTime(state.lockDateTime));
                } else if (state.lockOwner) {
                    msg = this.localization.get("Artifact_InfoBanner_LockedBy");
                    msg = msg.replace("{0}", state.lockOwner);
                } else if (state.lockDateTime) {
                    msg = this.localization.get("Artifact_InfoBanner_LockedOn");
                    msg = msg.replace("{0}", this.localization.current.formatShortDateTime(state.lockDateTime));
                } else {
                    msg = this.localization.get("Artifact_InfoBanner_Locked");
                }
                this.lockedMessage = msg;
                break;

            default:
                break;
        }
    }

    public get canLoadProject(): boolean {
        return this.canLoadProjectInternal();
    }

    private canLoadProjectInternal(): boolean {
        if (!this.artifact || !this.artifact.projectId) {
            return false;
        }

        const project = this.projectExplorerService.getProject(this.artifact.projectId);

        return !project;
    }

    public loadProject(): void {
        if (!this.canLoadProjectInternal()) {
            return;
        }

        const projectId = this.artifact.projectId;
        const artifactId = this.artifact.id;
        const openProjectLoadingId = this.loadingOverlayService.beginLoading();
        const openProjects = _.map(this.projectExplorerService.projects, "model.id");

        this.projectExplorerService.openProjectAndExpandToNode(projectId, artifactId)
            .finally(() => {
                //(eventCollection, action, label?, value?, custom?, jQEvent?
                const label = _.includes(openProjects, projectId) ? "duplicate" : "new";
                /*this.analytics.trackEvent("open", "project", label, projectId, {
                    openProjects: openProjects
                });*/

                this.loadingOverlayService.endLoading(openProjectLoadingId);
            });
    }

    private createToolbarActions(): void {
        const saveAction = new SaveAction(this.artifact, this.localization, this.messageService, this.loadingOverlayService);
        const publishAction = new PublishAction(this.artifact, this.localization, this.messageService, this.loadingOverlayService);
        const discardAction = new DiscardAction(this.artifact, this.localization, this.messageService,
            this.projectExplorerService, this.navigationService);
        const refreshAction = new RefreshAction(this.artifact, this.localization, this.projectExplorerService, this.loadingOverlayService,
            this.metadataService, this.navigationService, this.mainBreadcrumbService);
        const moveCopyAction = new MoveCopyAction(this.$q, this.$timeout, this.artifact, this.localization, this.messageService, this.projectExplorerService,
            this.dialogService, this.navigationService, this.loadingOverlayService);
        const addToCollectionAction = new AddToCollectionAction(this.$q, this.artifact, this.localization, this.messageService, this.projectExplorerService,
            this.dialogService, this.navigationService, this.loadingOverlayService, this.collectionService, this.itemInfoService);
        const buttonGroup = new BPButtonGroupAction(saveAction, publishAction, discardAction, refreshAction);

        // expanded toolbar
        this.toolbarActions.push(moveCopyAction, addToCollectionAction, buttonGroup);

        // collapsed toolbar
        this.collapsedToolbarActions.push(buttonGroup);
        this.additionalMenuActions.push(...this.getNestedDropdownActions(moveCopyAction));
        this.additionalMenuActions.push(...this.getNestedDropdownActions(addToCollectionAction));

        this.createCustomToolbarActions(buttonGroup);

        this.collapsedToolbarActions.push(new BPMenuAction(this.localization.get("App_Toolbar_Menu"), ...this.additionalMenuActions));
    }

    protected createCustomToolbarActions(buttonGroup: BPButtonGroupAction): void {
        const openImpactAnalysisAction = new OpenImpactAnalysisAction(this.artifact, this.localization);
        const deleteAction = new DeleteAction(this.artifact, this.localization, this.messageService,
            this.projectExplorerService, this.loadingOverlayService, this.dialogService, this.navigationService);

        if (buttonGroup) {
            buttonGroup.actions.push(deleteAction);
        }

        this.toolbarActions.push(openImpactAnalysisAction);
        this.additionalMenuActions.push(new BPButtonOrDropdownSeparator(), openImpactAnalysisAction);
    }

    protected getNestedDropdownActions(actionsContainer: IBPDropdownAction): IBPButtonOrDropdownAction[] {
        const nestedActions: IBPButtonOrDropdownAction[] = [];

        if (actionsContainer.actions.length) {
            actionsContainer.actions.forEach((action: BPButtonOrDropdownAction) => nestedActions.push(action));
        }

        return nestedActions;
    }

    private onWidthResized(mainWindow: IMainWindow) {
        if (mainWindow.causeOfChange === ResizeCause.browserResize || mainWindow.causeOfChange === ResizeCause.sidebarToggle) {
            const pageHeading = document.getElementsByClassName("page-heading").item(0) as HTMLElement;
            const pageToolbar = document.querySelector(".page-heading .toolbar__container") as HTMLElement;

            // THIS WILL BE USED TO TOGGLE BETWEEN THE EXPANDED AND COLLAPSED TOOLBAR
            // if (pageHeading && pageToolbar) {
            //     this.isToolbarCollapsed = pageToolbar.offsetWidth > pageHeading.offsetWidth / 3;
            // }
        }
    }
}
