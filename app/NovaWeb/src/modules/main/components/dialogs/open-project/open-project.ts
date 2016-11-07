import * as angular from "angular";
import {ILocalizationService} from "../../../../core";
import {Helper, IDialogSettings, BaseDialogController} from "../../../../shared";
import {IColumn, IColumnRendererParams} from "../../../../shared/widgets/bp-tree-view/";
import {Models, Enums, AdminStoreModels, TreeViewModels} from "../../../models";
import {IProjectService} from "../../../../managers/project-manager/project-service";

export interface IOpenProjectController {
    errorMessage: string;
    isProjectSelected: boolean;
    selectedItem?: TreeViewModels.InstanceItemNodeVM;
    selectedDescription: string;

    // BpTreeView bindings
    rootNode: TreeViewModels.InstanceItemNodeVM;
    columns: IColumn[];
    onSelect: (vm: TreeViewModels.IViewModel<any>, isSelected: boolean) => any;
    onDoubleClick: (vm: TreeViewModels.IViewModel<any>) => any;
    onError: (reason: any) => any;
}

export class OpenProjectController extends BaseDialogController implements IOpenProjectController {
    public hasCloseButton: boolean = true;
    private _selectedItem: TreeViewModels.InstanceItemNodeVM;
    private _errorMessage: string;

    public factory: TreeViewModels.TreeNodeVMFactory;

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectService", "dialogSettings", "$sce"];

    constructor(private $scope: ng.IScope,
                private localization: ILocalizationService,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                private projectService: IProjectService,
                dialogSettings: IDialogSettings,
                private $sce: ng.ISCEService) {
        super($uibModalInstance, dialogSettings);
        this.factory = new TreeViewModels.TreeNodeVMFactory(projectService);
        this.rootNode = this.factory.createInstanceItemNodeVM({
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true);
    };

    //Dialog return value
    public get returnValue(): Models.IProject {
        if (this.isProjectSelected) {
            const model = this.selectedItem.model;
            return {
                id: model.id || -1,
                name: model.name || "",
                description: model.description || "",
                itemTypeId: Enums.ItemTypePredefined.Project,
                permissions: Enums.RolePermissions.Read // if the user can select it, it means he can read it
            } as Models.IProject;
        }
        return undefined;
    };

    public get errorMessage(): string {
        return this._errorMessage;
    }

    public get isProjectSelected(): boolean {
        return this.selectedItem && this.selectedItem.model.type === AdminStoreModels.InstanceItemType.Project;
    }

    public get selectedItem(): TreeViewModels.InstanceItemNodeVM {
        return this._selectedItem;
    }

    private _selectedDescription: string;

    public get selectedDescription() {
        return this._selectedDescription;
    }

    private setSelectedItem(item: TreeViewModels.InstanceItemNodeVM) {
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

    public rootNode: TreeViewModels.InstanceItemNodeVM;
    public columns: IColumn[] = [{
        headerName: this.localization.get("App_Header_Name"),
        cellClass: (vm: TreeViewModels.TreeViewNodeVM<any>) => vm.getCellClass(),
        isGroup: true,
        innerRenderer: (params: IColumnRendererParams) => {
            const node = <TreeViewModels.TreeViewNodeVM<any>>params.vm;
            if (params.vm instanceof TreeViewModels.InstanceItemNodeVM && node.model.type === AdminStoreModels.InstanceItemType.Project) {
                //TODO this listener is never removed
                // Need to use a cellRenderer "Component" with a destroy method, not a function.
                // See https://www.ag-grid.com/javascript-grid-cell-rendering/
                // Also need to upgrade ag-grid as destroy wasn't being called until 6.3.0
                // See https://www.ag-grid.com/change-log/changeLogIndex.php
                params.eGridCell.addEventListener("keydown", this.onEnterKeyPressed);
            }
            const name = Helper.escapeHTMLText(node.name);
            return `<span class="ag-group-value-wrapper"><i></i><span>${name}</span></span>`;
        }
    }];

    public onSelect = (vm: TreeViewModels.InstanceItemNodeVM, isSelected: boolean): void => {
        if (isSelected) {
            this.$scope.$applyAsync((s) => {
                this.setSelectedItem(vm);
            });
        }
    }

    public onDoubleClick = (vm: TreeViewModels.InstanceItemNodeVM): void => {
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
