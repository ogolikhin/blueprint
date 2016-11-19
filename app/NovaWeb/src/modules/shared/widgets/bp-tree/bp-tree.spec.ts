import * as angular from "angular";
import "angular-mocks";
import {BPTreeController, IColumn, IColumnRendererParams} from "./bp-tree";
import {GridApi} from "ag-grid/main";
import {IArtifactNode} from "../../../managers/project-manager";

function toFlat(root: any): any[] {
    let stack: any[] = angular.isArray(root) ? root.slice() : [root], array: any[] = [];
    while (stack.length !== 0) {
        let node = stack.shift();
        array.push(node);
        if (angular.isArray(node.children)) {

            for (let i = node.children.length - 1; i >= 0; i--) {
                stack.push(node.children[i]);
            }
            node.children = null;
        }
    }

    return array;
}

describe("Embedded ag-grid events", () => {
    let controller: BPTreeController;
    let $scope, elem;
    const gridApi = new GridApi();
    gridApi.setFocusedCell = () => {
        return;
    };

    beforeEach(inject(function (_$q_, _$rootScope_, _$compile_, $timeout) {
        $scope = _$rootScope_.$new();

        elem = angular.element(`<div ag-grid="$ctrl.gridOptions" class="ag-grid"></div>`);

        controller = new BPTreeController($scope, $timeout);
        _$compile_(elem)($scope);
        //act
        controller.columns = [{
            headerName: "Header",
            field: "name",
            cellClass: (vm: IArtifactNode) => {
                const result: string[] = [];

                if (vm.group) {
                    result.push("has-children");
                }
                return result;
            },
            isGroup: true,
            innerRenderer: (params: IColumnRendererParams) => {
                return params.data.model.name;
            }
        } as IColumn];

        controller.$onInit();

        $scope.$digest();
    }));

    it("getNodeChildDetails", () => {
        // Arrange
        const rowItemMock = {
            group: true,
            expanded: true,
            key: "1"
        };

        const rowItemMockNoChildren = {};

        // Act
        const options = controller.options;
        const node = options.getNodeChildDetails(rowItemMock);
        const nodeNoChildren = options.getNodeChildDetails(rowItemMockNoChildren);

        // Assert
        expect(node.key).toEqual("1");
        expect(node.expanded).toBeTruthy();
        expect(nodeNoChildren).toBeNull();
    });

    xit("innerRenderer", () => {
        // // Arrange
        // const paramsMock = {
        //     data: {
        //         name: "artifact"
        //     }
        // };
        // const paramsMockFolder = {
        //     data: {
        //         type: "Folder"
        //     }
        // };

        // // Act
        // const options = controller.options;
        // const cellRenderer = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMock);
        // const cellRendererFolder = options.columnDefs[0].cellRendererParams.innerRenderer(paramsMockFolder);

        // // Assert
        // expect(cellRenderer).toEqual("artifact");
        // expect(cellRendererFolder).toEqual(undefined);
    });

    xit("add nodes", inject(($q: ng.IQService) => {
        // // Arrange
        // let dataFromCall;
        // gridApi.setRowData = function (data) {
        //     dataFromCall = data;
        // };

        // // Act
        // controller.options.api = gridApi;
        // $scope.$apply();

        // controller.api.reload([
        //     {id: 1, Name: `Name 1`},
        //     {id: 2, Name: `Name 2`}
        // ]);
        // // Assert
        // //let data = controller.options.api.getRenderedNodes();
        // expect(dataFromCall).toEqual(jasmine.any(Array));
        // expect(dataFromCall.length).toBe(2);
    }));
    xit("add children to node", inject(($q: ng.IQService) => {
        // // Arrange
        // let dataFromCall;
        // gridApi.setRowData = function (data) {
        //     dataFromCall = data;
        // };

        // // Act
        // controller.options.api = gridApi;
        // $scope.$apply();

        // controller.api.reload([
        //     {id: 1, Name: `Name 1`},
        //     {id: 2, Name: `Name 2`}
        // ]);
        // controller.api.reload([
        //     {id: 3, Name: `Name 3`},
        //     {id: 4, Name: `Name 4`}
        // ], 1);

        // // Assert
        // //let data = controller.options.api.getRenderedNodes();
        // expect(dataFromCall).toEqual(jasmine.any(Array));
        // expect(dataFromCall.length).toBe(2);
        // expect(toFlat(dataFromCall).length).toBe(4);
        // expect(dataFromCall[0].open).toBeTruthy;
        // expect(dataFromCall[0].loaded).toBeTruthy;

    }));


});
