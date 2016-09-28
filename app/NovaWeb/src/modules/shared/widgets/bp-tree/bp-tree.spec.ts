import * as angular from "angular";
import "angular-mocks";
import {BPTreeController, ITreeNode} from "./bp-tree";
import {GridApi} from "ag-grid/main";

function toFlat(root: any): any[] {
    var stack: any[] = angular.isArray(root) ? root.slice() : [root], array: any[] = [];
    while (stack.length !== 0) {
        var node = stack.shift();
        array.push(node);
        if (angular.isArray(node.children)) {

            for (var i = node.children.length - 1; i >= 0; i--) {
                stack.push(node.children[i]);
            }
            node.children = null;
        }
    }

    return array;
}

describe("Embedded ag-grid events", () => {
    var controller: BPTreeController;
    var $scope, elem;
    var gridApi = new GridApi();
    gridApi.setFocusedCell = () => { };

    beforeEach(inject(function(_$q_, _$rootScope_, _$compile_, $timeout) {
        $scope = _$rootScope_.$new();

        elem = angular.element(`<div ag-grid="$ctrl.gridOptions" class="ag-grid"></div>`);

        controller = new BPTreeController($scope, $timeout);
        _$compile_(elem)($scope);
        //act
        controller.gridColumns = [{
            headerName: "Header",
            field: "name",
            cellClassRules: {
                "has-children": function (params) { return params.data.type === "Folder" && params.data.hasChildren; },
                "is-project": function (params) { return params.data.type === "Project"; }
            },
            cellRenderer: "group",
            cellRendererParams: {
                innerRenderer: (params) => {
                    return params.data.name;
                }
            }
        }];

        controller.$onInit();

        $scope.$digest();
    }));

    it("getNodeChildDetails", () => {
        // Arrange
        var rowItemMock = {
            children: true,
            open: true,
            id: 1
        };

        var rowItemMockNoChildren = {};

        // Act
        var options = controller.options;
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
                name: "artifact"
            }
        };
        var paramsMockFolder = {
            data: {
                type: "Folder",
            }
        };

        // Act
        var options = controller.options;
        var cellRenderer = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMock);
        var cellRendererFolder = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMockFolder);

        // Assert
        expect(cellRenderer).toEqual("artifact");
        expect(cellRendererFolder).toEqual(undefined);
    });

    it("add nodes with property map", inject(($q: ng.IQService) => {
        // Arrange
        var dataFromCall: ITreeNode[];
        gridApi.setRowData = function (data) {
            dataFromCall = data as ITreeNode[];
        };

        controller.options.api = gridApi;
        $scope.$apply();
        controller.propertyMap = {
            itemId: "id",
            TheName: "name"
        };
        // Act
        controller.reload([{ itemId: 1, TheName: `Name 1` }]);
        // Assert
        //let data = controller.options.api.getRenderedNodes();
        expect(dataFromCall).toEqual(jasmine.any(Array));
        expect(dataFromCall.length).toBe(1);
        expect(dataFromCall[0].id).toBe(1);
        expect(dataFromCall[0].name).toBe("Name 1");

    }));

    it("add nodes with property map with not matched properties", inject(($q: ng.IQService) => {
        // Arrange
        var dataFromCall: ITreeNode[];
        gridApi.setRowData = function (data) {
            dataFromCall = data as ITreeNode[];
        };

        // Act
        controller.options.api = gridApi;
        controller.propertyMap = {
            id: "id",
            name: "name"
        };
        $scope.$apply();

        controller.reload([{ itemId: 1, TheName: `Name 1` }]);
        // Assert
        //let data = controller.options.api.getRenderedNodes();
        expect(dataFromCall).toEqual(jasmine.any(Array));
        expect(dataFromCall.length).toBe(1);
        expect(dataFromCall[0].id).toBeUndefined();
        expect(dataFromCall[0].name).toBeUndefined();
        expect(dataFromCall[0]["itemId"]).toBeDefined();
        expect(dataFromCall[0]["TheName"]).toBeDefined();

    }));



    it("add nodes", inject(($q: ng.IQService) => {
        // Arrange
        var dataFromCall;
        gridApi.setRowData = function (data) {
            dataFromCall = data;
        };

        // Act
        controller.options.api = gridApi;
        $scope.$apply();

        controller.reload([
            { id: 1, Name: `Name 1` },
            { id: 2, Name: `Name 2` }
        ]);
        // Assert
        //let data = controller.options.api.getRenderedNodes();
        expect(dataFromCall).toEqual(jasmine.any(Array));
        expect(dataFromCall.length).toBe(2);
    }));
    it("add children to node", inject(($q: ng.IQService) => {
        // Arrange
        var dataFromCall;
        gridApi.setRowData = function (data) {
            dataFromCall = data;
        };

        // Act
        controller.options.api = gridApi;
        $scope.$apply();

        controller.reload([
            { id: 1, Name: `Name 1` },
            { id: 2, Name: `Name 2` }
        ]);
        controller.reload( [
            { id: 3, Name: `Name 3` },
            { id: 4, Name: `Name 4` }
        ], 1);
        
        // Assert
        //let data = controller.options.api.getRenderedNodes();
        expect(dataFromCall).toEqual(jasmine.any(Array));
        expect(dataFromCall.length).toBe(2);
        expect(toFlat(dataFromCall).length).toBe(4);
        expect(dataFromCall[0].open).toBeTruthy;
        expect(dataFromCall[0].loaded).toBeTruthy;

    }));


    
});
