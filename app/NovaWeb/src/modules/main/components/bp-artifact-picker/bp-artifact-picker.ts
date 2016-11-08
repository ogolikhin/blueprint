import {IColumn, IColumnRendererParams} from "../../../shared/widgets/bp-tree-view/";
import {Helper} from "../../../shared/";
import {SearchResultVM, SearchResultVMFactory} from "./bp-artifact-picker-search-vm";
import {Models, AdminStoreModels, SearchServiceModels, TreeViewModels} from "../../models";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {ILocalizationService} from "../../../core/localization/localizationService";

/**
 * Usage:
 *
 * <bp-artifact-picker is-item-selectable="$ctrl.dialogData.isItemSelectable(item)"
 *                     selectable-item-types="$ctrl.selectableItemTypes"
 *                     selection-mode="$ctrl.selectionMode"
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
        isItemSelectable: "&?",
        selectableItemTypes: "<",
        selectionMode: "<",
        showSubArtifacts: "<",
        isOneProjectLevel: "<",
        onSelectionChanged: "&?",
        onDoubleClick: "&?"
    };
}

export interface IArtifactPickerController {
    project: AdminStoreModels.IInstanceItem;

    // BpArtifactPicker bindings
    isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean;
    selectableItemTypes?: Models.ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showSubArtifacts?: boolean;
    isOneProjectLevel?: boolean;
    onSelectionChanged: (params: {selectedVMs: TreeViewModels.IViewModel<any>[]}) => any;
    onDoubleClick: (params: {vm: TreeViewModels.IViewModel<any>}) => any;

    // BpTreeView bindings
    currentSelectionMode: "single" | "multiple" | "checkbox";
    rootNode: TreeViewModels.InstanceItemNodeVM;
    columns: IColumn[];
    onSelect: (vm: TreeViewModels.IViewModel<any>, isSelected: boolean) => any;

    // Search
    searchText: string;
    isSearching: boolean;
    searchResults: SearchResultVM<any>[];
    isMoreSearchResults: boolean;
    search(): void;
    clearSearch(): void;
    onDouble(vm: SearchResultVM<any>): void;
}

export class BpArtifactPickerController implements ng.IComponentController, IArtifactPickerController {
    private static readonly maxSearchResults = 100;

    // BpArtifactPicker bindings
    public isItemSelectable: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean;
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showSubArtifacts: boolean;
    public isOneProjectLevel: boolean;
    public onSelectionChanged: (params: {selectedVMs: TreeViewModels.IViewModel<any>[]}) => any;
    public onDoubleClick: (params: {vm: TreeViewModels.IViewModel<any>}) => any;

    public factory: SearchResultVMFactory;

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
        this.isItemSelectable = angular.isFunction(this.isItemSelectable) ? this.isItemSelectable : undefined;
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.showSubArtifacts = angular.isDefined(this.showSubArtifacts) ? this.showSubArtifacts : false;
        this.isOneProjectLevel = angular.isDefined(this.isOneProjectLevel) ? this.isOneProjectLevel : false;
        this.factory = new SearchResultVMFactory(this.projectService, this.onSelect, this.isItemSelectable, this.selectableItemTypes, this.showSubArtifacts);
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
        this.rootNode = this.factory.createInstanceItemNodeVM(project || {
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true);
    }

    private _selectedVMs: TreeViewModels.IViewModel<any>[] = [];

    private get selectedVMs(): TreeViewModels.IViewModel<any>[] {
        return this._selectedVMs;
    }

    private set selectedVMs(value: TreeViewModels.IViewModel<any>[]) {
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
    public rootNode: TreeViewModels.InstanceItemNodeVM;
    public columns: IColumn[] = [{
        cellClass: (vm: TreeViewModels.TreeViewNodeVM<any>) => vm.getCellClass(),
        isGroup: true,
        innerRenderer: (params: IColumnRendererParams) => {
            const vm = params.data as TreeViewModels.TreeViewNodeVM<any>;
            const icon = vm.getIcon();
            const name = Helper.escapeHTMLText(vm.name);
            return `<span class="ag-group-value-wrapper">${icon}<span>${name}</span></span>`;
        }
    }];

    public onSelect = (vm: TreeViewModels.IViewModel<any>, isSelected: boolean = undefined): boolean => {
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
            } else if (vm instanceof TreeViewModels.InstanceItemNodeVM && vm.model.type === AdminStoreModels.InstanceItemType.Project) {
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

    public onDouble(vm: SearchResultVM<any>): void {
        if (this.onDoubleClick && this.selectionMode === "single" && vm.isSelectable) {
            this.onDoubleClick({vm: vm});
        }
    }

    // Search

    public searchText: string = "";
    public isSearching: boolean = false;
    public searchResults: SearchResultVM<any>[];
    public isMoreSearchResults: boolean;
    private _preiousSelectedVMs = [];

    public search(): void {
        if (!this.isSearching && this.searchText && this.searchText.trim().length > 0) {
            this.isSearching = true;
            let searchResults: ng.IPromise<SearchResultVM<any>[]>;
            if (this.project) {
                const searchCriteria: SearchServiceModels.IItemNameSearchCriteria = {
                    query: this.searchText,
                    projectIds: [this.project.id],
                    predefinedTypeIds: this.selectableItemTypes,
                    includeArtifactPath: true
                };
                searchResults = this.projectService.searchItemNames(searchCriteria, 0, BpArtifactPickerController.maxSearchResults + 1)
                    .then(result => result.items.map(r => this.factory.createArtifactSearchResultVM(r)));
            } else {
                const searchCriteria: SearchServiceModels.ISearchCriteria = {
                    query: this.searchText
                };
                searchResults = this.projectService.searchProjects(searchCriteria, BpArtifactPickerController.maxSearchResults + 1)
                    .then(result => result.items.map(r => this.factory.createProjectSearchResultVM(r)));
            }
            searchResults.then(items => {
                this.searchResults = items.slice(0, BpArtifactPickerController.maxSearchResults);
                this.isMoreSearchResults = (items.length > BpArtifactPickerController.maxSearchResults);
                this._preiousSelectedVMs = this.selectedVMs;
                this.selectedVMs = [];
            }).finally(() => {
                this.isSearching = false;
            });
        }
    }

    public hasCustomIcon = (searchResult: ArtifactSearchResultVM) => {
        if (searchResult.model && _.isFinite(searchResult.model.itemTypeIconId)) {
            return true;
        }
        return false;
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
