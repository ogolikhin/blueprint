import {Models, Enums} from "../../models";
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
    BPButtonGroupAction,
    BPMenuAction,
    IBPButtonOrDropdownAction,
    BPButtonOrDropdownSeparator
} from "../../../shared";
import {
    SaveAction,
    PublishAction,
    DiscardAction,
    RefreshAction,
    DeleteAction,
    OpenImpactAnalysisAction,
    MoveAction
} from "./actions";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {Message, MessageType} from "../../../core/messages/message";
import {IMessageService} from "../../../core/messages/message.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {IMainBreadcrumbService} from "../bp-page-content/mainbreadcrumb.svc";
import {ISelectionManager} from "../../../managers/selection-manager";
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
        "selectionManager",
        "analytics"
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
                protected selectionManager: ISelectionManager,
                protected analytics: IAnalyticsProvider) {
        this.initProperties();
        this.subscribers = [];
    }

    public $onInit() {
        this.subscribers.push(
            this.windowManager.mainWindow
                .subscribeOnNext(this.onWidthResized, this)
        );

        this.artifact = this.artifactManager.selection.getArtifact();

        if (this.artifact) {
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

        this.updateToolbarOptions(this.artifact);
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
        this.isLocked = false;
        this.selfLocked = false;

        if (this.lockMessage) {
            this.messageService.deleteMessageById(this.lockMessage.id);
            this.lockMessage = null;
        }
    }

    protected updateProperties(artifact: IStatefulArtifact): void {
        this.initProperties();

        if (!artifact) {
            return;
        }

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

        this.updateStateProperties(artifact.artifactState);
    }

    protected updateStateProperties(state: IArtifactState): void {
        this.isReadonly = state.readonly;
        this.isChanged = state.dirty;

        switch (state.lockedBy) {
            case Enums.LockedByEnum.CurrentUser:
                this.selfLocked = true;
                break;

            case Enums.LockedByEnum.OtherUser:
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
        const selectedArtifact = this.selectionManager.getArtifact();
        if (!selectedArtifact || !selectedArtifact.projectId) {
            return false;
        }

        const project = this.projectManager.getProject(selectedArtifact.projectId);

        return !project;
    }

    public loadProject(): void {
        const selectedArtifact = this.selectionManager.getArtifact();
        if (!selectedArtifact || !selectedArtifact.projectId) {
            return;
        }

        const projectId = selectedArtifact.projectId;
        const artifactId = selectedArtifact.id;

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

    public get artifactHeadingMinWidth() {
        let style = {};

        if (this.$element.length) {
            let container: HTMLElement = this.$element[0];
            let toolbar: Element = container.querySelector(".page-top-toolbar"); //TODO: removed
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
        this.toolbarActions = [];
        this.collapsedToolbarActions = [];
        if (artifact) {
            const saveAction = new SaveAction(this.artifact, this.localization, this.messageService, this.loadingOverlayService);
            const publishAction = new PublishAction(this.artifact, this.localization, this.messageService, this.loadingOverlayService);
            const discardAction = new DiscardAction(this.artifact, this.localization, this.messageService,
                this.projectManager, this.loadingOverlayService);
            const refreshAction = new RefreshAction(this.artifact, this.localization, this.projectManager, this.loadingOverlayService,
                this.metadataService, this.mainBreadcrumbService);
            const moveAction = new MoveAction(this.$q, this.artifact, this.localization, this.messageService, this.projectManager,
                this.dialogService);
            const deleteAction = new DeleteAction(this.artifact, this.localization, this.messageService, this.artifactManager,
                this.projectManager, this.loadingOverlayService, this.dialogService, this.navigationService);
            const openImpactAnalysisAction = new OpenImpactAnalysisAction(this.artifact, this.localization);

            // expanded toolbar
            this.toolbarActions.push(
                moveAction,
                new BPButtonGroupAction(saveAction, publishAction, discardAction, refreshAction, deleteAction)
            );
            //we don't want to show impact analysis on collection artifact page
            if (this.artifact.predefinedType !== Enums.ItemTypePredefined.ArtifactCollection) {
                this.toolbarActions.push(openImpactAnalysisAction);
            }
            // collapsed toolbar
            const dropdownSeparator = new BPButtonOrDropdownSeparator();

            this.collapsedToolbarActions.push(new BPButtonGroupAction(saveAction, publishAction, discardAction, refreshAction));
            this.additionalMenuActions.push(moveAction, deleteAction);
            //we don't want to show impact analysis on collection artifact page
            if (this.artifact.predefinedType !== Enums.ItemTypePredefined.ArtifactCollection) {
                this.additionalMenuActions.push(dropdownSeparator, openImpactAnalysisAction);
            }
            this.collapsedToolbarActions.push(
                new BPMenuAction(this.localization.get("App_Toolbar_Menu"), ...this.additionalMenuActions)
            );
        }
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
                let toolbar: Element = container.querySelector(".page-top-toolbar"); //TODO: removed
                let heading: Element = container.querySelector(".artifact-heading");
                if (heading && toolbar) {
                    angular.element(heading).css("max-width", (document.body.clientWidth - sidebarsWidth) < 2 * toolbar.clientWidth ?
                        "100%" : "calc(100% - " + toolbar.clientWidth + "px)");
                }
            }
        }
    }
}
