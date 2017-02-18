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
}

export class ProjectExplorerController implements IProjectExplorerController {
    private subscribers: Rx.IDisposable[];
    private numberOfProjectsOnLastLoad: number;

    public static $inject: [string] = [
        "$log",
        "navigationService",
        "selectionManager",
        "$state",
        "projectExplorerService",
        "localization"
    ];

    constructor(private $log: ng.ILogService,
                private navigationService: INavigationService,
                private selectionManager: ISelectionManager,
                private $state: ng.ui.IStateService,
                public projectExplorerService: IProjectExplorerService,
                public localization: ILocalizationService) {
    }

    public $onInit() {
        this.subscribers = [
            this.selectionManager.explorerArtifactObservable
                .filter(artifact => !!artifact)
                // Selection change always causes a property change, so skip that
                .flatMap((artifact: IStatefulArtifact) => artifact.getPropertyObservable().skip(1))
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

    public isProjectTreeVisible(): boolean {
        return this.projectExplorerService.projects && this.projectExplorerService.projects.length > 0;
    }

    private onSelectedArtifactPropertyChange(changes: IItemChangeSet) {
        //If the artifact's name changes (on refresh), we refresh specific node only .
        //To prevent update treenode name while editing the artifact details, use it only for clean artifact.
        if (changes.item) {
            this.treeApi.refreshRows((vm: ExplorerNodeVM) => {
                if (vm.model.id === changes.item.id) {
                    vm.updateModel(changes);
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

    public setSelectedNode() {
        const selectedId = this.projectExplorerService.getSelectionId();
        this.$log.debug("bpExplorer.setSelectedNode(): " + selectedId);

        if (_.isFinite(selectedId) && this.treeApi.setSelected((vm: ExplorerNodeVM) => vm.model.id === selectedId)) {
            this.treeApi.ensureNodeVisible((vm: ExplorerNodeVM) => vm.model.id === selectedId);
            return;
        }

        this.treeApi.deselectAll();
    }
}
