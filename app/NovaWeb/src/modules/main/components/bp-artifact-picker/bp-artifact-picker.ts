import * as angular from "angular";
import { IColumn } from "../../../shared/widgets/bp-tree-view/";
import { Helper } from "../../../shared/";
import { ILocalizationService } from "../../../core";
import { ArtifactPickerNodeVM, InstanceItemNodeVM } from "./bp-artifact-picker-node-vm";
import { IDialogSettings, BaseDialogController } from "../../../shared/";
import { Models } from "../../models";
import { IProjectManager } from "../../../managers";
import { IProjectService } from "../../../managers/project-manager/project-service";

export class ArtifactPickerDialogController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private selectedVMs: ArtifactPickerNodeVM<any>[];

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor(
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        public dialogData: IArtifactPickerOptions
    ) {
        super($instance, dialogSettings);
    };

    public get returnValue(): any[] {
        return this.selectedVMs.map(vm => vm.model);
    };

    public onSelectionChanged(selectedVMs: ArtifactPickerNodeVM<any>[]) {
        this.selectedVMs = selectedVMs;
    }
}


export class BpArtifactPicker implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactPickerController;
    public template: string = require("./bp-artifact-picker.html");
    public bindings: {[binding: string]: string} = {
        selectableItemTypes: "<",
        selectionMode: "<",
        showSubArtifacts: "<",
        onSelectionChanged: "&?"
    };
}

export interface IArtifactPickerOptions {
    selectableItemTypes?: Models.ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showSubArtifacts?: boolean;
}

export class BpArtifactPickerController implements ng.IComponentController, IArtifactPickerOptions {
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showSubArtifacts: boolean;
    public onSelectionChanged: (params: {selectedVMs: ArtifactPickerNodeVM<any>[]}) => void;

    static $inject = [
        "$scope",
        "localization",
        "projectManager",
        "projectService"
    ];
        
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private projectService: IProjectService
    ) {
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.showSubArtifacts = angular.isDefined(this.showSubArtifacts) ? this.showSubArtifacts : false;
    };

    public $onInit(): void {
        this.project = this.projectManager.getSelectedProject();
    }

    public $onDestroy(): void {
        if (this.columns) {
            this.columns[0].cellClass = undefined;
            this.columns[0].innerRenderer = undefined;
            this.columns = undefined;
        }
        this.onSelect = undefined;
    }

    private setSelectedVMs(items: ArtifactPickerNodeVM<any>[]) {
        this.$scope.$applyAsync((s) => {
            if (this.onSelectionChanged) {
                this.onSelectionChanged({ selectedVMs: items });
            }
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

    public currentSelectionMode: "single" | "multiple" | "checkbox";
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
            this.currentSelectionMode = this.selectionMode || "single";
            this.rootNode = new InstanceItemNodeVM(this.projectManager, this.projectService, this, {
                id: project.id,
                type: Models.ProjectNodeType.Project,
                name: project.name,
                hasChildren: project.hasChildren,
            } as Models.IProjectNode, true);
        } else {
            this.currentSelectionMode = "single";
            this.rootNode = new InstanceItemNodeVM(this.projectManager, this.projectService, this, {
                id: 0,
                type: Models.ProjectNodeType.Folder,
                name: "",
                hasChildren: true
            } as Models.IProjectNode, true);
        }
    }
}
