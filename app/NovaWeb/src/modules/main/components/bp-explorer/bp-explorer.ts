import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";
import {IItemChangeSet} from "../../../managers/artifact-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {ISelectionManager} from "../../../managers/selection-manager";
import {Helper} from "../../../shared";
import {IBPTreeViewControllerApi, IColumn, IColumnRendererParams} from "../../../shared/widgets/bp-tree-view";
import {IProjectExplorerService} from "./project-explorer.service";
import {ExplorerNodeVM} from "../../models/tree-node-vm-factory";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = ProjectExplorerController;
}

export interface IProjectExplorerController {
    treeApi: IBPTreeViewControllerApi;
    columns: any[];
    onSelect: (vm: ExplorerNodeVM, isSelected: boolean) => void;
    onGridReset: (isExpanding: boolean) => void;
}

export class ProjectExplorerController implements IProjectExplorerController {
    private subscribers: Rx.IDisposable[];
    private numberOfProjectsOnLastLoad: number;

    public static $inject: [string] = [
        "$q",
        "$log",
        "navigationService",
        "selectionManager",
        "$state",
        "projectExplorerService",
        "localization"
    ];

    constructor(private $q: ng.IQService,
                private $log: ng.ILogService,
                private navigationService: INavigationService,
                private selectionManager: ISelectionManager,
                private $state: ng.ui.IStateService,
                public projectExplorerService: IProjectExplorerService,
                public localization: ILocalizationService) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        this.subscribers = [
            // this.projectManager.projectCollection.subscribeOnNext(this.onLoadProject, this),
            // this.selectionManager.explorerArtifactObservable
            //     .distinctUntilChanged(artifact => artifact ? artifact.id : -1)
            //     .map(artifact => {
            //         return !!artifact ? artifact.id : null;
            //     })
            //     .subscribeOnNext(this.setSelectedNode, this),
            this.selectionManager.explorerArtifactObservable
                .filter(artifact => !!artifact)
                .flatMap((artifact: IStatefulArtifact) => artifact.getProperyObservable())
                .subscribeOnNext(this.onSelectedArtifactPropertyChange, this)
        ];
    }

    public $onDestroy() {
        this.subscribers.forEach((subsciber: Rx.IDisposable) => subsciber.dispose());
        this.subscribers = [];
    }

    public navigateToUnpublishedChanges() {
        if (this.$state.current.name === "main.unpublished") {
            this.navigationService.reloadCurrentState();
        } else {
            this.$state.go("main.unpublished");
        }
    }

    public navigateToJobs() {
        if (this.$state.current.name === "main.jobs") {
            this.navigationService.reloadCurrentState();
        } else {
            this.$state.go("main.jobs");
        }
    }

    /**
     * If this method is called after onLoadProject, but before onGridReset, the artifact will be
     * selected in onGridReset. This allows code that refreshes explorer, then naviages to a new
     * artifact, to work as expected.
     * If selection becomes null, all nodes get deselected.
     * @param  artifactId - id of an artifact
     */
    // private setSelectedNode(artifactId: number) {
    //     this.$log.debug("setSelectedNode: " + artifactId);
    //
    //     if (!_.isFinite(artifactId)) {
    //         if (this.treeApi) {
    //             // this.selected = undefined;
    //             this.treeApi.deselectAll();
    //         }
    //         return;
    //     }
    //
    //     if (this.isLoading) {
    //         this.pendingSelectedArtifactId = artifactId;
    //     } else if (this.treeApi.setSelected((vm: ExplorerNodeVM) => vm.model.id === artifactId)) {
    //         this.treeApi.ensureNodeVisible((vm: ExplorerNodeVM) => vm.model.id === artifactId);
    //     } else {
    //         this.treeApi.deselectAll();
    //     }
    // }

    public isProjectTreeVisible(): boolean {
        return this.projectExplorerService.projects && this.projectExplorerService.projects.length > 0;
    }

    // private isMainAreaSelectedArtifactBelongsToOpeningProject(): boolean {
    //     return this.selectionManager.getArtifact().projectId === this.projects[0].model.id;
    // }

    // public onGridReset(isExpanding: boolean): void {
    //     this.isLoading = false;
    //     const selectedArtifactId = this.selected ? this.selected.model.id : undefined;
    //
    //     if (isExpanding) {
    //         if (selectedArtifactId) {
    //             this.treeApi.setSelected((vm: ExplorerNodeVM) => vm.model.id === selectedArtifactId);
    //         }
    //         return;
    //     }
    //
    //     let navigateToId: number;
    //     if (this.projects && this.projects.length > 0) {
    //         if (this.pendingSelectedArtifactId) {
    //             navigateToId = this.pendingSelectedArtifactId;
    //             this.pendingSelectedArtifactId = undefined;
    //             // For case when we open a project for loaded artifact in a main area. ("Load project" button in main area)
    //         } else if (this.numberOfProjectsOnLastLoad < this.projects.length &&
    //             this.selectionManager.getArtifact() &&
    //             // selectedArtifactId = undefined only if there is no projects open.
    //             // if there are some artifact pre selected in the main area before opening project
    //             // we need to check if this artifact is from opening project: this.projects[0] (opening project)
    //             (!selectedArtifactId ||
    //             (selectedArtifactId &&
    //             this.isMainAreaSelectedArtifactBelongsToOpeningProject()))) {
    //             if (!this.selectionManager.getArtifact().artifactState.historical) {
    //                 navigateToId = this.selectionManager.getArtifact().id;
    //             } else {
    //                 // for historical artifact we do not need to change selection in main area US3489
    //                 navigateToId = selectedArtifactId;
    //             }
    //         } else if (this.$state.current.name !== "main.unpublished" && (!selectedArtifactId ||
    // this.numberOfProjectsOnLastLoad !== this.projects.length)) {
    //             navigateToId = this.projects[0].model.id;
    //         } else if (this.projects.some(vm => Boolean(vm.getNode(model => model.id === selectedArtifactId)))) {
    //             navigateToId = selectedArtifactId;
    //         } else if (this.projects.some(vm => Boolean(vm.getNode(model => model.id === this.selected.model.parentId)))) {
    //             navigateToId = this.selected.model.parentId;
    //         } else if (this.projects.some(vm => Boolean(vm.getNode(model => model.id === this.selected.model.projectId)))) {
    //             navigateToId = this.selected.model.projectId;
    //         }
    //     }
    //
    //     this.numberOfProjectsOnLastLoad = this.projects.length;
    //
    //     if (_.isFinite(navigateToId)) {
    //         if (navigateToId !== selectedArtifactId) {
    //             this.treeApi.setSelected((vm: ExplorerNodeVM) => vm.model.id === navigateToId);
    //         } else {
    //             this.navigationService.reloadCurrentState();
    //         }
    //
    //         this.treeApi.ensureNodeVisible((vm: ExplorerNodeVM) => vm.model.id === navigateToId);
    //     } else {
    //         this.treeApi.deselectAll();
    //         this.selected = undefined;
    //     }
    // };

    public onGridReset(isExpanding: boolean): void {
        this.$log.debug("onGridReset: " + isExpanding);

        // if (_.isFinite(this.projectExplorerService.selectedNodeId)) {
        //     this.treeApi.setSelected((vm: ExplorerNodeVM) => vm.model.id === this.projectExplorerService.selectedNodeId);
        //     this.treeApi.ensureNodeVisible((vm: ExplorerNodeVM) => vm.model.id === this.projectExplorerService.selectedNodeId);
        // } else {
        //     this.treeApi.deselectAll();
        // }
    }

    private onSelectedArtifactPropertyChange(changes: IItemChangeSet) {
        //If the artifact's name changes (on refresh), we refresh specific node only .
        //To prevent update treenode name while editing the artifact details, use it only for clean artifact.
        if (changes.item && changes.change) {
            this.$log.debug("onSelectedArtifactPropertyChange");

            this.treeApi.refreshRows((vm: ExplorerNodeVM) => {
                if (vm.model.id === changes.item.id) {
                    if (changes.change.key in vm.model) {
                        vm.model[changes.change.key] = changes.change.value;
                    }
                    return true;
                }
                return false;
            });
        }
    }

    // BpTree bindings
    public treeApi: IBPTreeViewControllerApi;
    public columns: IColumn[] = [{
        cellClass: (vm: ExplorerNodeVM) => vm.getCellClass(),
        isGroup: true,
        cellRenderer: (params: IColumnRendererParams) => {
            const vm = params.data as ExplorerNodeVM;
            const icon = vm.getIcon();
            const label = Helper.escapeHTMLText(vm.getLabel());
            return `<a ui-sref="main.item({id: ${vm.model.id}})" class="explorer__node-link">` +
                `${icon}<span>${label}</span></a>`;
        }
    }];

    public onSelect = (vm: ExplorerNodeVM, isSelected: boolean): void => {
        this.$log.debug("onSelect");
        // this.treeApi.setSelected((node: ExplorerNodeVM) => node.model.id === vm.model.id);

        /*if (!this.resettingSelection && isSelected) {
            //Following has to be a const to restore current selection in case of faling navigation
            // const prevSelected = this.selected;
            // this.selected = vm;
            // this.navigationService.navigateTo({id: vm.model.id})
            //     .catch((err) => {
            //         this.resettingSelection = true;
            //         this.treeApi.setSelected(prevSelected);
            //     });
        }
        this.resettingSelection = false;
        */
    };

    public setSelectedNode() {
        const selectedId = this.projectExplorerService.selectedId;
        if (_.isFinite(selectedId) && this.treeApi.setSelected((vm: ExplorerNodeVM) => vm.model.id === selectedId)) {
            this.treeApi.ensureNodeVisible((vm: ExplorerNodeVM) => vm.model.id === selectedId);
            return;
        }

        this.treeApi.deselectAll();
    }
}
