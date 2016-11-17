﻿import {IColumn, IColumnRendererParams, IBPTreeViewControllerApi} from "../../../shared/widgets/bp-tree-view/";
import {Helper} from "../../../shared/";
import {SearchResultVM, ArtifactSearchResultVM, SearchResultVMFactory} from "./bp-artifact-picker-search-vm";
import {Models, AdminStoreModels, SearchServiceModels, TreeViewModels} from "../../models";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IMetaDataService} from "../../../managers/artifact-manager/metadata";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {ILocalizationService} from "../../../core/localization/localizationService";

/**
 * Usage:
 *
 * <bp-artifact-picker api="$ctrl.api"
 *                     selectable-item-types="$ctrl.selectableItemTypes"
 *                     selection-mode="$ctrl.selectionMode"
 *                     show-sub-artifacts="$ctrl.showSubArtifacts"
 *                     is-one-project-level="$ctrl.isOneProjectLevel"
 *                     is-item-selectable="$ctrl.dialogData.isItemSelectable(item)"
 *                     on-selection-changed="$ctrl.onSelectionChanged(selectedVMs)"
 *                     on-double-click="$ctrl.onDoubleClick(vm)">
 * </bp-artifact-picker>
 */
export class BpArtifactPicker implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactPickerController;
    public template: string = require("./bp-artifact-picker.html");
    public bindings: {[binding: string]: string} = {
        // Two-way
        api: "=?",
        // Input
        selectableItemTypes: "<",
        selectionMode: "<",
        showSubArtifacts: "<",
        isOneProjectLevel: "<",
        // Output
        isItemSelectable: "&?",
        onSelectionChanged: "&?",
        onDoubleClick: "&?"
    };
}

export interface IArtifactPickerAPI {
    deselectAll(): void;
}

export interface IArtifactPickerController {
    project: AdminStoreModels.IInstanceItem;

    // BpArtifactPicker bindings
    api: IArtifactPickerAPI;
    selectableItemTypes?: Models.ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showSubArtifacts?: boolean;
    isOneProjectLevel?: boolean;
    isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean;
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
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showSubArtifacts: boolean;
    public isOneProjectLevel: boolean;
    public isItemSelectable: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean;
    public onSelectionChanged: (params: {selectedVMs: TreeViewModels.IViewModel<any>[]}) => any;
    public onDoubleClick: (params: {vm: TreeViewModels.IViewModel<any>}) => any;

    public factory: SearchResultVMFactory;
    public treeApi: IBPTreeViewControllerApi;

    static $inject = [
        "$scope",
        "localization",
        "artifactManager",
        "projectManager",
        "projectService",
        "metadataService"
    ];

    constructor(private $scope: ng.IScope,
                private localization: ILocalizationService,
                private artifactManager: IArtifactManager,
                private projectManager: IProjectManager,
                private projectService: IProjectService,
                private metadataService: IMetaDataService) {
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
                    id: project.model.id,
                    type: AdminStoreModels.InstanceItemType.Project,
                    name: project.model.name,
                    hasChildren: project.group
                } as AdminStoreModels.IInstanceItem;
            } else {
                this.projectService.getProject(projectId)
                    .then(project => this.project = project);
            }
            this.resetItemTypes();
            this.populateItemTypes(projectId);
        } else {
            this.project = undefined;
        }
    }

    private populateItemTypes(projectId: number): void {
        this.metadataService.get(projectId).then((metaData) => {
            let artifactTypes = metaData.data.artifactTypes;
            if (this.selectableItemTypes) {
                artifactTypes = artifactTypes.filter(a => this.selectableItemTypes.indexOf(a.predefinedType) >= 0);
            } else {
            artifactTypes = artifactTypes.filter(a => 
                a.predefinedType !== Models.ItemTypePredefined.Project
                && a.predefinedType !== Models.ItemTypePredefined.ArtifactBaseline
                && a.predefinedType !== Models.ItemTypePredefined.ArtifactCollection
                && a.predefinedType !== Models.ItemTypePredefined.ArtifactReviewPackage
                && a.predefinedType !== Models.ItemTypePredefined.Baseline
                && a.predefinedType !== Models.ItemTypePredefined.BaselineFolder
                && a.predefinedType !== Models.ItemTypePredefined.Collections
                && a.predefinedType !== Models.ItemTypePredefined.CollectionFolder
                );
            }
            this.itemTypes = this.itemTypes.concat(artifactTypes);
        });
    }

    public $onDestroy(): void {
        if (this.columns) {
            this.columns[0].cellClass = undefined;
            this.columns[0].innerRenderer = undefined;
            this.columns = undefined;
        }
        this.onSelect = undefined;
        this.projectService.abort();
        delete this.itemTypes;
    }

    public api: IArtifactPickerAPI = {
        deselectAll: () => {
            this.treeApi.deselectAll();
        }
    };

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
        if (project) {
            this.resetItemTypes();
            this.populateItemTypes(project.id);
            this.filterItemType = this.itemTypes[0];
        } else {
            this.resetItemTypes();
        }
    }

    private resetItemTypes(): void {
        this.itemTypes =                 
                [{
                    name : "", 
                    id : null, 
                    prefix : "", 
                    predefinedType : null,
                    iconImageId: null,
                    usedInThisProject: null,
                    customPropertyTypeIds: null
                }];
        this.filterItemType = this.itemTypes[0];
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
            const label = Helper.escapeHTMLText(vm.getLabel());
            return `<span class="ag-group-value-wrapper">${icon}<span>${label}</span></span>`;
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
    public itemTypes: Models.IItemType[] = null;
    public filterItemType: Models.IItemType = null;

    public clearFilter(): void {
        this.filterItemType = this.itemTypes[0];
        this.search();
    }

    public search(): void {
        if (!this.isSearching && this.searchText && this.searchText.trim().length > 0) {
            this.isSearching = true;
            let searchResults: ng.IPromise<SearchResultVM<any>[]>;
            if (this.project) {
                const searchCriteria: SearchServiceModels.IItemNameSearchCriteria = {
                    query: this.searchText,
                    projectIds: [this.project.id],
                    predefinedTypeIds: this.filterItemType.id ? [] : this.selectableItemTypes,
                    itemTypeIds: this.filterItemType.id ? [this.filterItemType.id] : [],
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
