import {TreeModels, AdminStoreModels} from "../../models";
import {Helper} from "../../../shared";
import {IProjectManager, IArtifactManager} from "../../../managers";
import {IItemChangeSet} from "../../../managers/artifact-manager";
import {ISelectionManager} from "../../../managers/selection-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {IMessageService} from "../../../core/messages/message.svc";
import {IBPTreeViewControllerApi, IColumn, IColumnRendererParams} from "../../../shared/widgets/bp-tree-view";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {IAnalyticsProvider} from "../analytics/analyticsProvider";
import {ILocalizationService} from "../../../core/localization/localizationService";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = ProjectExplorerController;
    public transclude: boolean = true;
}

export interface IProjectExplorerController {
    // BpTree bindings
    treeApi: IBPTreeViewControllerApi;
    projects: TreeModels.StatefulArtifactNodeVM[];
    columns: any[];
    onSelect: (vm: TreeModels.ITreeNodeVM<any>, isSelected: boolean) => void;
    onError: (reason: any) => any;
    onGridReset: (isExpanding: boolean) => void;
}

export class ProjectExplorerController implements IProjectExplorerController {
    private subscribers: Rx.IDisposable[];
    private selectedArtifactSubscriber: Rx.IDisposable;
    private numberOfProjectsOnLastLoad: number;

    public static $inject: [string] = [
        "$q",
        "projectManager",
        "artifactManager",
        "navigationService",
        "selectionManager",
        "messageService",
        "projectService",
        "loadingOverlayService",
        "analytics",
        "localization"
    ];

    constructor(private $q: ng.IQService,
                private projectManager: IProjectManager,
                private artifactManager: IArtifactManager,
                private navigationService: INavigationService,
                private selectionManager: ISelectionManager,
                private messageService: IMessageService,
                private projectService: IProjectService,
                private loadingOverlayService: ILoadingOverlayService,
                private analytics: IAnalyticsProvider,
                public localization: ILocalizationService) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onLoadProject, this),
            this.selectionManager.explorerArtifactObservable.filter(artifact => !!artifact).subscribeOnNext(this.setSelectedNode, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        if (this.selectedArtifactSubscriber) {
            this.selectedArtifactSubscriber.dispose();
        }
    }

    private setSelectedNode(artifactId: number) {
        // If this method is called after onLoadProject, but before onGridReset, the artifact will be
        // selected in onGridReset. This allows code that refreshes explorer, then naviages to a new
        // artifact, to work as expected.
        if (this.isLoading) {
            this.pendingSelectedArtifactId = artifactId;
        } else if (this.treeApi.setSelected((vm: TreeModels.ITreeNodeVM<any>) => vm.model.id === artifactId)) {
            this.treeApi.ensureNodeVisible((vm: TreeModels.ITreeNodeVM<any>) => vm.model.id === artifactId);
        } else {
            this.treeApi.deselectAll();
        }
    }

    private _selected: TreeModels.ITreeNodeVM<any>;
    private get selected() {
        return this._selected;
    }

    private set selected(value: TreeModels.ITreeNodeVM<any>) {
        this._selected = value;

        //Dispose of old subscriber and subscribe to new artifact.
        if (this.selectedArtifactSubscriber) {
            this.selectedArtifactSubscriber.dispose();
        }

        if (value) {
            this.selectedArtifactSubscriber = value.model.getProperyObservable()
                .distinctUntilChanged(changes => changes.item && changes.item.name)
                .subscribeOnNext(this.onSelectedArtifactChange);
        }
    }

    private isLoading: boolean;
    private pendingSelectedArtifactId: number;

    private onLoadProject = (projects: TreeModels.StatefulArtifactNodeVM[]) => {
        this.isLoading = true;
        this.projects = projects.slice(0); // create a copy
    }

    public isProjectTreeVisible(): boolean {
        return this.projects && this.projects.length > 0;
    }
    
    public openProject(): void {
        const selectedArtifact = this.selectionManager.getArtifact();
        if (!selectedArtifact || !selectedArtifact.projectId) {
            this.projectManager.openProjectWithDialog();
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

    public onGridReset(isExpanding: boolean): void {
        this.isLoading = false;

        if (isExpanding) {
            return;
        }

        const selectedArtifactId = this.selected ? this.selected.model.id : undefined;
        let navigateToId: number;
        if (this.projects && this.projects.length > 0) {
            if (this.pendingSelectedArtifactId) {
                navigateToId = this.pendingSelectedArtifactId;
                this.pendingSelectedArtifactId = undefined;
            } else if (!selectedArtifactId || this.numberOfProjectsOnLastLoad !== this.projects.length) {
                navigateToId = this.projects[0].model.id;
            } else if (this.projects.some(vm => Boolean(vm.getNode(model => model.id === selectedArtifactId)))) {
                navigateToId = selectedArtifactId;
            } else if (this.projects.some(vm => Boolean(vm.getNode(model => model.id === this.selected.model.parentId)))) {
                navigateToId = this.selected.model.parentId;
            } else if (this.projects.some(vm => Boolean(vm.getNode(model => model.id === this.selected.model.projectId)))) {
                navigateToId = this.selected.model.projectId;
            }
        }

        this.numberOfProjectsOnLastLoad = this.projects.length;

        if (_.isFinite(navigateToId)) {
            if (navigateToId !== selectedArtifactId) {
                this.treeApi.setSelected((vm: TreeModels.ITreeNodeVM<any>) => vm.model.id === navigateToId);
                this.treeApi.ensureNodeVisible((vm: TreeModels.ITreeNodeVM<any>) => vm.model.id === navigateToId);
            } else {
                this.navigationService.reloadParentState();
            }
        } else {
            this.treeApi.deselectAll();
            this.selected = undefined;
        }
    };

    private onSelectedArtifactChange = (changes: IItemChangeSet) => {
        //If the artifact's name changes (on refresh), we refresh specific node only .
        //To prevent update treenode name while editing the artifact details, use it only for clean artifact.
        if (changes.item) {
            this.treeApi.refreshRows((vm: TreeModels.ITreeNodeVM<any>) => vm.model.id === changes.item.id);
        }
    };

    // BpTree bindings

    public treeApi: IBPTreeViewControllerApi;
    public projects: TreeModels.StatefulArtifactNodeVM[];
    public columns: IColumn[] = [{
        cellClass: (vm: TreeModels.ITreeNodeVM<any>) => vm.getCellClass(),
        isGroup: true,
        cellRenderer: (params: IColumnRendererParams) => {
            const vm = params.data as TreeModels.ITreeNodeVM<any>;
            const icon = vm.getIcon();
            const label = Helper.escapeHTMLText(vm.getLabel());
            return `<a ui-sref="main.item({id: ${vm.model.id}})" ng-click="$event.preventDefault()" class="explorer__node-link">` +
                   `${icon}<span>${label}</span></a>`;
        }
    }];

    public onSelect = (vm: TreeModels.ITreeNodeVM<any>, isSelected: boolean): void => {
        if (isSelected) {
            this.selected = vm;
            this.navigationService.navigateTo({id: vm.model.id});
        }
    };

    public onError = (reason: any): void => {
        if (reason) {
            this.messageService.addError(reason["message"] || "Artifact_NotFound");
        }
    }
}
