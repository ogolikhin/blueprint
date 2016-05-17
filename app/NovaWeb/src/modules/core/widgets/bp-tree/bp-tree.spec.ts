import "angular";
import "angular-mocks";
import {BPTreeController} from "./bp-tree";
import {GridApi} from "ag-grid/main";


describe("Embedded ag-grid events", () => {
    var controller: BPTreeController;
    var $scope, elem;
    var gridApi = new GridApi();

    beforeEach(inject(function(_$q_, _$rootScope_, _$compile_, $timeout) {
        $scope = _$rootScope_.$new();

        elem = angular.element(`<div ag-grid="$ctrl.gridOptions" class="ag-grid"></div>`);

        controller = new BPTreeController($scope, $timeout);
        _$compile_(elem)($scope);
        //act
        controller.gridColumns = [{
            headerName: "Header",
            field: "Name",
            cellClassRules: {
                "has-children": function (params) { return params.data.Type === "Folder" && params.data.HasChildren; },
                "is-project": function (params) { return params.data.Type === "Project"; }
            },
            cellRenderer: "group",
            cellRendererParams: {
                innerRenderer: (params) => {
                    return params.data.Name;
                }
            }
        }];

        controller.$onInit();

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
                Name: "artifact"
            }
        };
        var paramsMockFolder = {
            data: {
                Type: "Folder",
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

    //it("setDataSource",  inject(($q: ng.IQService) => {
    //    // Arrange
    //    var dataFromCall;
    //    gridApi.setRowData = function (data) {
    //        dataFromCall = data;
    //    };

    //    // Act
    //    var options = controller.options;

    //    controller.options.api = gridApi;
    //    $scope.$apply();

    //    controller.setDataSource([{
    //        id: 1, Name: `Name 1`
    //    }]);
    //    controller.setDataSource([{
    //        id: 2, Name: `Name 2`
    //    }]);

    //    // Assert
    //    let data = controller.options.api.getRenderedNodes();
    //    expect(dataFromCall.Type).toBeTruthy;
    //    expect(dataFromCall.Name).toBeTruthy;
    //}));


    //it("rowGroupOpened", inject(($q: ng.IQService) => {
    //    // Arrange
    //    var dataFromCall;
    //    var paramsMock = {
    //        node: {
    //            data: {
    //                Id: 77,
    //                Type: "Folder",
    //                Name: "folder",
    //                Children: [],
    //            },
    //            expanded: true
    //        }
    //    };
    //    controller.onExpand = function () {
    //        var deferred = $q.defer();
    //        deferred.resolve([{
    //            Id: 88,
    //            Type: "Project",
    //            Name: "Project",
    //        }]);
    //        return deferred.promise;
    //    };

    //    var setRowDataMock = function (data) {
    //        dataFromCall = data;
    //    };
    //    gridApi.setRowData = setRowDataMock;

    //    // Act
    //    var options = controller.options;
    //    controller.options.api = gridApi;
    //    options.onRowGroupOpened(paramsMock);
    //    expect(paramsMock.node.data["open"]).toBeUndefined;
    //    expect(paramsMock.node.data["Loaded"]).toBeUndefined;

    //    $scope.$apply();

    //    // Assert
    //    expect(paramsMock.node.data.Children).toEqual(jasmine.any(Array));
    //    expect(paramsMock.node.data.Children.length).toEqual(1);
    //    expect(paramsMock.node.data.Children[0].Type).toBe("Project");
    //    expect(paramsMock.node.data["open"]).toBeTruthy;
    //    expect(paramsMock.node.data["Loaded"]).toBeTruthy;
    //}));

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