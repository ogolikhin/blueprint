﻿import {IColumn, IColumnRendererParams, IBPTreeViewControllerApi} from "../../../shared/widgets/bp-tree-view/";
import {Helper} from "../../../shared/";
import {SearchResultVM, ArtifactSearchResultVM, ProjectSearchResultVM} from "./search-result-vm";
import {Models, AdminStoreModels, SearchServiceModels, TreeModels} from "../../models";
import {IProjectManager} from "../../../managers/project-manager";
import {IArtifactManager, IStatefulArtifactFactory} from "../../../managers/artifact-manager";
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
    updateSelectableNodes(): void;
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
    onSelectionChanged: (params: {selectedVMs: Models.IViewModel<any>[]}) => any;
    onDoubleClick: (params: {vm: Models.IViewModel<any>}) => any;

    // BpTreeView bindings
    currentSelectionMode: "single" | "multiple" | "checkbox";
    rowData: TreeModels.InstanceItemNodeVM[];
    columns: IColumn[];
    onSelect: (vm: Models.IViewModel<any>, isSelected: boolean) => any;

    // Search
    searchText: string;
    isSearching: boolean;
    searchResults: SearchResultVM<any>[];
    isMoreSearchResults: boolean;
    search(): void;
    clearSearch(): void;
    clearSearchEnabled(): boolean;
    onDouble(vm: SearchResultVM<any>): void;
    searchPlaceholder: string;
}

export class BpArtifactPickerController implements ng.IComponentController, IArtifactPickerController {
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showSubArtifacts: boolean;
    public isOneProjectLevel: boolean;
    public factory: TreeModels.TreeNodeVMFactory;
    public treeApi: IBPTreeViewControllerApi;

    public isItemSelectable: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean;
    public onSelectionChanged: (params: {selectedVMs: Models.IViewModel<any>[]}) => any;
    public onDoubleClick: (params: {vm: Models.IViewModel<any>}) => any;

    private _project: AdminStoreModels.IInstanceItem;
    public rowData: TreeModels.InstanceItemNodeVM[];
    public searchResults: SearchResultVM<any>[];
    public isMoreSearchResults: boolean;
    public canceller: ng.IDeferred<void>;
    private _selectedVMs: Models.IViewModel<any>[];
    public isSearching: boolean;
    public itemTypes: Models.IItemType[];
    public filterItemType: Models.IItemType;
    public searchText: string;
    public columns: IColumn[];
    public api: IArtifactPickerAPI;
    private _previousSelectedVMs: Array<any>;

    static $inject = [
        "$q",
        "$scope",
        "localization",
        "artifactManager",
        "projectManager",
        "projectService",
        "statefulArtifactFactory",
        "metadataService"
    ];

    constructor(private $q: ng.IQService,
                private $scope: ng.IScope,
                private localization: ILocalizationService,
                private artifactManager: IArtifactManager,
                private projectManager: IProjectManager,
                private projectService: IProjectService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private metadataService: IMetaDataService) {

        /*todo: refactor much of this 'setup' into the onInit lifecycle hook*/
        this._previousSelectedVMs = [];

        this.api = {
            deselectAll: () => {
                this.treeApi.deselectAll();
            },
            updateSelectableNodes: (): void => {
                this.treeApi.updateSelectableNodes((item) => (!this.isItemSelectable || this.isItemSelectable({item: item})) &&
                (!this.selectableItemTypes || this.selectableItemTypes.indexOf(item.predefinedType) !== -1));
            }
        };
        this.columns = [{
            cellClass: (vm: TreeModels.ITreeNodeVM<any>) => vm.getCellClass(),
            isGroup: true,
            cellRenderer: (params: IColumnRendererParams) => {
                const vm = params.data as TreeModels.ITreeNodeVM<any>;
                const icon = vm.getIcon();
                const label = Helper.escapeHTMLText(vm.getLabel());
                return `${icon}<span>${label}</span>`;
            }
        }];
        this.searchText = "";
        this.isSearching = false;
        this.itemTypes = null;
        this.filterItemType = null;
        this._selectedVMs = [];
        this.canceller = this.$q.defer<any>();
        this.isItemSelectable = angular.isFunction(this.isItemSelectable) ? this.isItemSelectable : undefined;
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.showSubArtifacts = angular.isDefined(this.showSubArtifacts) ? this.showSubArtifacts : false;
        this.isOneProjectLevel = angular.isDefined(this.isOneProjectLevel) ? this.isOneProjectLevel : false;
        this.factory = new TreeModels.TreeNodeVMFactory(this.projectService, this.artifactManager, this.statefulArtifactFactory,
            this.canceller.promise, this.isItemSelectable, this.selectableItemTypes, this.showSubArtifacts);
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
                    hasChildren: project.model.hasChildren
                } as AdminStoreModels.IInstanceItem;
            } else {
                this.projectService.getProject(projectId, this.canceller.promise)
                    .then(project => this.project = project);
            }
        } else {
            this.project = undefined;
        }
    }

    public currentSelectionMode: "single" | "multiple" | "checkbox";


    public get project(): AdminStoreModels.IInstanceItem {
        return this._project;
    }

    public set project(project: AdminStoreModels.IInstanceItem) {
        this.selectedVMs = [];
        this._project = project;
        this.currentSelectionMode = project ? this.selectionMode : "single";
        this.rowData = [this.factory.createInstanceItemNodeVM(project || {
                id: 0,
                type: AdminStoreModels.InstanceItemType.Folder,
                name: "",
                hasChildren: true
            } as AdminStoreModels.IInstanceItem, true)];
        this.resetItemTypes();
        if (project) {
            this.populateItemTypes(project.id);
            this.filterItemType = this.itemTypes[0];
        }
    }

    private get selectedVMs(): Models.IViewModel<any>[] {
        return this._selectedVMs;
    }

    private set selectedVMs(value: Models.IViewModel<any>[]) {
        this._selectedVMs = value;
        this.raiseSelectionChanged();
    }

    public get searchPlaceholder(): string {
        return this.localization.get(this.project ? "Label_Search_Artifacts" : "Label_Search_Projects");
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
            artifactTypes.sort(function (a, b) {
                if (a.name.toUpperCase() > b.name.toUpperCase()) {
                    return 1;
                }
                if (a.name.toUpperCase() < b.name.toUpperCase()) {
                    return -1;
                }
                return 0;
            });
            this.itemTypes = this.itemTypes.concat(artifactTypes);
        });
    }

    public $onDestroy(): void {
        if (this.columns) {
            this.columns[0].cellClass = null;
            this.columns[0].cellRenderer = null;
            this.columns = null;
        }
        this.onSelect = null;
        this.canceller.reject();
        this.canceller = null;
        this.rowData = null;
        this.treeApi = null;
        this.itemTypes = null;
    }

    public clearSearchEnabled(): boolean {
        return !!this.searchText || !!this.searchResults;
    }

    private resetItemTypes(): void {
        this.itemTypes =
            [{
                name: this.localization.get("Filter_Artifact_All_Types", "All types"),
                id: null,
                prefix: "",
                predefinedType: null,
                iconImageId: null,
                usedInThisProject: null,
                customPropertyTypeIds: null
            }];
        this.filterItemType = this.itemTypes[0];
    }

    private raiseSelectionChanged() {
        if (this.onSelectionChanged) {
            this.$scope.$applyAsync(() => {
                this.onSelectionChanged({selectedVMs: this.selectedVMs});
            });
        }
    }

    public onSelect = (vm: Models.IViewModel<any>, isSelected: boolean = undefined): boolean => {
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
            } else if (vm instanceof TreeModels.InstanceItemNodeVM && vm.model.type === AdminStoreModels.InstanceItemType.Project) {
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

    public clearFilter(): void {
        this.filterItemType = this.itemTypes[0];
        this.search();
    }

    public searchByFilter(type): void {
        this.filterItemType = type;
        this.search();

    }

    public search(): void {
        const maxSearchResults = 100;

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
                searchResults = this.projectService.searchItemNames(searchCriteria, 0, maxSearchResults + 1, this.canceller.promise)
                    .then(result => result.items.map(r => {
                        r.artifactPath = r.path;
                        return new ArtifactSearchResultVM(r, this.onSelect, this.isItemSelectable, this.selectableItemTypes, this.project);
                    }));
            } else {
                const searchCriteria: SearchServiceModels.ISearchCriteria = {
                    query: this.searchText
                };
                searchResults = this.projectService.searchProjects(searchCriteria, maxSearchResults + 1, undefined, this.canceller.promise)
                    .then(result => result.items.map(r => new ProjectSearchResultVM(r, this.onSelect)));
            }
            searchResults.then(items => {
                this.searchResults = items.slice(0, maxSearchResults);
                this.isMoreSearchResults = (items.length > maxSearchResults);
                this._previousSelectedVMs = this.selectedVMs;
                this.selectedVMs = [];
            }).finally(() => {
                this.isSearching = false;
            });
        }
    }

    public hasCustomIcon(searchResult: ArtifactSearchResultVM) {
        return !!(searchResult.model && _.isFinite(searchResult.model.itemTypeIconId));
    };

    public getArtifactTextPath(path: string | string[]): string {
        if (!path) {
            return "";
        }
        if (typeof path === "string") {
            return path;
        }
        return path.join(" > ");
    }

    public clearSearch(): void {
        if (this.isSearching) {
            this.canceller.reject();
            this.canceller = this.$q.defer<void>();
        }
        this.filterItemType = this.itemTypes[0];
        this.searchText = undefined;
        this.searchResults = undefined;
        this.isMoreSearchResults = undefined;
        this.selectedVMs = this._previousSelectedVMs;
    }
}
