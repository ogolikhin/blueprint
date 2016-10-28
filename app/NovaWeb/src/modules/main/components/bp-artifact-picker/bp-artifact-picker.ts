﻿import * as angular from "angular";
import {IColumn} from "../../../shared/widgets/bp-tree-view/";
import {Helper} from "../../../shared/";
import {ILocalizationService} from "../../../core";
import {ArtifactPickerNodeVM, InstanceItemNodeVM} from "./bp-artifact-picker-node-vm";
import {IDialogSettings, BaseDialogController} from "../../../shared/";
import {Models, AdminStoreModels, SearchServiceModels} from "../../models";
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

/**
 * Usage:
 *
 * <bp-artifact-picker selectable-item-types="$ctrl.selectableItemTypes" ;
 *                     selection-mode="$ctrl.selectionMode" ;
 *                     show-sub-artifacts="$ctrl.showSubArtifacts"
 *                     is-one-project-level="$ctrl.isOneProjectLevel"
 *                     on-selection-changed="$ctrl.onSelectionChanged(selectedVMs)">
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
    project: AdminStoreModels.IInstanceItem;

    // BpArtifactPicker bindings
    onSelectionChanged: (params: {selectedVMs: ArtifactPickerNodeVM<any>[]}) => void;

    // BpTreeView bindings
    currentSelectionMode: "single" | "multiple" | "checkbox";
    rootNode: InstanceItemNodeVM;
    columns: IColumn[];
    onSelect: (vm: ArtifactPickerNodeVM<any>, isSelected: boolean, selectedVMs: ArtifactPickerNodeVM<any>[]) => void;

    // Search
    searchText: string;
    isSearching: boolean;
    searchResults: SearchServiceModels.ISearchResult[];
    isMoreSearchResults: boolean;
    search(): void;
    clearSearch(): void;
}

export class BpArtifactPickerController implements ng.IComponentController, IArtifactPickerController {
    private static readonly maxSearchResults = 100;

    // BpArtifactPicker bindings
    public selectableItemTypes: Models.ItemTypePredefined[];
    public selectionMode: "single" | "multiple" | "checkbox";
    public showSubArtifacts: boolean;
    public isOneProjectLevel: boolean;
    public onSelectionChanged: (params: {selectedVMs: ArtifactPickerNodeVM<any>[]}) => void;

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
        this.clearSearch();
        this.setSelectedVMs([]);
        this._project = project;
        this.currentSelectionMode = this.selectionMode || "single";
        this.rootNode = new InstanceItemNodeVM(this.artifactManager, this.projectService, this, project || {
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true);
    }

    private setSelectedVMs(items: ArtifactPickerNodeVM<any>[]): void {
        this.$scope.$applyAsync((s) => {
            if (this.onSelectionChanged) {
                this.onSelectionChanged({selectedVMs: items});
            }
        });
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

    public onSelect = (vm: ArtifactPickerNodeVM<any>, isSelected: boolean, selectedVMs: ArtifactPickerNodeVM<any>[]) => {
        if (vm instanceof InstanceItemNodeVM) {
            this.setSelectedVMs([]);
            if (vm.model.type === AdminStoreModels.InstanceItemType.Project) {
                this.project = vm.model;
            }
        } else {
            this.setSelectedVMs(selectedVMs);
        }
    };

    // Search
    public searchText: string = "";
    public isSearching: boolean = false;
    public searchResults: SearchServiceModels.ISearchResult[];
    public isMoreSearchResults: boolean;

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
                this.searchResults = result.items.slice(0, BpArtifactPickerController.maxSearchResults);
                this.isMoreSearchResults = (result.items.length > BpArtifactPickerController.maxSearchResults);
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
    }

    public onSearchResultClicked(searchResult: SearchServiceModels.ISearchResult) {
        if (this.project) {
            return; //TODO
        } else {
            this.project = {
                id: searchResult.itemId,
                type: AdminStoreModels.InstanceItemType.Project,
                name: searchResult.name,
                hasChildren: true
            } as AdminStoreModels.IInstanceItem;
        }
    }
}
