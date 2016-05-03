import "angular";
import "angular-mocks"
import * as $D from "../../../services/dialog.svc";
import {IOpenProjectResult, OpenProjectController} from "./openproject.ctrl";
import {LocalizationServiceMock} from "../../../shell/login/mocks.spec";
import {IProjectService} from "../../../services/project.svc";
import {GridApi, InMemoryRowModel, RowNode} from "ag-grid/main";

export class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {

    constructor() {
    }

    public close(result?: any): void {
        
    }

    public dismiss(reason?: any): void {
    }

    public result: angular.IPromise<any>;

    public opened: angular.IPromise<any>;

    public rendered: angular.IPromise<any>;
}

describe("Open Project.", () => {
    var controller: OpenProjectController;

    beforeEach(() => {
        controller = new OpenProjectController(null, new LocalizationServiceMock(), new ModalServiceInstanceMock(), null, null, null, null);
    });

    describe("Return value.", () => {
        it("check return empty value", () => {

            // Arrange
            var result: IOpenProjectResult =  <IOpenProjectResult>{
                id: -1,
                name: "",
                description:""
            }
            // Act

            // Assert
            expect(controller.returnvalue).toBeDefined();
            expect(controller.returnvalue).toEqual(result);
        });
    });
    describe("Verify control.", () => {
        it("Checking options: ", () => {
            
            // Arrange

            // Act
            var options = controller.gridOptions;
            
            // Assert
            expect(options).toBeDefined();
            expect(options.columnDefs).toBeDefined();
            expect(options.columnDefs).toEqual(jasmine.any(Array));
            expect(options.columnDefs.length).toBeGreaterThan(0);
            expect(options.columnDefs[0].field).toBeDefined();
            expect(options.columnDefs[0].headerName).toBe("App_Header_Name");
            expect(options.columnDefs[0].cellRenderer).toBeDefined();
            expect(options.columnDefs[0].cellRendererParams).toBeDefined();
            expect(options.columnDefs[0].cellRendererParams.innerRenderer).toBeDefined();
            expect(options.getNodeChildDetails).toEqual(jasmine.any(Function));
            expect(options.onCellFocused).toEqual(jasmine.any(Function));
            expect(options.onRowGroupOpened).toEqual(jasmine.any(Function));
            expect(options.onGridReady).toEqual(jasmine.any(Function));
        });
    });
});

export class ProjectServiceMock implements IProjectService {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) {
    }

    public getFolders(id?: number): angular.IPromise<any[]> {
        var deferred = this.$q.defer<any[]>();
        var folders = [
            {
                "Id": 3,
                "ParentFolderId": 1,
                "Name": "Folder with content",
                "Type": "Folder"
            },
            {
                "Id": 7,
                "ParentFolderId": 1,
                "Name": "Empty folder",
                "Type": "Folder"
            },
            {
                "Id": 33,
                "ParentFolderId": 1,
                "Name": "Process",
                "Description": "Process description",
                "Type": "Project"
            }
        ];
        deferred.resolve(folders);
        return deferred.promise;
    }
}

describe("Embedded ag-grid events", () => {
    var controller: OpenProjectController;
    var $scope, elem;
    var gridApi = new GridApi();

    beforeEach(inject(function(_$q_, _$rootScope_, _$compile_) {
        $scope = _$rootScope_.$new();

        elem = angular.element('<div ag-grid="ctrl.gridOptions" class="ag-grid"></div>');

        controller = new OpenProjectController($scope, new LocalizationServiceMock(), new ModalServiceInstanceMock(), new ProjectServiceMock(_$q_), null, null, null);
        _$compile_(elem)($scope);

        $scope.$digest();
    }));

    it("getNodeChildDetails", () => {
        // Arrange
        var rowItemMock = {
            Children: true,
            open: true,
            Id: 1
        };
        var rowItemMockNoChildren = {};

        // Act
        var options = controller.gridOptions;
        var node = options.getNodeChildDetails(rowItemMock);
        var nodeNoChildren = options.getNodeChildDetails(rowItemMockNoChildren);

        // Assert
        expect(node.key).toEqual(1);
        expect(node.expanded).toBeTruthy();
        expect(nodeNoChildren).toBeNull();
    });

    it("innerRenderer", () => {
        // Arrange
        var paramsMock = {
            data: {
                Name: "artifact"
            }
        };
        var paramsMockFolder = {
            data: {
                Type: "Folder",
                Name: "folder"
            }
        };
        var paramsMockProject = {
            data: {
                Type: "Project",
                Name: "project"
            },
            eGridCell: document.createElement("div")
        };

        // Act
        var options = controller.gridOptions;
        var cellRenderer = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMock);
        var cellRendererFolder = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMockFolder);
        var cellRendererProject = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMockProject);

        // Assert
        expect(cellRenderer).toEqual("artifact");
        expect(cellRendererFolder).toEqual("folder");
        expect(cellRendererProject).toContain("project");
    });

    it("onEnterKeyOnProject", () => {
        // Arrange
        var event = new Event("keydown");
        var div = document.createElement("div");
        var paramsMock = {
            data: {
                Type: "Project",
                Name: "project"
            },
            eGridCell: div
        };

        // Act
        var options = controller.gridOptions;
        var cellRenderer = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMock);
        div.dispatchEvent(event);

        // Assert
        expect(cellRenderer).toContain("project");
    });

    it("onGidReady", () => {
        // Arrange
        var dataFromCall;
        var paramsMock = {
            api: {
                sizeColumnsToFit: function() {}
            },
            columnApi: {
                autoSizeColumns: function(columnName) {}
            }
        };
        var setRowDataMock = function(data) {
            dataFromCall = data;
        };
        gridApi.setRowData = setRowDataMock;

        // Act
        var options = controller.gridOptions;
        controller.gridOptions.api = gridApi;
        options.onGridReady(paramsMock);
        $scope.$apply();

        // Assert
        expect(dataFromCall.length).toEqual(3);
        expect(dataFromCall[2].Type).toBe("Project");
    });

    it("rowGroupOpened", () => {
        // Arrange
        var dataFromCall;
        var paramsMock = {
            node: {
                data: {
                    Id: 77,
                    Type: "Folder",
                    Name: "folder",
                    Children: [],
                    open: null,
                    alreadyLoadedFromServer: null
                },
                expanded: true
            }
        };
        var setRowDataMock = function(data) {
            dataFromCall = data;
        };
        gridApi.setRowData = setRowDataMock;

        // Act
        var options = controller.gridOptions;
        controller.gridOptions.api = gridApi;
        options.onRowGroupOpened(paramsMock);
        $scope.$apply();

        // Assert
        expect(paramsMock.node.data.Children.length).toEqual(3);
        expect(paramsMock.node.data.Children[2].Type).toBe("Project");
        expect(paramsMock.node.data.open).toBeTruthy();
        expect(paramsMock.node.data.alreadyLoadedFromServer).toBeTruthy();
    });

    /*it("cellFocused", () => {
        // Arrange

        var paramsMock = {
            rowIndex: 0
        };
        var getModel = function() {
            var rowModel = new InMemoryRowModel();
            return rowModel;
        };
        gridApi.getModel = getModel;

        // Act
        var options = controller.gridOptions;
        controller.gridOptions.api = gridApi;
        options.onCellFocused(paramsMock);
        $scope.$apply();

        // Assert
    });*/
});