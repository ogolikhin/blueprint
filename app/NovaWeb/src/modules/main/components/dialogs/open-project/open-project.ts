import {Helper, IDialogSettings, BaseDialogController} from "../../../../shared";
import {IColumn, IColumnRendererParams} from "../../../../shared/widgets/bp-tree-view/";
import {AdminStoreModels, TreeModels} from "../../../models";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IProjectService} from "../../../../managers/project-manager/project-service";
import {IArtifactManager, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";

export interface IOpenProjectController {
    errorMessage: string;
    isProjectSelected: boolean;
    selectedItem?: TreeModels.InstanceItemNodeVM;
    selectedDescription: string;

    // BpTreeView bindings
    rowData: TreeModels.InstanceItemNodeVM[];
    columns: IColumn[];
    onSelect: (vm: TreeModels.ITreeNodeVM<any>, isSelected: boolean) => any;
    onDoubleClick: (vm: TreeModels.ITreeNodeVM<any>) => any;
    onError: (reason: any) => any;
}

export class OpenProjectController extends BaseDialogController implements IOpenProjectController {
    public hasCloseButton: boolean = true;
    private _selectedItem: TreeModels.InstanceItemNodeVM;
    private _errorMessage: string;

    public factory: TreeModels.TreeNodeVMFactory;

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectService", "artifactManager", "statefulArtifactFactory", "dialogSettings", "$sce"];

    constructor(private $scope: ng.IScope,
                private localization: ILocalizationService,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                private projectService: IProjectService,
                private artifactManager: IArtifactManager,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                dialogSettings: IDialogSettings,
                private $sce: ng.ISCEService) {
        super($uibModalInstance, dialogSettings);
        this.factory = new TreeModels.TreeNodeVMFactory(projectService, artifactManager, statefulArtifactFactory);
        this.rowData = [this.factory.createInstanceItemNodeVM({
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true)];
    };

    //Dialog return value
    public get returnValue(): AdminStoreModels.IInstanceItem {
        return this.isProjectSelected ? this.selectedItem.model : undefined;
    };

    public get errorMessage(): string {
        return this._errorMessage;
    }

    public get isProjectSelected(): boolean {
        return this.selectedItem && this.selectedItem.model.type === AdminStoreModels.InstanceItemType.Project;
    }

    public get selectedItem(): TreeModels.InstanceItemNodeVM {
        return this._selectedItem;
    }

    private _selectedDescription: string;

    public get selectedDescription() {
        return this._selectedDescription;
    }

    private setSelectedItem(item: TreeModels.InstanceItemNodeVM) {
        this._selectedItem = item;

        const description = this.selectedItem.model.description;
        if (description) {
            //TODO Why do we need this? Project descriptions are plain text and Instance Folders can't have descriptions.
            const virtualDiv = window.document.createElement("DIV");
            virtualDiv.innerHTML = description;

            const aTags = virtualDiv.querySelectorAll("a");
            for (let a = 0; a < aTags.length; a++) {
                aTags[a].setAttribute("target", "_blank");
            }
            this._selectedDescription = this.$sce.trustAsHtml(Helper.stripWingdings(virtualDiv.innerHTML));
            this.selectedItem.model.description = this.selectedDescription.toString();
        } else {
            this._selectedDescription = undefined;
        }
    }

    private onEnterKeyPressed = (e: KeyboardEvent) => {
        const key = e.which || e.keyCode;
        if (key === 13) {
            //user pressed Enter key on project
            this.ok();
        }
    };

    // BpTreeView bindings

    public rowData: TreeModels.InstanceItemNodeVM[];
    public columns: IColumn[] = [{
        headerName: this.localization.get("App_Header_Name"),
        cellClass: (vm: TreeModels.ITreeNodeVM<any>) => vm.getCellClass(),
        isGroup: true,
        cellRenderer: (params: IColumnRendererParams) => {
            const vm = params.data as TreeModels.ITreeNodeVM<any>;
            if (vm instanceof TreeModels.InstanceItemNodeVM && vm.model.type === AdminStoreModels.InstanceItemType.Project) {
                //TODO this listener is never removed
                // Need to use a cellRenderer "Component" with a destroy method, not a function.
                // See https://www.ag-grid.com/javascript-grid-cell-rendering/
                // Also need to upgrade ag-grid as destroy wasn't being called until 6.3.0
                // See https://www.ag-grid.com/change-log/changeLogIndex.php
                params.eGridCell.addEventListener("keydown", this.onEnterKeyPressed);
            }
            const label = Helper.escapeHTMLText(vm.getLabel());
            return `<i></i><span>${label}</span>`;
        }
    }];

    public onSelect = (vm: TreeModels.InstanceItemNodeVM, isSelected: boolean): void => {
        if (isSelected) {
            this.$scope.$applyAsync((s) => {
                this.setSelectedItem(vm);
            });
        }
    }

    public onDoubleClick = (vm: TreeModels.InstanceItemNodeVM): void => {
        if (vm.model.type === AdminStoreModels.InstanceItemType.Project) {
            this.$scope.$applyAsync((s) => {
                this.setSelectedItem(vm);
                this.ok();
            });
        }
    }

    public onError = (reason: any): void => {
        //close dialog on authentication error
        this._errorMessage = this.localization.get("Project_NoProjectsAvailable");
    }
}
