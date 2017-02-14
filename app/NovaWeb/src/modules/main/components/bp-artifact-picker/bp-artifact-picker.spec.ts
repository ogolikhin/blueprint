import "angular-mocks";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IMetaDataService} from "../../../managers/artifact-manager/metadata";
import {IProjectManager} from "../../../managers/project-manager";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {IColumnRendererParams} from "../../../shared/widgets/bp-tree-view/";
import {AdminStoreModels, Models, SearchServiceModels, TreeModels} from "../../models";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";
import {BpArtifactPicker, BpArtifactPickerController} from "./bp-artifact-picker";
import {ArtifactSearchResultVM, ProjectSearchResultVM} from "./search-result-vm";
import * as angular from "angular";

describe("BpArtifactPicker", () => {
    angular.module("bp.components.artifactpicker", [])
        .component("bpArtifactPicker", new BpArtifactPicker());

    beforeEach(angular.mock.module("bp.components.artifactpicker", ($provide: ng.auto.IProvideService) => {
        $provide.service("localization", () => ({
            get: (name: string, defaultValue?: string) => { return; }
        }));
        $provide.service("selectionManager", () => ({
            getArtifact: () => ({projectId: 1})
        }));
        $provide.service("projectManager", () => ({
            getProject: (id: number) => ({model: {id: id, name: "default"}, group: true})
        }));
        $provide.service("projectService", () => ({
            abort: () => undefined
        }));
        $provide.service("metadataService", () => ({
            get: (projectId: number) => undefined
        }));
    }));

    it("Values are bound", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $q: ng.IQService, metadataService: IMetaDataService) => {
        // Arrange
        spyOn(metadataService, "get").and.callFake(() => {
            const deferred = $q.defer();
            deferred.resolve({artifactTypes: {}});
            return deferred.promise;
        });

        const element = `<bp-artifact-picker is-item-selectable="$ctrl.isItemSelectable()"
                                             selectable-item-types="[1, 2]"
                                             selection-mode="'multiple'"
                                             show-projects="false"
                                             show-artifacts="false"
                                             show-baselines-and-reviews="true"
                                             show-collections="true"
                                             show-sub-artifacts="true"
                                             on-selection-changed="$ctrl.onSelectionChanged(selectedItems)"
                                             on-double-click="$ctrl.onDoubleClick(vm)">`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("bpArtifactPicker") as BpArtifactPickerController;

        // Assert
        expect(angular.isFunction(controller.isItemSelectable)).toEqual(true);
        expect(controller.selectableItemTypes).toEqual([1, 2]);
        expect(controller.selectionMode).toEqual("multiple");
        expect(controller.showProjects).toEqual(false);
        expect(controller.showArtifacts).toEqual(false);
        expect(controller.showBaselinesAndReviews).toEqual(true);
        expect(controller.showCollections).toEqual(true);
        expect(controller.showSubArtifacts).toEqual(true);
        expect(angular.isFunction(controller.onSelectionChanged)).toEqual(true);
        expect(angular.isFunction(controller.onDoubleClick)).toEqual(true);
    }));

    it("Defaults values are applied", inject(($compile: ng.ICompileService,
                                              $rootScope: ng.IRootScopeService,
                                              $q: ng.IQService,
                                              metadataService: IMetaDataService) => {
        // Arrange
        spyOn(metadataService, "get").and.callFake(() => {
            const deferred = $q.defer();
            deferred.resolve({artifactTypes: {}});
            return deferred.promise;
        });
        const element = `<bp-artifact-picker />`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("bpArtifactPicker") as BpArtifactPickerController;

        // Assert
        expect(controller.isItemSelectable).toBeUndefined();
        expect(controller.selectableItemTypes).toBeUndefined();
        expect(controller.selectionMode).toEqual("single");
        expect(controller.showProjects).toEqual(true);
        expect(controller.showArtifacts).toEqual(true);
        expect(controller.showBaselinesAndReviews).toEqual(false);
        expect(controller.showCollections).toEqual(false);
        expect(controller.showSubArtifacts).toEqual(false);
        expect(controller.onSelectionChanged).toBeUndefined();
        expect(controller.onDoubleClick).toBeUndefined();
    }));
});

describe("BpArtifactPickerController", () => {
    let $scope: ng.IScope;
    let projectService: IProjectService;
    let metadataService: IMetaDataService;
    let localization: ILocalizationService;
    let controller: BpArtifactPickerController;
    const project = {model: {id: 1, name: "default", hasChildren: true}};

    beforeEach(inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        $scope = $rootScope.$new();
        localization = {get: jasmine.createSpyObj("localization", ["get"])} as ILocalizationService;
        spyOn(localization, "get").and.returnValue("All types");
        const selectionManager = jasmine.createSpyObj("selectionManager", ["getArtifact"]) as ISelectionManager;
        (selectionManager.getArtifact as jasmine.Spy).and.returnValue({projectId: 1});
        const projectManager = jasmine.createSpyObj("projectManager", ["getProject"]) as IProjectManager;
        (projectManager.getProject as jasmine.Spy).and.returnValue(project);
        projectService = jasmine.createSpyObj("projectService", ["abort", "searchItemNames", "searchProjects"]) as IProjectService;
        metadataService = jasmine.createSpyObj("metadataService", ["get"]) as IMetaDataService;
        (metadataService.get as jasmine.Spy).and.returnValue($q.resolve({data: {artifactTypes: []}}));
        controller = new BpArtifactPickerController($q, $scope, localization, selectionManager,
            projectManager, projectService, metadataService);
    }));

    it("$onInit sets selected project", inject(($rootScope: ng.IRootScopeService) => {
        // Arrange

        // Act
        controller.$onInit();

        // Assert
        $rootScope.$digest(); // Resolves promises
        expect(controller.project).toEqual({
            id: project.model.id,
            type: AdminStoreModels.InstanceItemType.Project,
            name: project.model.name,
            hasChildren: project.model.hasChildren
        });
    }));

    it("$onDestroy cleans up", () => {
        // Arrange

        // Act
        controller.$onDestroy();

        // Assert
        expect(controller.columns).toBeNull();
        expect(controller.onSelect).toBeNull();
    });

    it("clearSearch clears text and results", () => {
        // Arrange
        controller.itemTypes =
            [{
                name : "All types",
                id : null,
                prefix : "",
                predefinedType : null,
                iconImageId: null,
                usedInThisProject: null,
                customPropertyTypeIds: null
            }];

        // Act
        controller.clearSearch();
        const clearSearchEnabled = controller.clearSearchEnabled();

        // Assert
        expect(controller.searchText).toBeUndefined();
        expect(controller.searchResults).toBeUndefined();
        expect(controller.isMoreSearchResults).toBeUndefined();
        expect(clearSearchEnabled).toBe(false);
    });

    it("search, when project is set, searches artifacts", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        // Arrange
        controller.project = {id: 2, name: "proj", type: AdminStoreModels.InstanceItemType.Project} as AdminStoreModels.IInstanceItem;
        controller.searchText = "test";
        const searchResults = {items: []} as SearchServiceModels.IItemNameSearchResultSet;
        (projectService.searchItemNames as jasmine.Spy).and.returnValue($q.resolve(searchResults));

        // Act
        controller.search();
        const clearSearchEnabled = controller.clearSearchEnabled();

        // Assert
        expect(controller.isSearching).toEqual(true);
        expect(projectService.searchItemNames).toHaveBeenCalledWith({
            query: "test",
            projectIds: [2],
            predefinedTypeIds: undefined,
            itemTypeIds: [ ],
            includeArtifactPath: true
        }, 0, 101, controller.canceller.promise);
        $rootScope.$digest(); // Resolves promises
        expect(controller.isSearching).toEqual(false);
        expect(controller.searchResults).toEqual([]);
        expect(controller.isMoreSearchResults).toEqual(false);
        expect(clearSearchEnabled).toBe(true);
    }));

    it("search, when project is not set, searches projects", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        // Arrange
        controller.searchText = "test";
        const searchResults = {items: []} as SearchServiceModels.IProjectSearchResultSet;
        (projectService.searchProjects as jasmine.Spy).and.returnValue($q.resolve(searchResults));

        // Act
        controller.search();

        // Assert
        expect(controller.isSearching).toEqual(true);
        expect(projectService.searchProjects).toHaveBeenCalledWith({query: "test"}, 101, undefined, controller.canceller.promise);
        $rootScope.$digest(); // Resolves promises
        expect(controller.isSearching).toEqual(false);
        expect(controller.searchResults).toEqual([]);
        expect(controller.isMoreSearchResults).toEqual(false);
    }));

    it("onSearchResultDoubleClick, when single-selection mode, calls onDoubleClick", () => {
        // Arrange
        const model = {id: 13, itemId: 13, predefinedType: ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
        const vm = new ArtifactSearchResultVM(model, undefined);
        controller.selectionMode = "single";
        controller.onDoubleClick = jasmine.createSpy("onDoubleClick");

        // Act
        controller.onDouble(vm);

        // Assert
        expect(controller.onDoubleClick).toHaveBeenCalledWith({vm: vm});
    });

    it("getArtifactTextPath, when path is undefined, returns empty string", () => {
        // Arrange, Act
        const result = controller.getArtifactTextPath(undefined);

        // Assert
        expect(result).toBe("");
    });

    it("getArtifactTextPath, when path is string, returns path", () => {
        // Arrange, Act
        const path = "test";
        const result = controller.getArtifactTextPath(path);

        // Assert
        expect(result).toBe(path);
    });

    it("getArtifactTextPath, when path is string[], returns path join", () => {
        // Arrange, Act
        const path = ["parent", "child"];
        const result = controller.getArtifactTextPath(path);

        // Assert
        expect(result).toBe(path.join(" > "));
    });

    describe("columns", () => {
        it("column properties are correctly defined", () => {
            // Arrange

            // Act

            // Assert
            expect(controller.columns).toEqual([jasmine.objectContaining({
                isGroup: true
            })]);
            expect(angular.isFunction(controller.columns[0].cellClass)).toEqual(true);
            expect(angular.isFunction(controller.columns[0].cellRenderer)).toEqual(true);
        });

        it("getCellClass returns correct result", () => {
            // Arrange
            const vm = {getCellClass: () => ["test"]} as TreeModels.ITreeNodeVM<any>;

            // Act
            const css = controller.columns[0].cellClass(vm);

            // Assert
            expect(css).toEqual(["test"]);
        });

        it("cellRenderer returns correct result", () => {
            // Arrange
            const vm = {
                getIcon() {
                    return "icon";
                },
                getLabel() {
                    return "name";
                }
            } as TreeModels.ITreeNodeVM<any>;
            const cell = {} as HTMLElement;
            const params: IColumnRendererParams = {
                data: vm,
                $scope: $scope,
                eGridCell: cell
            };

            // Act
            const result = controller.columns[0].cellRenderer(params);

            // Assert
            expect(result).toEqual(`icon<span>name</span>`);
        });
    });

    it("onSelect, when ArtifactNodeVM or SubArtifactNodeVM, sets selection", inject(($browser) => {
        // Arrange
        const model = {id: 3} as Models.IArtifact;
        const vm = controller.factory.createArtifactNodeVM(controller.project, model);
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.onSelect(vm, true);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: [vm]});
    }));

    it("onSelect, when InstanceItemNodeVM of type Project, sets project", () => {
        // Arrange
        const model = {id: 11, name: "proj", type: AdminStoreModels.InstanceItemType.Project} as AdminStoreModels.IInstanceItem;
        const vm = controller.factory.createInstanceItemNodeVM(model);

        // Act
        controller.onSelect(vm, true);

        // Assert
        expect(controller.project).toBe(model);
    });

    it("onSelect, when ProjectSearchResultVM, clears search and sets project", () => {
        // Arrange
        const model = {itemId: 15} as SearchServiceModels.IProjectSearchResult;
        const vm = new ProjectSearchResultVM(model, undefined);
        controller.clearSearch = jasmine.createSpy("clearSearch");

        // Act
        controller.onSelect(vm, true);

        // Assert
        expect(controller.clearSearch).toHaveBeenCalled();
        expect(controller.project).toEqual({
            id: model.itemId,
            type: AdminStoreModels.InstanceItemType.Project,
            name: model.name,
            hasChildren: true
        });
    });

    it("set project, when project is defined, clears selection and sets project and root node", inject(($browser) => {
        // Arrange
        const newProject = {id: 6, name: "new", hasChildren: true} as AdminStoreModels.IInstanceItem;
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.project = newProject;

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: []});
        expect(controller.project).toBe(newProject);
        expect(controller.rowData).toEqual([controller.factory.createInstanceItemNodeVM(newProject, true)]);
    }));

    it("set project, when project is undefined, clears selection and project and sets root node", inject(($browser) => {
        // Arrange
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.project = undefined;

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: []});
        expect(controller.project).toBeUndefined();
        expect(controller.rowData).toEqual([controller.factory.createInstanceItemNodeVM({
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true)]);
    }));
});
