import "angular";
import { ColDef } from "ag-grid/main";
import { Helper } from "../../../../shared/";
import { ILocalizationService } from "../../../../core";
import { ArtifactPickerNodeVM, InstanceItemNodeVM, ArtifactNodeVM, SubArtifactNodeVM } from "./bp-artifact-picker-node-vm";
import { IDialogSettings, BaseDialogController } from "../../../../shared/";
import { Models } from "../../../models";
import { IProjectManager } from "../../../../managers";
import { IProjectService } from "../../../../managers/project-manager/project-service";

export interface IArtifactPickerController {
    project: Models.IProject;
    rootNode: ArtifactPickerNodeVM<any>;
    columnDefs: any[];
    onSelect: (vm: ArtifactPickerNodeVM<any>) => void;
}

export interface IArtifactPickerOptions {
    selectableItemTypes?: Models.ItemTypePredefined[];
    showSubArtifacts?: boolean;
}

export class ArtifactPickerController extends BaseDialogController implements IArtifactPickerController {

    public hasCloseButton: boolean = true;
    private _selectedItem: Models.IItem;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "$scope",
        "localization",
        "projectManager",
        "projectService",
        "dialogData"];
        
    constructor(
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private projectService: IProjectService,
        private dialogData: IArtifactPickerOptions
    ) {
        super($instance, dialogSettings);
        this.project = this.projectManager.getSelectedProject();

        $scope.$on("$destroy", () => {
            if (this.columnDefs) {
                this.columnDefs[0].cellClass = undefined;
                this.columnDefs[0].cellRendererParams["innerRenderer"] = undefined;
                this.columnDefs = undefined;
            }
            this.onSelect = undefined;
        });
    };

    //Dialog return value
    public get returnValue(): any {
        return this._selectedItem;
    };

    private setSelectedItem(item: Models.IItem) {
        this.$scope.$applyAsync((s) => {
            this._selectedItem = this.isItemSelectable(item) ? item : undefined;
        });
    }

    private isItemSelectable(item: Models.IItem): boolean {
        return !(item &&
            this.dialogData &&
            this.dialogData.selectableItemTypes &&
            this.dialogData.selectableItemTypes.length > 0 &&
            this.dialogData.selectableItemTypes.indexOf(item.predefinedType) === -1);
    }

    public columnDefs: ColDef[] = [{
        headerName: "",
        field: "name",
        cellClass: function (params) {
            const vm = params.data as ArtifactPickerNodeVM<any>;
            return vm.getCellClass();
        },
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                const vm = params.data as ArtifactPickerNodeVM<any>;
                const icon = vm.getIcon();
                const name = Helper.escapeHTMLText(vm.name);
                return `<span class="ag-group-value-wrapper">${icon}<span>${name}</span></span>`;
            },
            padding: 20
        },
        suppressMenu: true,
        suppressSorting: true,
    }];

    public rootNode: InstanceItemNodeVM;

    public onSelect = (vm: ArtifactPickerNodeVM<any>) => {
        if (vm instanceof ArtifactNodeVM || vm instanceof SubArtifactNodeVM) {
            this.setSelectedItem(vm.model);
        } else {
            this.setSelectedItem(undefined);
            if (vm instanceof InstanceItemNodeVM && vm.model.type === Models.ProjectNodeType.Project) {
                this.project = vm.model;
            }
        }
    };

    private _project: Models.IProject;

    public get project(): Models.IProject {
        return this._project;
    }

    public set project(project: Models.IProject) {
        this.setSelectedItem(undefined);
        this._project = project;
        if (project) {
            this.rootNode = new InstanceItemNodeVM(this.projectManager, this.projectService, this.dialogData, {
                id: project.id,
                type: Models.ProjectNodeType.Project,
                name: project.name,
                hasChildren: project.hasChildren,
            } as Models.IProjectNode, true);
        } else {
            this.rootNode = new InstanceItemNodeVM(this.projectManager, this.projectService, this.dialogData, {
                id: 0,
                type: Models.ProjectNodeType.Folder,
                name: "",
                hasChildren: true
            } as Models.IProjectNode, true);
        }
    }
}
