import "angular";
import "angular-mocks";
import {IOpenProjectResult, OpenProjectController} from "./open-project";
import {LocalizationServiceMock} from "../../../shell/login/mocks.spec";
import {ProjectRepositoryMock} from "../../services/project-repository.mock";
import {ProjectManager} from "../../managers/project-manager";


export class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {

    public close(result?: any): void {}

    public dismiss(reason?: any): void {}

    public result: angular.IPromise<any>;

    public opened: angular.IPromise<any>;

    public rendered: angular.IPromise<any>;
}



describe("Open Project.", () => {
    var controller: OpenProjectController;

    beforeEach(() => {
        controller = new OpenProjectController(
            null,
            new LocalizationServiceMock(),
            new ModalServiceInstanceMock(),
            null,
            null, null, null);
    });

    describe("Return value.", () => {
        it("check return empty value", () => {

        // Arrange
            var result: IOpenProjectResult = <IOpenProjectResult>{
                id: -1,
                name: "",
                description: ""
            };
        // Act

        // Assert
        expect(controller.returnvalue).toBeDefined();
        expect(controller.returnvalue).toEqual(result);
    });
    });
    it("innerRenderer", () => {
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
        expect(cellRenderer).toEqual("artifact");
        expect(cellRendererFolder).toEqual("folder");
        expect(cellRendererProject).toContain("project");
        expect(cellRendererProject).not.toContain("<button");
    });

    describe("Verify control.", () => {
        it("Checking options: ", () => {

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
    });
    });

describe("Embedded ag-grid events", () => {
    var controller: OpenProjectController;
    var $scope, elem;

    beforeEach(inject(function (_$q_, _$rootScope_, _$compile_) {
        $scope = _$rootScope_.$new();

        elem = angular.element(`<div ag-grid="ctrl.gridOptions" class="ag-grid"></div>`);

        controller = new OpenProjectController(
            $scope,
            new LocalizationServiceMock(),
            new ModalServiceInstanceMock(),
            null, //new ProjectManager((_$q_),
            null,
            null,
            null);
        _$compile_(elem)($scope);

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
});