import * as angular from "angular";
import {IColumn} from "../../../shared/widgets/bp-tree-view/";
import {Helper} from "../../../shared/";
import {ILocalizationService} from "../../../core";
import {ArtifactPickerNodeVM, InstanceItemNodeVM} from "./bp-artifact-picker-node-vm";
import {IDialogSettings, BaseDialogController} from "../../../shared/";
import {Models, SearchServiceModels} from "../../models";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IProjectService} from "../../../managers/project-manager/project-service";

export class ArtifactPickerDialogController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private selectedVMs: ArtifactPickerNodeVM<any>[];

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IArtifactPickerOptions) {
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
        isOneProjectLevel: "<",
        onSelectionChanged: "&?"
    };
}

export interface IArtifactPickerOptions {
    selectableItemTypes?: Models.ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showSubArtifacts?: boolean;
    isOneProjectLevel?: boolean;
}

export interface IArtifactPickerController extends IArtifactPickerOptions {
    searchText: string;
    clearSearch(): void;
    search(): void;
    isSearching: boolean;
    searchResults: SearchServiceModels.ISearchResult[];
    project: Models.IProjectNode;
    rootNode: InstanceItemNodeVM;
    columns: IColumn[];
    onSelect: (vm: ArtifactPickerNodeVM<any>, isSelected: boolean, selectedVMs: ArtifactPickerNodeVM<any>[]) => void;
    setProject(project: Models.IProjectNode): void;
    clearProject(): void;
}

export class BpArtifactPickerController implements ng.IComponentController, IArtifactPickerController {
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showSubArtifacts: boolean;
    public isOneProjectLevel: boolean;
    public onSelectionChanged: (params: {selectedVMs: ArtifactPickerNodeVM<any>[]}) => void;
    public searchText: string = "";
    public isSearching: boolean = false;
    public searchResults: SearchServiceModels.ISearchResult[];

    static $inject = [
        "$scope",
        "localization",
        "artifactManager",
        "projectManager",
        "projectService"
    ];

    constructor(private $scope: ng.IScope,
                private localization: ILocalizationService,
                private artifactManager: IArtifactManager,
                private projectManager: IProjectManager,
                private projectService: IProjectService) {
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.showSubArtifacts = angular.isDefined(this.showSubArtifacts) ? this.showSubArtifacts : false;
        this.isOneProjectLevel = angular.isDefined(this.isOneProjectLevel) ? this.isOneProjectLevel : false;
    };

    public $onInit(): void {
        const selectedArtifact = this.artifactManager.selection.getArtifact();
        const projectId = selectedArtifact ? selectedArtifact.projectId : undefined;
        if (projectId) {
            const project = this.projectManager.getProject(projectId);
            if (project) {
                this.setProject({
                    id: project.id,
                    type: Models.ProjectNodeType.Project,
                    name: project.name,
                    hasChildren: project.hasChildren
                } as Models.IProjectNode);
            } else {
                this.projectService.getProject(projectId)
                    .then(project => this.setProject(project));
            }
        } else {
            this.clearProject();
        }
    }

    public $onDestroy(): void {
        if (this.columns) {
            this.columns[0].cellClass = undefined;
            this.columns[0].innerRenderer = undefined;
            this.columns = undefined;
        }
        this.onSelect = undefined;
        this.projectService.abort();
    }

    private setSelectedVMs(items: ArtifactPickerNodeVM<any>[]): void {
        this.$scope.$applyAsync((s) => {
            if (this.onSelectionChanged) {
                this.onSelectionChanged({selectedVMs: items});
            }
        });
    }

    public clearSearch(): void {
        this.searchText = undefined;
        this.searchResults = undefined;
    }

    public search(): void {
        if (this.searchText) {
            this.isSearching = true;
            if (this.project) {
                this.projectService.searchItemNames({query: this.searchText, projectIds: [this.project.id], includeArtifactPath: true}).then(result => {
                    this.searchResults = result.items;
                    this.isSearching = false;
                });
            } else {
                this.projectService.searchProjects({query: this.searchText}).then(result => {
                    this.searchResults = result.items;
                    this.isSearching = false;
                });
            }
        }
    }

    public columns: IColumn[] = [{
        cellClass: (vm: ArtifactPickerNodeVM<any>) => vm.getCellClass(),
        isGroup: true,
        innerRenderer: (vm: ArtifactPickerNodeVM<any>, eGridCell: HTMLElement) => {
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
                this.setProject(vm.model);
            }
        } else {
            this.setSelectedVMs(selectedVMs);
        }
    };

    public project: Models.IProjectNode;

    public setProject(project: Models.IProjectNode): void {
        this.clearSearch();
        this.setSelectedVMs([]);
        this.project = project;
        this.currentSelectionMode = this.selectionMode || "single";
        this.rootNode = new InstanceItemNodeVM(this.artifactManager, this.projectService, this, project, true);
    }

    public clearProject(): void {
        this.clearSearch();
        this.setSelectedVMs([]);
        this.project = undefined;
        this.currentSelectionMode = "single";
        this.rootNode = new InstanceItemNodeVM(this.artifactManager, this.projectService, this, {
            id: 0,
            type: Models.ProjectNodeType.Folder,
            name: "",
            hasChildren: true
        } as Models.IProjectNode, true);
    }
}
