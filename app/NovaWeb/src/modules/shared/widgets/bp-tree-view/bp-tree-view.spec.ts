import "angular";
import "angular-mocks";
import * as agGrid from "ag-grid/main";
import { BPTreeViewComponent, BPTreeViewController, ITreeViewNodeVM } from "./bp-tree-view";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";

describe("BPTreeViewComponent", () => {
    angular.module("bp.widjets.treeView", [])
        .component("bpTreeView", new BPTreeViewComponent());

    beforeEach(angular.mock.module("bp.widjets.treeView", ($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("Values are bound", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        const element = `<bp-tree-view grid-class="project-tree"
                                       row-buffer="100"
                                       row-height="20"
                                       root-node="{key: 'root'}"
                                       root-node-visible="true"
                                       column-defs="[{field: 'key'}]"
                                       header-height="20"
                                       on-select="onSelect()" />`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("bpTreeView") as BPTreeViewController;
        controller.options.api = jasmine.createSpyObj("api", ["setRowData"]);

        // Assert
        expect(controller.gridClass).toEqual("project-tree");
        expect(controller.rowBuffer).toEqual(100);
        expect(controller.rowHeight).toEqual(20);
        expect(controller.rootNode).toEqual({key: "root"});
        expect(controller.rootNodeVisible).toEqual(true);
        expect(controller.columnDefs).toEqual([{field: "key"}]);
        expect(controller.headerHeight).toEqual(20);
        expect(angular.isFunction(controller.onSelect)).toEqual(true);
    }));

    it("Defaults values are applied", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        const element = `<bp-tree-view />`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("bpTreeView") as BPTreeViewController;
        controller.options.api = jasmine.createSpyObj("api", ["setRowData"]);

        // Assert
        expect(controller.gridClass).toEqual("project-explorer");
        expect(controller.rowBuffer).toEqual(200);
        expect(controller.rowHeight).toEqual(24);
        expect(controller.rootNode).toBeUndefined();
        expect(controller.rootNodeVisible).toEqual(false);
        expect(controller.columnDefs).toEqual([]);
        expect(controller.headerHeight).toEqual(0);
        expect(controller.onSelect).toBeUndefined();
    }));
});

describe("BPTreeViewController", () => {
    let controller: BPTreeViewController;

    beforeEach(inject(($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
        const element = angular.element(`<bp-tree-view />`)[0];
        controller = new BPTreeViewController($q, element, new LocalizationServiceMock($rootScope));
        controller.options = {api: jasmine.createSpyObj("api", ["getModel", "setRowData", "sizeColumnsToFit"])};
    }));

    describe("Component lifecylcle methods", () => {
        it("$onInit sets options", () => {
            // Arrange

            // Act
            controller.$onInit();

            // Assert
            expect(controller.options).toEqual(jasmine.objectContaining({
                rowBuffer: controller.rowBuffer,
                enableColResize: true,
                icons: {
                    groupExpanded: "<i />",
                    groupContracted: "<i />"
                },
                angularCompileRows: true,
                suppressContextMenu: true,
                rowSelection: "single",
                rowHeight: controller.rowHeight,
                showToolPanel: false,
                columnDefs: controller.columnDefs,
                headerHeight: controller.headerHeight,
                getBusinessKeyForNode: controller.getBusinessKeyForNode,
                getNodeChildDetails: controller.getNodeChildDetails,
                onRowGroupOpened: controller.onRowGroupOpened,
                onViewportChanged: controller.onViewportChanged,
                onCellFocused: controller.onCellFocused,
                onRowSelected: controller.onRowSelected,
                onGridReady: controller.onGridReady,
                onModelUpdated: controller.onModelUpdated
            }));
            expect(angular.isFunction(controller.options.localeTextFunc)).toBeTruthy();
        });

        it("$onChanges, when rootNode changes, calls resetRowDataAsync", () => {
            // Arrange
            spyOn(controller, "resetRowDataAsync");

            // Act
            controller.$onChanges({rootNode: {} as ng.IChangesObject} as ng.IOnChangesObject);

            // Assert
            expect(controller.resetRowDataAsync).toHaveBeenCalled();
        });

        it("$onChanges, when rootNodeVisible changes, calls resetRowDataAsync", () => {
            // Arrange
            spyOn(controller, "resetRowDataAsync");

            // Act
            controller.$onChanges({rootNodeVisible: {} as ng.IChangesObject} as ng.IOnChangesObject);

            // Assert
            expect(controller.resetRowDataAsync).toHaveBeenCalled();
        });

        it("$onChanges, when columnDefs changes, does not call resetRowDataAsync", () => {
            // Arrange
            spyOn(controller, "resetRowDataAsync");

            // Act
            controller.$onChanges({columnDefs: {} as ng.IChangesObject} as ng.IOnChangesObject);

            // Assert
            expect(controller.resetRowDataAsync).not.toHaveBeenCalled();
        });

        it("$onDestroy calls setRowData and updateScrollbars", () => {
            // Arrange
            spyOn(controller, "updateScrollbars");

            // Act
            controller.$onDestroy();

            // Assert
            expect(controller.options.api.setRowData).toHaveBeenCalledWith(null);
            expect(controller.updateScrollbars).toHaveBeenCalled();
        });
    });

    describe("resetRowDataAsync", () => {
        it("When root node visible, sets root node", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            controller.rootNode = {
                key: "root",
                isExpandable: true,
                children: [],
                isExpanded: false
            };
            controller.rootNodeVisible = true;

            // Act
            controller.resetRowDataAsync().then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([controller.rootNode]);
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                done();
            });
            $rootScope.$digest();
        }));

        it("When loading asynchronously, loads and sets children", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            const children = [{key: "child"}] as ITreeViewNodeVM[];
            controller.rootNode = {
                key: "root",
                isExpandable: true,
                children: [],
                isExpanded: false,
                loadChildrenAsync() { this.children = children; return $q.resolve(); }
            };

            // Act
            controller.resetRowDataAsync().then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith(children);
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                done();
            });
            $rootScope.$digest();
        }));

        it("When not loading asynchronously, sets children", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            controller.rootNode = {
                key: "root",
                isExpandable: true,
                children: [{key: "child"}] as ITreeViewNodeVM[],
                isExpanded: false
            };

            // Act
            controller.resetRowDataAsync().then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith(controller.rootNode.children);
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                done();
            });
            $rootScope.$digest();
        }));

        it("When no root node, sets empty row data", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange

            // Act
            controller.resetRowDataAsync().then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([]);
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                done();
            });
            $rootScope.$digest();
        }));
    });

    describe("ag-grid callbacks", () => {
        it("getNodeChildDetails, when expandable, returns vm details", () => {
            // Arrange
            const vm = {key: "key", isExpandable: true, children: [], isExpanded: false};

            // Act
            const result = controller.getNodeChildDetails(vm);

            // Assert
            expect(result).toEqual({
                group: true,
                children: vm.children,
                expanded: vm.isExpanded,
                key: vm.key
            });
        });

        it("getNodeChildDetails, when not expandable, returns null", () => {
            // Arrange
            const vm = {key: "key", isExpandable: false, children: [], isExpanded: false};

            // Act
            const result = controller.getNodeChildDetails(vm);

            // Assert
            expect(result).toBeNull();
        });

        it("getBusinessKeyForNode returns vm.key", () => {
            // Arrange
            const vm = {key: "key"} as ITreeViewNodeVM;

            // Act
            const result = controller.getBusinessKeyForNode({data: vm} as agGrid.RowNode);

            // Assert
            expect(result).toEqual(vm.key);
        });
    });

    describe("ag-grid event handlers", () => {
        it("onModelUpdated calls updateScrollbars", () => {
            // Arrange
            spyOn(controller, "updateScrollbars");

            // Act
            controller.onModelUpdated();

            // Assert
            expect(controller.updateScrollbars).toHaveBeenCalled();
        });

        it("onViewportChanged calls updateScrollbars", () => {
            // Arrange
            spyOn(controller, "updateScrollbars");

            // Act
            controller.onViewportChanged();

            // Assert
            expect(controller.updateScrollbars).toHaveBeenCalled();
        });

        it("onCellFocused calls setSelected", () => {
            // Arrange
            const model = jasmine.createSpyObj("model", ["getRow"]);
            const row = jasmine.createSpyObj("row", ["setSelected"]);
            (controller.options.api.getModel as jasmine.Spy).and.returnValue(model);
            (model.getRow as jasmine.Spy).and.returnValue(row);

            // Act
            controller.onCellFocused({rowIndex: 5});

            // Assert
            expect(model.getRow).toHaveBeenCalledWith(5);
            expect(row.setSelected).toHaveBeenCalledWith(true);
        });

        it("onRowSelected, when selected, calls onSelect", () => {
            // Arrange
            controller.onSelect = jasmine.createSpy("onSelect");
            const node = {data: {} as ITreeViewNodeVM, isSelected: () => true} as agGrid.RowNode;

            // Act
            controller.onRowSelected({node: node});

            // Assert
            expect(controller.onSelect).toHaveBeenCalledWith({vm: node.data});
        });

        it("onRowSelected, when not selected, does not call onSelect", () => {
            // Arrange
            controller.onSelect = jasmine.createSpy("onSelect");
            const node = {data: {} as ITreeViewNodeVM, isSelected: () => false} as agGrid.RowNode;

            // Act
            controller.onRowSelected({node: node});

            // Assert
            expect(controller.onSelect).not.toHaveBeenCalled();
        });

        it("onGridReady calls resetRowDataAsync", () => {
            // Arrange
            spyOn(controller, "resetRowDataAsync");

            // Act
            controller.onGridReady();

            // Assert
            expect(controller.resetRowDataAsync).toHaveBeenCalled();
        });
    });
});
