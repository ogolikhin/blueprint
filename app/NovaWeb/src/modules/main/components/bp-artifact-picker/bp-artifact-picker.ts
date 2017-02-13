import {IMetaDataService} from "../../../managers/artifact-manager/metadata";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {Helper} from "../../../shared/";
import {IBPTreeViewControllerApi, IColumn, IColumnRendererParams} from "../../../shared/widgets/bp-tree-view/";
import {AdminStoreModels, Models, SearchServiceModels, TreeModels} from "../../models";
import {ArtifactSearchResultVM, ProjectSearchResultVM, SearchResultVM} from "./search-result-vm";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IProjectExplorerService} from "../bp-explorer/project-explorer.service";

/**
 * Usage:
 *
 * <bp-artifact-picker api="$ctrl.api"
 *                     selectable-item-types="$ctrl.selectableItemTypes"
 *                     selection-mode="$ctrl.selectionMode"                         (default: "single")
 *                     show-projects="$ctrl.showProjects"                           (default: true)
 *                     show-artifacts="$ctrl.showArtifacts"                         (default: true)
 *                     show-baselines-and-reviews="$ctrl.showBaselinesAndReviews"   (default: false)
 *                     show-collections="$ctrl.showCollections"                     (default: false)
 *                     show-sub-artifacts="$ctrl.showSubArtifacts"                  (default: false)
 *                     is-item-selectable="$ctrl.isItemSelectable(item)"
 *                     on-selection-changed="$ctrl.onSelectionChanged(selectedVMs)"
 *                     on-double-click="$ctrl.onDoubleClick(vm)">
 * </bp-artifact-picker>
 *
 * At least one of show-proects, show-artifacts, show-collections must be true.
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
        showProjects: "<",
        showArtifacts: "<",
        showBaselinesAndReviews: "<",
        showCollections: "<",
        showSubArtifacts: "<",
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
    showProjects?: boolean;
    showArtifacts?: boolean;
    showBaselinesAndReviews?: boolean;
    showCollections?: boolean;
    showSubArtifacts?: boolean;
    isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean;
    onSelectionChanged: (params: {selectedVMs: Models.IViewModel<any>[]}) => any;
    onDoubleClick: (params: {vm: Models.IViewModel<any>}) => any;

    // BpTreeView bindings
    currentSelectionMode: "single" | "multiple" | "checkbox";
    rowData: TreeModels.InstanceItemNodeVM[];
    treeApi: IBPTreeViewControllerApi;
    columns: IColumn[];
    onSelect: (vm: Models.IViewModel<any>, isSelected: boolean) => any;
    onDouble(vm: TreeModels.ITreeNodeVM<any> | SearchResultVM<any>): void;

    // Search bindings
    searchPlaceholder: string;
    searchText: string;
    isSearching: boolean;
    searchResults: SearchResultVM<any>[];
    isMoreSearchResults: boolean;
    hasCustomIcon(searchResult: ArtifactSearchResultVM);
    getArtifactTextPath(path: string[]): string;
    itemTypes: Models.IItemType[];
    filterItemType: Models.IItemType;
    search(): void;
    searchByFilter(type): void;
    clearFilter(): void;
    clearSearch(): void;
    clearSearchEnabled(): boolean;
}

export class BpArtifactPickerController implements ng.IComponentController, IArtifactPickerController {
    // Constants
    private readonly instanceRoot = {
        id: 0,
        type: AdminStoreModels.InstanceItemType.Folder,
        name: "",
        hasChildren: true
    } as AdminStoreModels.IInstanceItem;
    private readonly allTypes = {
        name: this.localization.get("Filter_Artifact_All_Types", "All types"),
        id: null,
        prefix: "",
        predefinedType: null,
        iconImageId: null,
        usedInThisProject: null,
        customPropertyTypeIds: null
    } as Models.IItemType;

    // BpArtifactPicker bindings
    public api: IArtifactPickerAPI;
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showProjects: boolean;
    public showArtifacts: boolean;
    public showBaselinesAndReviews: boolean;
    public showCollections: boolean;
    public showSubArtifacts: boolean;
    public isItemSelectable: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean;
    public onSelectionChanged: (params: {selectedVMs: Models.IViewModel<any>[]}) => any;
    public onDoubleClick: (params: {vm: Models.IViewModel<any>}) => any;

    // BpTreeView bindings
    public currentSelectionMode: "single" | "multiple" | "checkbox";
    public rowData: TreeModels.InstanceItemNodeVM[];
    public treeApi: IBPTreeViewControllerApi;
    public columns: IColumn[];

    // Search bindings
    public searchText: string;
    public isSearching: boolean;
    public searchResults: SearchResultVM<any>[];
    public isMoreSearchResults: boolean;
    public itemTypes: Models.IItemType[];
    public filterItemType: Models.IItemType;

    public canceller: ng.IDeferred<void>;
    public factory: TreeModels.TreeNodeVMFactory;
    private _project: AdminStoreModels.IInstanceItem;
    private _selectedVMs: Models.IViewModel<any>[];
    private _previousSelectedVMs: Models.IViewModel<any>[];

    static $inject = [
        "$q",
        "$scope",
        "localization",
        "selectionManager",
        "projectExplorerService",
        "projectService",
        "metadataService"
    ];

    constructor(private $q: ng.IQService,
                private $scope: ng.IScope,
                public localization: ILocalizationService,
                private selectionManager: ISelectionManager,
                private projectExplorerService: IProjectExplorerService,
                private projectService: IProjectService,
                private metadataService: IMetaDataService) {

        /*todo: refactor much of this 'setup' into the onInit lifecycle hook*/

        // BpArtifactPicker bindings
        this.api = {
            deselectAll: () => {
                this.treeApi.deselectAll();
            },
            updateSelectableNodes: (): void => {
                if (this.searchResults) {
                    _.each(this.searchResults, (item) => {
                        item.selectable = this.isNodeSelectable(item.model);
                    });
                }

                this.treeApi.updateSelectableNodes((item) => this.isNodeSelectable(item));
            }
        };
        this.selectionMode = _.isString(this.selectionMode) ? this.selectionMode : "single";
        this.showProjects = _.isBoolean(this.showProjects) ? this.showProjects : true;
        this.showArtifacts = _.isBoolean(this.showArtifacts) ? this.showArtifacts : true;
        this.showBaselinesAndReviews = _.isBoolean(this.showBaselinesAndReviews) ? this.showBaselinesAndReviews : false;
        this.showCollections = _.isBoolean(this.showCollections) ? this.showCollections : false;
        this.showSubArtifacts = _.isBoolean(this.showSubArtifacts) ? this.showSubArtifacts : false;
        this.isItemSelectable = _.isFunction(this.isItemSelectable) ? this.isItemSelectable : undefined;

        // BpTreeView bindings
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

        // Search bindings
        this.searchText = "";
        this.isSearching = false;
        this.itemTypes = null;
        this.filterItemType = null;

        this.canceller = this.$q.defer<any>();
        this.factory = new TreeModels.TreeNodeVMFactory(this.projectService, this.canceller.promise, this.isItemSelectable,
            this.selectableItemTypes, this.showArtifacts, this.showBaselinesAndReviews, this.showCollections, this.showSubArtifacts);
        this._selectedVMs = [];
        this._previousSelectedVMs = [];
    };

    public $onInit(): void {
        let project: AdminStoreModels.IInstanceItem | ng.IPromise<AdminStoreModels.IInstanceItem>;
        if (this.showArtifacts || this.showBaselinesAndReviews || this.showCollections) {
            const selectedArtifact = this.selectionManager.getArtifact();
            const projectId = selectedArtifact ? selectedArtifact.projectId : undefined;
            if (projectId) {
                const projectVM = this.projectExplorerService.getProject(projectId);
                if (projectVM) {
                    project = {
                        id: projectVM.model.id,
                        type: AdminStoreModels.InstanceItemType.Project,
                        name: projectVM.model.name,
                        hasChildren: projectVM.model.hasChildren
                    } as AdminStoreModels.IInstanceItem;
                } else {
                    project = this.projectService.getProject(projectId, this.canceller.promise);
                }
            }
        }
        this.$q.when(project).then(project => this.project = project);
    }

    private isNodeSelectable = (item: Models.IArtifact | Models.ISubArtifactNode) => {
        return (!this.isItemSelectable || this.isItemSelectable({item: item})) &&
            (!this.selectableItemTypes || this.selectableItemTypes.indexOf(item.predefinedType) !== -1);
    }

    public $onDestroy(): void {
        if (this.columns) {
            this.columns[0].cellClass = null;
            this.columns[0].cellRenderer = null;
            this.columns = null;
        }
        this.onSelect = null;
        this.canceller.resolve();
        this.canceller = null;
        this.rowData = null;
        this.treeApi = null;
        this.itemTypes = null;
    }

    public get project(): AdminStoreModels.IInstanceItem {
        return this._project;
    }

    public set project(project: AdminStoreModels.IInstanceItem) {
        this.selectedVMs = [];
        this._project = project;
        this.currentSelectionMode = project ? this.selectionMode : "single";
        this.rowData = [this.factory.createInstanceItemNodeVM(project || this.instanceRoot, true)];
        this.itemTypes = [this.allTypes];
        this.filterItemType = this.allTypes;
        if (project) {
            this.populateItemTypes(project.id);
            this.filterItemType = this.itemTypes[0];
        }
    }

    private populateItemTypes(projectId: number): void {
        this.metadataService.get(projectId).then((metaData) => {
            let itemTypes = [];

            const artifactTypes = metaData.data.artifactTypes.filter(a => {
                if ((a.predefinedType & Models.ItemTypePredefined.BaselineArtifactGroup) !== 0) {
                    return this.showBaselinesAndReviews &&
                        a.id !== Models.ItemTypePredefined.BaselinesAndReviews; // Exclude main Baselines and Reviews folder
                }
                if ((a.predefinedType & Models.ItemTypePredefined.CollectionArtifactGroup) !== 0) {
                    return this.showCollections &&
                        a.id !== Models.ItemTypePredefined.Collections; // Exclude main Collection folder
                }
                return this.showArtifacts &&
                    a.predefinedType !== Models.ItemTypePredefined.Project &&
                    a.predefinedType !== Models.ItemTypePredefined.Baseline;
            });
            if (artifactTypes.length) {
                itemTypes = artifactTypes;
            }

            // TODO: re-enable the following once the search can return sub-artifacts as well
            // if (this.showSubArtifacts && subArtifactTypes.length) {
            //     itemTypes = _.concat(itemTypes, subArtifactTypes);
            // }

            if (this.selectableItemTypes) {
                itemTypes = itemTypes.filter(a => this.selectableItemTypes.indexOf(a.predefinedType) >= 0);
            }
            itemTypes = _.sortBy(itemTypes, type => type.name.toLowerCase());
            this.itemTypes = this.itemTypes.concat(itemTypes);
        });
    }

    private get selectedVMs(): Models.IViewModel<any>[] {
        return this._selectedVMs;
    }

    private set selectedVMs(value: Models.IViewModel<any>[]) {
        this._selectedVMs = value;
        if (this.onSelectionChanged) {
            this.$scope.$applyAsync(() => {
                this.onSelectionChanged({selectedVMs: this.selectedVMs});
            });
        }
    }

    // BpTreeView bindings

    public onSelect = (vm: Models.IViewModel<any>, isSelected: boolean = undefined): boolean => {
        if (_.isBoolean(isSelected)) {
            if ((this.showArtifacts || this.showBaselinesAndReviews || this.showCollections) && vm instanceof TreeModels.InstanceItemNodeVM &&
                vm.model.type === AdminStoreModels.InstanceItemType.Project) {
                // Selecting a project from the instance tree
                this.project = vm.model;
            } else if ((this.showArtifacts || this.showBaselinesAndReviews || this.showCollections) && vm instanceof ProjectSearchResultVM) {
                // Selecting a project from the project search results
                this.clearSearch();
                this.project = {
                    id: vm.model.itemId,
                    type: AdminStoreModels.InstanceItemType.Project,
                    name: vm.model.name,
                    hasChildren: true
                } as AdminStoreModels.IInstanceItem;
            } else if (isSelected) {
                this.selectedVMs = this.selectionMode === "single" ? [vm] : _.concat(this.selectedVMs, vm);
            } else {
                this.selectedVMs = _.filter(this.selectedVMs, selectedVM => selectedVM !== vm);
            }
        } else {
            return this.selectedVMs.indexOf(vm) >= 0;
        }
    };

    public onDouble(vm: TreeModels.ITreeNodeVM<any> | SearchResultVM<any>): void {
        if (this.onDoubleClick && this.selectionMode === "single" && vm.selectable) {
            this.onDoubleClick({vm: vm});
        }
    }

    // Search bindings

    public get searchPlaceholder(): string {
        return this.localization.get(this.project ? "Label_Search_Artifacts" : "Label_Search_Projects");
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
                    predefinedTypeIds: this.selectableItemTypes,
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

    public searchByFilter(type: Models.IItemType): void {
        this.filterItemType = type;
        this.search();
    }

    public clearFilter(): void {
        this.filterItemType = this.itemTypes[0];
        this.search();
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
            this.canceller.resolve();
            this.canceller = this.$q.defer<void>();
        }
        this.filterItemType = this.itemTypes[0];
        this.searchText = undefined;
        this.searchResults = undefined;
        this.isMoreSearchResults = undefined;
        this.selectedVMs = this._previousSelectedVMs;
    }

    public clearSearchEnabled(): boolean {
        return !!this.searchText || !!this.searchResults;
    }
}
