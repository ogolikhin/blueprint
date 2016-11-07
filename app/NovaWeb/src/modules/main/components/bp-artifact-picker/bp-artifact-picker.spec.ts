import * as angular from "angular";
import "angular-mocks";
import {BpArtifactPicker, BpArtifactPickerController} from "./bp-artifact-picker";
import {ArtifactSearchResultVM} from "./bp-artifact-picker-search-vm";
import {ILocalizationService} from "../../../core";
import {Models, AdminStoreModels, SearchServiceModels, TreeViewModels} from "../../models";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {IColumnRendererParams} from "../../../shared/widgets/bp-tree-view/";

describe("BpArtifactPicker", () => {
    angular.module("bp.components.artifactpicker", [])
        .component("bpArtifactPicker", new BpArtifactPicker());

    beforeEach(angular.mock.module("bp.components.artifactpicker", ($provide: ng.auto.IProvideService) => {
        $provide.service("localization", () => undefined);
        $provide.service("artifactManager", () => ({
            selection: {
                getArtifact: () => ({projectId: 1})
            }
        }));
        $provide.service("projectManager", () => ({
            getProject: (id: number) => ({id: id, name: "default"})
        }));
        $provide.service("projectService", () => ({
            abort: () => { return; }
        }));
    }));

    it("Values are bound", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        const element = `<bp-artifact-picker selectable-item-types="[1, 2]"
                                             selection-mode="'multiple'"
                                             show-sub-artifacts="true"
                                             on-selection-changed="$ctrl.onSelectionChanged(selectedItems)">`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("bpArtifactPicker") as BpArtifactPickerController;

        // Assert
        expect(controller.selectableItemTypes).toEqual([1, 2]);
        expect(controller.selectionMode).toEqual("multiple");
        expect(controller.showSubArtifacts).toEqual(true);
        expect(angular.isFunction(controller.onSelectionChanged)).toEqual(true);
    }));

    it("Defaults values are applied", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        const element = `<bp-artifact-picker />`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("bpArtifactPicker") as BpArtifactPickerController;

        // Assert
        expect(controller.selectableItemTypes).toBeUndefined();
        expect(controller.selectionMode).toEqual("single");
        expect(controller.showSubArtifacts).toEqual(false);
        expect(controller.onSelectionChanged).toBeUndefined();
    }));
});

describe("BpArtifactPickerController", () => {
    let $scope: ng.IScope;
    let artifactManager: IArtifactManager;
    let projectService: IProjectService;
    let controller: BpArtifactPickerController;
    const artifact = {projectId: 1};
    const project = {id: 1, name: "default", hasChildren: true};

    beforeEach(inject(($rootScope: ng.IRootScopeService) => {
        $scope = $rootScope.$new();
        const localization = {} as ILocalizationService;
        artifactManager = {selection: jasmine.createSpyObj("selectionManager", ["getArtifact"])} as IArtifactManager;
        (artifactManager.selection.getArtifact as jasmine.Spy).and.returnValue(artifact);
        const projectManager = jasmine.createSpyObj("projectManager", ["getProject"]) as IProjectManager;
        (projectManager.getProject as jasmine.Spy).and.returnValue(project);
        projectService = jasmine.createSpyObj("projectService", ["abort", "searchItemNames", "searchProjects"]) as IProjectService;
        controller = new BpArtifactPickerController($scope, localization, artifactManager, projectManager, projectService);
    }));

    it("$onInit sets selected project", () => {
        // Arrange

        // Act
        controller.$onInit();

        // Assert
        expect(controller.project).toEqual({
            id: project.id,
            type: AdminStoreModels.InstanceItemType.Project,
            name: project.name,
            hasChildren: project.hasChildren
        });
    });

    it("$onDestroy cleans up", () => {
        // Arrange

        // Act
        controller.$onDestroy();

        // Assert
        expect(controller.columns).toBeUndefined();
        expect(controller.onSelect).toBeUndefined();
        expect(projectService.abort).toHaveBeenCalled();
    });

    it("clearSearch clears text and results", () => {
        // Arrange

        // Act
        controller.clearSearch();

        // Assert
        expect(controller.searchText).toBeUndefined();
        expect(controller.searchResults).toBeUndefined();
        expect(controller.isMoreSearchResults).toBeUndefined();
    });

    it("search, when project is set, searches artifacts", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        // Arrange
        controller.project = {id: 2, name: "proj", type: AdminStoreModels.InstanceItemType.Project} as AdminStoreModels.IInstanceItem;
        controller.searchText = "test";
        const searchResults = {items: []} as SearchServiceModels.IItemNameSearchResultSet;
        (projectService.searchItemNames as jasmine.Spy).and.returnValue($q.resolve(searchResults));

        // Act
        controller.search();

        // Assert
        expect(controller.isSearching).toEqual(true);
        expect(projectService.searchItemNames).toHaveBeenCalledWith({
            query: "test",
            projectIds: [2],
            predefinedTypeIds: undefined,
            includeArtifactPath: true
        }, 0, 101);
        $rootScope.$digest(); // Resolves promises
        expect(controller.isSearching).toEqual(false);
        expect(controller.searchResults).toEqual([]);
        expect(controller.isMoreSearchResults).toEqual(false);
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
        expect(projectService.searchProjects).toHaveBeenCalledWith({query: "test"}, 101);
        $rootScope.$digest(); // Resolves promises
        expect(controller.isSearching).toEqual(false);
        expect(controller.searchResults).toEqual([]);
        expect(controller.isMoreSearchResults).toEqual(false);
    }));

    it("onSearchResultDoubleClick, when single-selection mode, calls onDoubleClick", () => {
        // Arrange
        const model = {id: 13, itemId: 13, predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
        const vm = new ArtifactSearchResultVM(model, controller.onSelect);
        controller.selectionMode = "single";
        controller.onDoubleClick = jasmine.createSpy("onDoubleClick");

        // Act
        controller.onDouble(vm);

        // Assert
        expect(controller.onDoubleClick).toHaveBeenCalledWith({vm: vm});
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
            expect(angular.isFunction(controller.columns[0].innerRenderer)).toEqual(true);
        });

        it("getCellClass returns correct result", () => {
            // Arrange
            const vm = {getCellClass: () => ["test"]} as TreeViewModels.TreeViewNodeVM<any>;

            // Act
            const css = controller.columns[0].cellClass(vm);

            // Assert
            expect(css).toEqual(["test"]);
        });

        it("innerRenderer returns correct result", () => {
            // Arrange
            const vm = {
                name: "name", getIcon() {
                    return "icon";
                }
            } as TreeViewModels.TreeViewNodeVM<any>;
            const cell = {} as HTMLElement;
            const params: IColumnRendererParams = {
                vm: vm,
                $scope: $scope,
                eGridCell: cell
            };
            // Act
            const result = controller.columns[0].innerRenderer(params);

            // Assert
            expect(result).toEqual(`<span class="ag-group-value-wrapper">icon<span>name</span></span>`);
        });
    });

    it("onSelect, when ArtifactNodeVM or SubArtifactNodeVM, sets selection", inject(($browser) => {
        // Arrange
        const model = {id: 3} as Models.IArtifact;
        const vm = new TreeViewModels.ArtifactNodeVM(artifactManager, projectService, controller, model);
        controller.project = {id: 6, name: "new", hasChildren: true} as AdminStoreModels.IInstanceItem;
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
        const vm = new TreeViewModels.InstanceItemNodeVM(artifactManager, projectService, controller, model);

        // Act
        controller.onSelect(vm, true);

        // Assert
        expect(controller.project).toBe(model);
    });

    it("onSelect, when SearchResultVM, clears search and sets project", () => {
        // Arrange
        const model = {id: 13, itemId: 13, predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
        const vm = new ArtifactSearchResultVM(model, controller.onSelect);
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
        expect(controller.rootNode).toEqual(new TreeViewModels.InstanceItemNodeVM(artifactManager, projectService, controller, newProject, true));
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
        expect(controller.rootNode).toEqual(new TreeViewModels.InstanceItemNodeVM(artifactManager, projectService, controller, {
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true));
    }));
});
