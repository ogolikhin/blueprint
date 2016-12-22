import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {ItemTypePredefined, LockedByEnum} from "../../models/enums";
import {IWindowManager, IMainWindow, ResizeCause} from "../../services";
import {
    IArtifactState,
    IArtifactManager,
    IStatefulArtifact,
    IMetaDataService,
    IItemChangeSet
} from "../../../managers/artifact-manager";
import {IProjectManager} from "../../../managers/project-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";
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
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {Message, MessageType} from "../../../core/messages/message";
import {IMessageService} from "../../../core/messages/message.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {IMainBreadcrumbService} from "../bp-page-content/mainbreadcrumb.svc";
import {IAnalyticsProvider} from "../analytics/analyticsProvider";

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactInfoController;
    public transclude: boolean = true;
}

export class BpArtifactInfoController {
    static $inject: [string] = [
        "$q",
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
        "metadataService",
        "mainbreadcrumbService",
        "analytics"
    ];

    protected subscribers: Rx.IDisposable[] = [];
    protected artifact: IStatefulArtifact;
    public breadcrumbLinks: IBreadcrumbLink[];
    public isReadonly: boolean;
    public isChanged: boolean;
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
    public toolbarActions: IBPAction[] = [];
    public collapsedToolbarActions: IBPAction[] = [];
    public additionalMenuActions: IBPButtonOrDropdownAction[] = [];
    public isToolbarCollapsed: boolean = true;
    public historicalMessage: string;

    constructor(public $q: ng.IQService,
                public $scope: ng.IScope,
                private $element: ng.IAugmentedJQuery,
                protected artifactManager: IArtifactManager,
                protected localization: ILocalizationService,
                protected messageService: IMessageService,
                protected dialogService: IDialogService,
                protected windowManager: IWindowManager,
                protected loadingOverlayService: ILoadingOverlayService,
                protected navigationService: INavigationService,
                protected projectManager: IProjectManager,
                protected metadataService: IMetaDataService,
                protected mainBreadcrumbService: IMainBreadcrumbService,
                protected analytics: IAnalyticsProvider) {
        this.initProperties();

        this.breadcrumbLinks = [];
    }

    public $onInit() {
        this.artifact = this.artifactManager.selection.getArtifact();

        if (this.artifact) {
            this.createToolbarActions();

            this.subscribers.push(
                this.artifact.getObservable()
                    .subscribeOnNext(this.onArtifactLoaded, this),
                this.artifact.artifactState.onStateChange
                    .debounce(100)
                    .subscribeOnNext(this.onArtifactStateChanged, this),
                this.artifact.getProperyObservable()
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

    protected onArtifactLoaded(): void {
        if (this.artifact) {
            this.updateProperties(this.artifact);

            if (this.artifact.artifactState.historical && !this.artifact.artifactState.deleted) {
                const publishedDate = this.localization.current.formatShortDateTime(this.artifact.lastEditedOn);
                const publishedBy = this.artifact.lastEditedBy.displayName;
                this.historicalMessage = `Version ${this.artifact.version}, published by ${publishedBy} on ${publishedDate}`;
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

        this.initStateProperties();
    }

    private initStateProperties() {
        this.isReadonly = false;
        this.isChanged = false;
        this.selfLocked = false;

        if (this.lockMessage) {
            this.messageService.deleteMessageById(this.lockMessage.id);
            this.lockMessage = null;
        }
    }

    private updateProperties(artifact: IStatefulArtifact): void {
        this.initProperties();

        this.artifactName = artifact.name;
        this.artifactTypeId = artifact.itemTypeId;
        this.artifactTypeIconId = artifact.itemTypeIconId;
        this.hasCustomIcon = _.isFinite(artifact.itemTypeIconId);

        this.isLegacy = artifact.predefinedType === ItemTypePredefined.Storyboard ||
            artifact.predefinedType === ItemTypePredefined.GenericDiagram ||
            artifact.predefinedType === ItemTypePredefined.BusinessProcess ||
            artifact.predefinedType === ItemTypePredefined.UseCase ||
            artifact.predefinedType === ItemTypePredefined.UseCaseDiagram ||
            artifact.predefinedType === ItemTypePredefined.UIMockup ||
            artifact.predefinedType === ItemTypePredefined.DomainDiagram ||
            artifact.predefinedType === ItemTypePredefined.Glossary;

        if (artifact.itemTypeId === ItemTypePredefined.Collections && artifact.predefinedType === ItemTypePredefined.CollectionFolder) {
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

        switch (state.lockedBy) {
            case LockedByEnum.CurrentUser:
                this.selfLocked = true;
                break;

            case LockedByEnum.OtherUser:
                let msg = state.lockOwner ? "Locked by " + state.lockOwner : "Locked ";
                if (state.lockDateTime) {
                    msg += " on " + this.localization.current.formatShortDateTime(state.lockDateTime);
                }
                msg += ".";
                this.messageService.addMessage(this.lockMessage = new Message(MessageType.Lock, msg));
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

        const project = this.projectManager.getProject(this.artifact.projectId);

        return !project;
    }

    public loadProject(): void {
        if (!this.canLoadProjectInternal()) {
            return;
        }

        const projectId = this.artifact.projectId;
        const artifactId = this.artifact.id;

        const openProjectLoadingId = this.loadingOverlayService.beginLoading();

        let openProjects = _.map(this.projectManager.projectCollection.getValue(), "model.id");
        this.projectManager.openProjectAndExpandToNode(projectId, artifactId)
            .finally(() => {
                //(eventCollection, action, label?, value?, custom?, jQEvent?
                const label = _.includes(openProjects, projectId) ? "duplicate" : "new";
                this.analytics.trackEvent("open", "project", label, projectId, {
                    openProjects: openProjects
                });

                this.loadingOverlayService.endLoading(openProjectLoadingId);
            });
    }

    private createToolbarActions(): void {
        const saveAction = new SaveAction(this.artifact, this.localization, this.messageService, this.loadingOverlayService);
        const publishAction = new PublishAction(this.artifact, this.localization, this.messageService, this.loadingOverlayService);
        const discardAction = new DiscardAction(this.artifact, this.localization, this.messageService,
            this.projectManager, this.loadingOverlayService, this.navigationService);
        const refreshAction = new RefreshAction(this.artifact, this.localization, this.projectManager, this.loadingOverlayService,
            this.metadataService, this.mainBreadcrumbService);
        const moveCopyAction = new MoveCopyAction(this.$q, this.artifact, this.localization, this.messageService, this.projectManager,
            this.dialogService, this.navigationService, this.loadingOverlayService);
        const addToCollectionAction = new AddToCollectionAction(this.$q, this.artifact, this.localization, this.messageService, this.projectManager,
            this.dialogService, this.navigationService, this.loadingOverlayService);
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
        const deleteAction = new DeleteAction(this.artifact, this.localization, this.messageService, this.artifactManager,
            this.projectManager, this.loadingOverlayService, this.dialogService, this.navigationService);

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
            const pageHeading = document.querySelector(".page-heading") as HTMLElement;
            const pageToolbar = document.querySelector(".page-heading .page-toolbar__container") as HTMLElement;

            // THIS WILL BE USED TO TOGGLE BETWEEN THE EXPANDED AND COLLAPSED TOOLBAR
            // if (pageHeading && pageToolbar) {
            //     this.isToolbarCollapsed = pageToolbar.offsetWidth > pageHeading.offsetWidth / 3;
            // }
        }
    }
}
