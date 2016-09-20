import "angular";
import "angular-mocks";
import {ArtifactPickerController} from "./bp-artifact-picker";
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
        const dialogData = {ItemTypePredefines: [] as Models.ItemTypePredefined[]};
        controller = new ArtifactPickerController($instance, dialogSettings, $scope, localization, projectManager, projectService, dialogData);
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
        expect(controller.columnDefs).toBeUndefined();
        expect(controller.onSelect).toBeUndefined();
    }));

    describe("columnDefs", () => {
        it("columnDefs properties are correctly defined", () => {
            // Arrange

            // Act

            // Assert
            expect(controller.columnDefs).toEqual([jasmine.objectContaining({
                headerName: "",
                field: "name",
                cellRenderer: "group",
                cellRendererParams: jasmine.objectContaining({}),
                suppressMenu: true,
                suppressSorting: true,
            })]);
            expect(angular.isFunction(controller.columnDefs[0].cellClass)).toEqual(true);
            expect(angular.isFunction(controller.columnDefs[0].cellRendererParams["innerRenderer"])).toEqual(true);
        });

        it("cellClass, when is expandable, correct result", () => {
            // Arrange
            const vm = {isExpandable: true, getTypeClass: () => ""} as ArtifactPickerNodeVM<any>;
            const cellClass = controller.columnDefs[0].cellClass as (cellClassParams: any) => string[];

            // Act
            const css = cellClass({data: vm});

            // Assert
            expect(css).toEqual(["has-children"]);
        });

        it("cellClass, when has type class, correct result", () => {
            // Arrange
            const vm = {isExpandable: false, getTypeClass: () => "is-folder"} as ArtifactPickerNodeVM<any>;
            const cellClass = controller.columnDefs[0].cellClass as (cellClassParams: any) => string[];

            // Act
            const css = cellClass({data: vm});

            // Assert
            expect(css).toEqual(["is-folder"]);
        });

        it("innerRenderer, when InstanceItemNodeVM of type Folder, adds event listener", () => {
            // Arrange
            const cell = jasmine.createSpyObj("cell", ["addEventListener"]);
            const vm = new InstanceItemNodeVM(projectService, {id: 1, type: Models.ProjectNodeType.Folder} as Models.IProjectNode);
            const innerRenderer = controller.columnDefs[0].cellRendererParams["innerRenderer"] as (params: any) => string;

            // Act
            innerRenderer({data: vm, eGridCell: cell});

            // Assert
            expect(cell.addEventListener).toHaveBeenCalledWith("keydown", jasmine.any(Function));
        });

        it("innerRenderer, when ArtifactNodeVM with custom icon, correct result", () => {
            // Arrange
            const vm = new ArtifactNodeVM(projectService, {id: 1, name: "artifact", prefix: "AC"});
            const innerRenderer = controller.columnDefs[0].cellRendererParams["innerRenderer"] as (params: any) => string;
            (projectManager.getArtifact as jasmine.Spy).and.returnValue({metadata: {getItemType() { return {id: 123, iconImageId: 456}; }}});

            // Act
            const result = innerRenderer({data: vm});

            // Assert
            expect(result).toEqual(`<span class="ag-group-value-wrapper"><bp-item-type-icon item-type-id="123" \
item-type-icon="456"></bp-item-type-icon><span>AC1 artifact</span></span>`);
        });

        it("innerRenderer, when not ArtifactNodeVM with custom icon, correct result", () => {
            // Arrange
            const vm = {name: "name"} as ArtifactPickerNodeVM<any>;
            const innerRenderer = controller.columnDefs[0].cellRendererParams["innerRenderer"] as (params: any) => string;

            // Act
            const result = innerRenderer({data: vm});

            // Assert
            expect(result).toEqual(`<span class="ag-group-value-wrapper"><i></i><span>name</span></span>`);
        });
    });

    it("onSelect, when ArtifactNodeVM or SubArtifactNodeVM, sets selected item", inject(($browser) => {
        // Arrange
        const model = {id: 3} as Models.IArtifact;
        const vm = new ArtifactNodeVM(projectService, model);

        // Act
        controller.onSelect(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toBe(model);
    }));

    it("onSelect, when InstanceItemNodeVM of type Project, clears selected item and sets project", inject(($browser) => {
        // Arrange
        const model = {id: 11, type: Models.ProjectNodeType.Project} as Models.IProjectNode;
        const vm = new InstanceItemNodeVM(projectService, model);

        // Act
        controller.onSelect(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toBeUndefined();
        expect(controller.project).toBe(model);
    }));

    it("onSelect, when InstanceItemNodeVM of type Folder, clears selected item", inject(($browser) => {
        // Arrange
        const model = {id: 99, type: Models.ProjectNodeType.Folder} as Models.IProjectNode;
        const vm = new InstanceItemNodeVM(projectService, model);

        // Act
        controller.onSelect(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toBeUndefined();
    }));

    it("project, when defined, clears selected item and sets project and root node", inject(($browser) => {
        // Arrange
        const newProject = {id: 6, name: "new", hasChildren: true} as Models.IProject;

        // Act
        controller.project = newProject;

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.returnValue).toBeUndefined();
        expect(controller.project).toBe(newProject);
        expect(controller.rootNode).toEqual(new InstanceItemNodeVM(projectService, {
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
        expect(controller.returnValue).toBeUndefined();
        expect(controller.project).toBeUndefined();
        expect(controller.rootNode).toEqual(new InstanceItemNodeVM(projectService, {
            id: 0,
            type: Models.ProjectNodeType.Folder,
            name: "",
            hasChildren: true
        } as Models.IProjectNode, true));
    }));
});
