import "angular";
import { IColumn } from "../../../../shared/widgets/bp-tree-view/";
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
    columns: IColumn[];
    onSelect: (vm: ArtifactPickerNodeVM<any>, isSelected: boolean, selectedVMs: ArtifactPickerNodeVM<any>[]) => void;
}

export interface IArtifactPickerOptions {
    selectableItemTypes?: Models.ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showSubArtifacts?: boolean;
}

export class ArtifactPickerController extends BaseDialogController implements IArtifactPickerController {

    public hasCloseButton: boolean = true;
    private _selectedVMs: ArtifactPickerNodeVM<any>[];

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
            if (this.columns) {
                this.columns[0].cellClass = undefined;
                this.columns[0].innerRenderer = undefined;
                this.columns = undefined;
            }
            this.onSelect = undefined;
        });
    };

    //Dialog return value
    public get returnValue(): any {
        return this._selectedVMs.map(vm => vm.model);
    };

    private setSelectedVMs(items: ArtifactPickerNodeVM<any>[]) {
        this.$scope.$applyAsync((s) => {
            this._selectedVMs = items;
        });
    }

    public columns: IColumn[] = [{
        cellClass: (vm: ArtifactPickerNodeVM<any>) => vm.getCellClass(),
        isGroup: true,
        innerRenderer: (vm: ArtifactPickerNodeVM<any>) => {
            const icon = vm.getIcon();
            const name = Helper.escapeHTMLText(vm.name);
            return `<span class="ag-group-value-wrapper">${icon}<span>${name}</span></span>`;
        }
    }];

    public selectionMode: "single" | "multiple" | "checkbox";
    public rootNode: InstanceItemNodeVM;

    public onSelect = (vm: ArtifactPickerNodeVM<any>, isSelected: boolean, selectedVMs: ArtifactPickerNodeVM<any>[]) => {
        if (vm instanceof InstanceItemNodeVM) {
            this.setSelectedVMs([]);
            if (vm.model.type === Models.ProjectNodeType.Project) {
                this.project = vm.model;
            }
        } else {
            this.setSelectedVMs(selectedVMs);
        }
    };

    private _project: Models.IProject;

    public get project(): Models.IProject {
        return this._project;
    }

    public set project(project: Models.IProject) {
        this.setSelectedVMs([]);
        this._project = project;
        if (project) {
            this.selectionMode = this.dialogData.selectionMode;
            this.rootNode = new InstanceItemNodeVM(this.projectManager, this.projectService, this.dialogData, {
                id: project.id,
                type: Models.ProjectNodeType.Project,
                name: project.name,
                hasChildren: project.hasChildren,
            } as Models.IProjectNode, true);
        } else {
            this.selectionMode = "single";
            this.rootNode = new InstanceItemNodeVM(this.projectManager, this.projectService, this.dialogData, {
                id: 0,
                type: Models.ProjectNodeType.Folder,
                name: "",
                hasChildren: true
            } as Models.IProjectNode, true);
        }
    }
}
