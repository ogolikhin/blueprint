import * as angular from "angular";
import {IColumn} from "../../../shared/widgets/bp-tree-view/";
import {Helper} from "../../../shared/";
import {ILocalizationService} from "../../../core";
import {IViewModel, ArtifactPickerNodeVM, InstanceItemNodeVM, SearchResultVM} from "./bp-artifact-picker-node-vm";
import {IDialogSettings, BaseDialogController} from "../../../shared/";
import {Models, AdminStoreModels, SearchServiceModels} from "../../models";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IProjectService} from "../../../managers/project-manager/project-service";

export class ArtifactPickerDialogController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private selectedVMs: IViewModel<any>[];

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

    public onSelectionChanged(selectedVMs: IViewModel<any>[]) {
        this.selectedVMs = selectedVMs;
    }

    public onDoubleClick(vm: IViewModel<any>) {
        this.selectedVMs = [vm];
        this.ok();
    }
}

/**
 * Usage:
 *
 * <bp-artifact-picker selectable-item-types="$ctrl.selectableItemTypes" ;
 *                     selection-mode="$ctrl.selectionMode" ;
 *                     show-sub-artifacts="$ctrl.showSubArtifacts"
 *                     is-one-project-level="$ctrl.isOneProjectLevel"
 *                     on-selection-changed="$ctrl.onSelectionChanged(selectedVMs)"
 *                     on-double-click="$ctrl.onDoubleClick(vm)">
 * </bp-artifact-picker>
 */
export class BpArtifactPicker implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactPickerController;
    public template: string = require("./bp-artifact-picker.html");
    public bindings: {[binding: string]: string} = {
        selectableItemTypes: "<",
        selectionMode: "<",
        showSubArtifacts: "<",
        isOneProjectLevel: "<",
        onSelectionChanged: "&?",
        onDoubleClick: "&?"
    };
}

export interface IArtifactPickerOptions {
    selectableItemTypes?: Models.ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showSubArtifacts?: boolean;
    isOneProjectLevel?: boolean;
}

export interface IArtifactPickerController extends IArtifactPickerOptions {
    project: AdminStoreModels.IInstanceItem;

    // BpArtifactPicker bindings
    onSelectionChanged: (params: {selectedVMs: IViewModel<any>[]}) => any;
    onDoubleClick: (params: {vm: IViewModel<any>}) => any;

    // BpTreeView bindings
    currentSelectionMode: "single" | "multiple" | "checkbox";
    rootNode: InstanceItemNodeVM;
    columns: IColumn[];
    onSelect: (vm: IViewModel<any>, isSelected: boolean) => any;

    // Search
    searchText: string;
    isSearching: boolean;
    searchResults: SearchResultVM[];
    isMoreSearchResults: boolean;
    search(): void;
    clearSearch(): void;
    onDouble(vm: SearchResultVM): void;
}

export class BpArtifactPickerController implements ng.IComponentController, IArtifactPickerController {
    private static readonly maxSearchResults = 100;

    // BpArtifactPicker bindings
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showSubArtifacts: boolean;
    public isOneProjectLevel: boolean;
    public onSelectionChanged: (params: {selectedVMs: IViewModel<any>[]}) => any;
    public onDoubleClick: (params: {vm: IViewModel<any>}) => any;

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
                this.project = {
                    id: project.id,
                    type: AdminStoreModels.InstanceItemType.Project,
                    name: project.name,
                    hasChildren: project.hasChildren
                } as AdminStoreModels.IInstanceItem;
            } else {
                this.projectService.getProject(projectId)
                    .then(project => this.project = project);
            }
        } else {
            this.project = undefined;
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

    private _project: AdminStoreModels.IInstanceItem;

    public get project(): AdminStoreModels.IInstanceItem {
        return this._project;
    }

    public set project(project: AdminStoreModels.IInstanceItem) {
        this.selectedVMs = [];
        this._project = project;
        this.currentSelectionMode = project ? this.selectionMode : "single";
        this.rootNode = new InstanceItemNodeVM(this.artifactManager, this.projectService, this, project || {
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true);
    }

    private _selectedVMs: IViewModel<any>[] = [];

    private get selectedVMs(): IViewModel<any>[] {
        return this._selectedVMs;
    }

    private set selectedVMs(value: IViewModel<any>[]) {
        this._selectedVMs = value;
        this.raiseSelectionChanged();
    }

    private raiseSelectionChanged() {
        if (this.onSelectionChanged) {
            this.$scope.$applyAsync((s) => {
                this.onSelectionChanged({selectedVMs: this.selectedVMs});
            });
        }
    }

    // BpTreeView bindings

    public currentSelectionMode: "single" | "multiple" | "checkbox";
    public rootNode: InstanceItemNodeVM;
    public columns: IColumn[] = [{
        cellClass: (vm: ArtifactPickerNodeVM<any>) => vm.getCellClass(),
        isGroup: true,
        innerRenderer: (vm: ArtifactPickerNodeVM<any>, eGridCell: HTMLElement) => {
            const icon = vm.getIcon();
            const name = Helper.escapeHTMLText(vm.name);
            return `<span class="ag-group-value-wrapper">${icon}<span>${name}</span></span>`;
        }
    }];

    public onSelect = (vm: IViewModel<any>, isSelected: boolean = undefined): boolean => {
        if (angular.isDefined(isSelected)) {
            if (this.project) {
                // Selecting an item from the project tree or project search results
                if (isSelected) {
                    if (this.selectionMode === "single") {
                        this.selectedVMs = [vm];
                    } else {
                        this.selectedVMs.push(vm);
                        this.raiseSelectionChanged();
                    }
                } else {
                    const index = this.selectedVMs.indexOf(vm);
                    if (index >= 0) {
                        this.selectedVMs.splice(index, 1);
                        this.raiseSelectionChanged();
                    }
                }
            } else if (vm instanceof InstanceItemNodeVM && vm.model.type === AdminStoreModels.InstanceItemType.Project) {
                // Selecting a project from the instance tree
                this.project = vm.model;
            } else if (vm instanceof SearchResultVM) {
                // Selecting a project from the project search results
                this.clearSearch();
                this.project = {
                    id: vm.model.itemId,
                    type: AdminStoreModels.InstanceItemType.Project,
                    name: vm.model.name,
                    hasChildren: true
                } as AdminStoreModels.IInstanceItem;
            }
        } else {
            return this.selectedVMs.indexOf(vm) >= 0;
        }
    };

    public onDouble(vm: SearchResultVM): void {
        if (this.onDoubleClick && this.selectionMode === "single") {
            this.onDoubleClick({vm: vm});
        }
    }

    // Search

    public searchText: string = "";
    public isSearching: boolean = false;
    public searchResults: SearchResultVM[];
    public isMoreSearchResults: boolean;
    private _preiousSelectedVMs = [];

    public search(): void {
        if (!this.isSearching && this.searchText && this.searchText.trim().length > 0) {
            this.isSearching = true;
            let search: ng.IPromise<SearchServiceModels.ISearchResultSet<SearchServiceModels.ISearchResult>>;
            if (this.project) {
                const searchCriteria: SearchServiceModels.IItemNameSearchCriteria = {
                    query: this.searchText,
                    projectIds: [this.project.id],
                    predefinedTypeIds: this.selectableItemTypes,
                    includeArtifactPath: true
                };
                search = this.projectService.searchItemNames(searchCriteria, 0, BpArtifactPickerController.maxSearchResults + 1);
            } else {
                const searchCriteria: SearchServiceModels.ISearchCriteria = {
                    query: this.searchText
                };
                search = this.projectService.searchProjects(searchCriteria, BpArtifactPickerController.maxSearchResults + 1);
            }
            search.then(result => {
                this.searchResults = result.items.slice(0, BpArtifactPickerController.maxSearchResults).map(r => new SearchResultVM(r, this.onSelect));
                this.isMoreSearchResults = (result.items.length > BpArtifactPickerController.maxSearchResults);
                this._preiousSelectedVMs = this.selectedVMs;
                this.selectedVMs = [];
            }).finally(() => {
                this.isSearching = false;
            });
        }
    }

    public clearSearch(): void {
        if (this.isSearching) {
            this.projectService.abort();
        }
        this.searchText = undefined;
        this.searchResults = undefined;
        this.isMoreSearchResults = undefined;
        this.selectedVMs = this._preiousSelectedVMs;
    }
}
