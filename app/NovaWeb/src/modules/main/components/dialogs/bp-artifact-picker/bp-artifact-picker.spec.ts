import "angular";
import "angular-mocks";
import {ArtifactPickerController, IArtifactPickerOptions} from "./bp-artifact-picker";
import {ArtifactPickerNodeVM, InstanceItemNodeVM, ArtifactNodeVM} from "./bp-artifact-picker-node-vm";
import {ILocalizationService} from "../../../../core";
import {Models} from "../../../models";
import {IProjectManager} from "../../../../managers";
import {Project} from "../../../../managers/project-manager/";
import {IProjectService} from "../../../../managers/project-manager/project-service";

describe("ArtifactPickerController", () => {
    let $scope: ng.IScope;
    let projectManager: IProjectManager;
    let projectService: IProjectService;
    let options: IArtifactPickerOptions;
    let controller: ArtifactPickerController;
    const project = {id: 1} as Project;

    beforeEach(inject(($rootScope: ng.IRootScopeService) => {
        const $instance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {};
        $scope = $rootScope.$new();
        const localization = {} as ILocalizationService;
        projectManager = jasmine.createSpyObj("projectManager", ["getSelectedProject", "getArtifact"]) as IProjectManager;
        (projectManager.getSelectedProject as jasmine.Spy).and.returnValue(project);
        projectService = {} as IProjectService;
        options = {};
        controller = new ArtifactPickerController($instance, dialogSettings, $scope, localization, projectManager, projectService, options);
    }));

    it("constructor sets selected project", inject(($rootScope: ng.IRootScopeService) => {
        // Arrange

        // Act

        // Assert
        expect(controller.project).toEqual(project);
    }));

    it("constructor cleans up on $destroy", inject(($rootScope: ng.IRootScopeService) => {
        // Arrange

        // Act
        $scope.$broadcast("$destroy");

        // Assert
        expect(controller.columns).toBeUndefined();
        expect(controller.onSelect).toBeUndefined();
    }));

    describe("columnDefs", () => {
        it("columnDefs properties are correctly defined", () => {
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

        it("getRendering returns correct result", () => {
            // Arrange
            const vm = {name: "name", getIcon() { return "icon"; }} as ArtifactPickerNodeVM<any>;

            // Act
            const result = controller.columns[0].innerRenderer(vm);

            // Assert
            expect(result).toEqual(`<span class="ag-group-value-wrapper">icon<span>name</span></span>`);
        });
    });

    it("onSelect, when ArtifactNodeVM or SubArtifactNodeVM, sets selected item", inject(($browser) => {
        // Arrange
        const model = {id: 3} as Models.IArtifact;
        const vm = new ArtifactNodeVM(projectManager, projectService, options, model);

        // Act
        controller.onSelect(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toEqual([model]);
    }));

    it("onSelect, when InstanceItemNodeVM of type Project, clears selected item and sets project", inject(($browser) => {
        // Arrange
        const model = {id: 11, type: Models.ProjectNodeType.Project} as Models.IProjectNode;
        const vm = new InstanceItemNodeVM(projectManager, projectService, options, model);

        // Act
        controller.onSelect(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toEqual([]);
        expect(controller.project).toBe(model);
    }));

    it("onSelect, when InstanceItemNodeVM of type Folder, clears selected item", inject(($browser) => {
        // Arrange
        const model = {id: 99, type: Models.ProjectNodeType.Folder} as Models.IProjectNode;
        const vm = new InstanceItemNodeVM(projectManager, projectService, options, model);

        // Act
        controller.onSelect(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toEqual([]);
    }));

    it("project, when defined, clears selected item and sets project and root node", inject(($browser) => {
        // Arrange
        const newProject = {id: 6, name: "new", hasChildren: true} as Models.IProject;

        // Act
        controller.project = newProject;

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toEqual([]);
        expect(controller.project).toBe(newProject);
        expect(controller.rootNode).toEqual(new InstanceItemNodeVM(projectManager, projectService, options, {
            id: 6,
            type: Models.ProjectNodeType.Project,
            name: "new",
            hasChildren: true
        } as Models.IProjectNode, true));
    }));

    it("project, when undefined, clears selected item and project and sets root node", inject(($browser) => {
        // Arrange

        // Act
        controller.project = undefined;

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toEqual([]);
        expect(controller.project).toBeUndefined();
        expect(controller.rootNode).toEqual(new InstanceItemNodeVM(projectManager, projectService, options, {
            id: 0,
            type: Models.ProjectNodeType.Folder,
            name: "",
            hasChildren: true
        } as Models.IProjectNode, true));
    }));
});
