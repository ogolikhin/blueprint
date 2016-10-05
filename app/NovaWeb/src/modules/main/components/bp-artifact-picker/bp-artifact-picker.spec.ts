import * as angular from "angular";
import "angular-mocks";
import {BpArtifactPicker, BpArtifactPickerController} from "./bp-artifact-picker";
import {ArtifactPickerNodeVM, InstanceItemNodeVM, ArtifactNodeVM} from "./bp-artifact-picker-node-vm";
import {ILocalizationService} from "../../../core";
import {Models} from "../../models";
import {IProjectManager} from "../../../managers";
import {Project} from "../../../managers/project-manager/";
import {IProjectService} from "../../../managers/project-manager/project-service";

describe("BpArtifactPicker", () => {
    angular.module("bp.components.artifactpicker", [])
        .component("bpArtifactPicker", new BpArtifactPicker());

    beforeEach(angular.mock.module("bp.components.artifactpicker", ($provide: ng.auto.IProvideService) => {
        $provide.service("localization", () => {});
        $provide.service("projectManager", () => { return { getSelectedProject: () => undefined }; });
        $provide.service("projectService", () => { return { abort: () => {} }; });
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
    let projectManager: IProjectManager;
    let projectService: IProjectService;
    let controller: BpArtifactPickerController;
    const project = {id: 1} as Project;

    beforeEach(inject(($rootScope: ng.IRootScopeService) => {
        $scope = $rootScope.$new();
        const localization = {} as ILocalizationService;
        projectManager = jasmine.createSpyObj("projectManager", ["getSelectedProject", "getArtifact"]) as IProjectManager;
        (projectManager.getSelectedProject as jasmine.Spy).and.returnValue(project);
        projectService = jasmine.createSpyObj("projectService", ["abort", "searchProjects"]) as IProjectService;
        controller = new BpArtifactPickerController($scope, localization, projectManager, projectService);
    }));

    it("$onInit sets selected project", () => {
        // Arrange

        // Act
        controller.$onInit();

        // Assert
        expect(controller.project).toEqual(project);
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
    });

    it("search, when search text is not empty, performs search", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        // Arrange
        controller.searchText = "test";
        const searchResults = [] as Models.IProjectNode[];
        (projectService.searchProjects as jasmine.Spy).and.returnValue($q.resolve(searchResults));

        // Act
        controller.search();

        // Assert
        expect(controller.isSearching).toEqual(true);
        $rootScope.$digest(); // Resolves promises
        expect(controller.isSearching).toEqual(false);
        expect(controller.searchResults).toEqual([]);
    }));

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
            const vm = {getCellClass: () => ["test"]} as ArtifactPickerNodeVM<any>;

            // Act
            const css = controller.columns[0].cellClass(vm);

            // Assert
            expect(css).toEqual(["test"]);
        });

        it("innerRenderer returns correct result", () => {
            // Arrange
            const vm = {name: "name", getIcon() { return "icon"; }} as ArtifactPickerNodeVM<any>;
            const cell = {} as HTMLElement;

            // Act
            const result = controller.columns[0].innerRenderer(vm, cell);

            // Assert
            expect(result).toEqual(`<span class="ag-group-value-wrapper">icon<span>name</span></span>`);
        });
    });

    it("onSelect, when InstanceItemNodeVM of type Project, clears selection and sets project", inject(($browser) => {
        // Arrange
        const model = {id: 11, type: Models.ProjectNodeType.Project} as Models.IProjectNode;
        const vm = new InstanceItemNodeVM(projectManager, projectService, controller, model);
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.onSelect(vm, true, [vm]);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: []});
        expect(controller.project).toBe(model);
    }));

    it("onSelect, when InstanceItemNodeVM of type Folder, clears selection", inject(($browser) => {
        // Arrange
        const model = {id: 99, type: Models.ProjectNodeType.Folder} as Models.IProjectNode;
        const vm = new InstanceItemNodeVM(projectManager, projectService, controller, model);
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.onSelect(vm, true, [vm]);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: []});
    }));

    it("onSelect, when InstanceItemNodeVM of type Project, clears selection and sets project", inject(($browser) => {
        // Arrange
        const model = {id: 11, type: Models.ProjectNodeType.Project} as Models.IProjectNode;
        const vm = new InstanceItemNodeVM(projectManager, projectService, controller, model);
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.onSelect(vm, true, [vm]);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: []});
        expect(controller.project).toBe(model);
    }));

    it("onSelect, when ArtifactNodeVM or SubArtifactNodeVM, sets selection", inject(($browser) => {
        // Arrange
        const model = {id: 3} as Models.IArtifact;
        const vm = new ArtifactNodeVM(projectManager, projectService, controller, model);
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.onSelect(vm, true, [vm]);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: [vm]});
    }));

    it("project, when defined, clears selection and sets project and root node", inject(($browser) => {
        // Arrange
        const newProject = {id: 6, name: "new", hasChildren: true} as Models.IProject;
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.project = newProject;

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: []});
        expect(controller.project).toBe(newProject);
        expect(controller.rootNode).toEqual(new InstanceItemNodeVM(projectManager, projectService, controller, {
            id: 6,
            type: Models.ProjectNodeType.Project,
            name: "new",
            hasChildren: true
        } as Models.IProjectNode, true));
    }));

    it("project, when undefined, clears search, selection and project and sets root node", inject(($browser) => {
        // Arrange
        controller.onSelectionChanged = jasmine.createSpy("onSelectionChanged");

        // Act
        controller.project = undefined;

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.searchText).toBeUndefined();
        expect(controller.searchResults).toBeUndefined();
        expect(controller.onSelectionChanged).toHaveBeenCalledWith({selectedVMs: []});
        expect(controller.project).toBeUndefined();
        expect(controller.rootNode).toEqual(new InstanceItemNodeVM(projectManager, projectService, controller, {
            id: 0,
            type: Models.ProjectNodeType.Folder,
            name: "",
            hasChildren: true
        } as Models.IProjectNode, true));
    }));
});
