import "angular";
import "angular-mocks";
import { OpenProjectController } from "./open-project";
import { SettingsService } from "../../../core";
import { MessageService } from "../../../shell/";
import { ProjectManager, SelectionManager } from "../../services";
import { BPTreeControllerMock } from "../../../shared/widgets/bp-tree/bp-tree.mock";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ProjectRepositoryMock } from "../../services/project-repository.mock";

export class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {

    public close(result?: any): void {}

    public dismiss(reason?: any): void {}

    public result: angular.IPromise<any>;   

    public opened: angular.IPromise<any>;

    public rendered: angular.IPromise<any>;

    public closed: angular.IPromise<any>;
}
var controller: OpenProjectController;


describe("Open Project.", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));


    beforeEach(inject((localization: LocalizationServiceMock)  => {
        controller = new OpenProjectController(
            null,
            localization,
            new ModalServiceInstanceMock(),
            null,
            null, null, null);

    }));

    it("Test InnerRenderer", () => {
        // Arrange
        var paramsMock = {
            data: {
                name: "artifact"
            }
        };
        var paramsMockFolder = {
            data: {
                type: "Folder",
                name: "folder"
            }
        };
        var paramsMockProject = {
            data: {
                type: "Project",
                name: "<button onclick=\"alert('HEY!')\";>project</button>"
            },
            eGridCell: document.createElement("div")
        };

        // Act
        var columns = controller.columns;
        var cellRenderer = columns[0].cellRendererParams.innerRenderer(paramsMock);
        var cellRendererFolder = columns[0].cellRendererParams.innerRenderer(paramsMockFolder);
        var cellRendererProject = columns[0].cellRendererParams.innerRenderer(paramsMockProject);

        // Assert
        expect(cellRenderer).toContain("artifact");
        expect(cellRendererFolder).toContain("folder");
        expect(cellRendererProject).toContain("project");
        expect(cellRendererProject).not.toContain("<button");
    });

    it("Test options ", () => {

        // Arrange

        // Act
        var columns = controller.columns;

        //// Assert
        expect(columns).toBeDefined();
        expect(columns).toEqual(jasmine.any(Array));
        expect(columns.length).toBeGreaterThan(0);
        expect(columns[0].field).toBeDefined();
        expect(columns[0].headerName).toBe("App_Header_Name");
        expect(columns[0].cellRenderer).toBeDefined();
        expect(columns[0].cellRendererParams).toBeDefined();
        expect(columns[0].cellRendererParams.innerRenderer).toBeDefined();
    });

    it("propertyMap", () => {
        // Arrange
        // Act
        // Assert

        expect(controller.propertyMap).toBeDefined();
        expect(controller.propertyMap["id"]).toEqual("id");
        expect(controller.propertyMap["type"]).toEqual("type");
        expect(controller.propertyMap["name"]).toEqual("name");
        expect(controller.propertyMap["hasChildren"]).toEqual("hasChildren");

    });
    it("selectItem", () => {
        // Arrange
        let item = { id: -1, name: "", description: "", itemTypeId: -1 };
        // Act
        controller["setSelectedItem"](null);

        let _selected = controller.selectedItem;
        // Assert
        expect(_selected).toEqual(item);

    });
    it("isProjectSelected", () => {
        // Arrange
        let item = { id: 1, name: "Project", description: "", type: 1 };
        // Act
        controller["setSelectedItem"](item);

        let _selected = controller.isProjectSelected;
        // Assert
        expect(_selected).toBeTruthy();

    });
    it("returnValue", () => {
        // Arrange
        let item = { id: 1, name: "Project", description: "", type: 1 };
        // Act
        controller["setSelectedItem"](item);

        let _selected = controller.returnValue;
        // Assert
        expect(_selected).toBeDefined;

    });

    

});

describe("Embedded ag-grid events", () => {
    let $scope;
    let elem;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("settings", SettingsService);
        $provide.service("messageService", MessageService);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("manager", ProjectManager);
        $provide.service("selectionManager", SelectionManager);

    }));
    beforeEach(inject((
        $q: ng.IQService,
        $rootScope: ng.IRootScopeService,
        $compile: ng.ICompileService,
        manager: ProjectManager,
        localization: LocalizationServiceMock
    ) => {
        $rootScope["config"] = {
            "settings": {
                "StorytellerMessageTimeout": `{ "Warning": 0, "Info": 3000, "Error": 0 }`
            }
        };
        manager.initialize();

        $scope = $rootScope.$new();

        elem = angular.element(`<div ag-grid="ctrl.gridOptions" class="ag-grid"></div>`);

        controller = new OpenProjectController(
            $scope,
            localization,
            new ModalServiceInstanceMock(),
            manager,
            null,
            null,
            null);

        controller["tree"] = new BPTreeControllerMock();

        $compile(elem)($scope);
        $scope.$digest();
    }));


    it("onEnterKeyOnProject", () => {
        // Arrange
        var event = new Event("keydown");
        var div = document.createElement("div");
        var paramsMock = {
            data: {
                type: "Project",
                name: "project"
            },
            eGridCell: div
        };

        // Act
        var columns = controller.columns;
        var cellRenderer = columns[0].cellRendererParams.innerRenderer(paramsMock);
        div.dispatchEvent(event);

        // Assert
        expect(cellRenderer).toContain("project");
    });

    it("Load data", inject(($rootScope: ng.IRootScopeService) => {
        // Arrange
        controller.doLoad({ id: 1 });
        var event = new Event("keydown");
        var div = document.createElement("div");
        var paramsMock = {
            data: {
                type: "Project",
                name: "project"
            },
            eGridCell: div
        };

        // Act
        var columns = controller.columns;
        var cellRenderer = columns[0].cellRendererParams.innerRenderer(paramsMock);
        div.dispatchEvent(event);

        // Assert
        expect(cellRenderer).toContain("project");
    }));

    it("Load empty data", inject(($rootScope: ng.IRootScopeService) => {

        // Arrange

        // Act, load empty datasource
        controller.doLoad({ id: -1 });
        $rootScope.$digest();

        // Assert
        expect(controller.hasError).toBeTruthy();
        expect(controller.errorMessage).toEqual("Project_NoProjectsAvailable");
    }));


});
