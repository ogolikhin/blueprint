import * as angular from "angular";
import "angular-mocks";
import * as agGrid from "ag-grid/main";
import { BPTreeViewComponent, BPTreeViewController, ITreeViewNodeVM, IColumn } from "./bp-tree-view";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";

describe("BPTreeViewComponent", () => {
    angular.module("bp.widgets.treeView", [])
        .component("bpTreeView", new BPTreeViewComponent());

    beforeEach(angular.mock.module("bp.widgets.treeView", ($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("Values are bound", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        const element = `<bp-tree-view grid-class="class"
                                       selection-mode="'multiple'"
                                       row-buffer="0"
                                       row-height="20"
                                       root-node="{key: 'root'}"
                                       root-node-visible="true"
                                       columns="[{field: 'key'}]"
                                       header-height="20"
                                       on-select="onSelect()" />`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("bpTreeView") as BPTreeViewController;
        controller.options.api = jasmine.createSpyObj("api", ["setColumnDefs", "setRowData"]);

        // Assert
        expect(controller.gridClass).toEqual("class");
        expect(controller.selectionMode).toEqual("multiple");
        expect(controller.rowBuffer).toEqual(0);
        expect(controller.rowHeight).toEqual(20);
        expect(controller.rootNode).toEqual({key: "root"});
        expect(controller.rootNodeVisible).toEqual(true);
        expect(controller.columns).toEqual([{field: "key"}]);
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
        expect(controller.selectionMode).toEqual("single");
        expect(controller.rowBuffer).toEqual(200);
        expect(controller.rowHeight).toEqual(24);
        expect(controller.rootNode).toBeUndefined();
        expect(controller.rootNodeVisible).toEqual(false);
        expect(controller.columns).toEqual([]);
        expect(controller.headerHeight).toEqual(0);
        expect(controller.onSelect).toBeUndefined();
        expect(controller.options).toEqual(jasmine.objectContaining({
            suppressRowClickSelection: true,
            rowBuffer: controller.rowBuffer,
            enableColResize: true,
            icons: {
                groupExpanded: "<i />",
                groupContracted: "<i />",
                checkboxChecked: `<i class="ag-checkbox-checked" />`,
                checkboxUnchecked: `<i class="ag-checkbox-unchecked" />`,
                checkboxIndeterminate: `<i class="ag-checkbox-indeterminate" />`
            },
            angularCompileRows: true,
            suppressContextMenu: true,
            rowHeight: controller.rowHeight,
            showToolPanel: false,
            headerHeight: controller.headerHeight,
            getBusinessKeyForNode: controller.getBusinessKeyForNode,
            getNodeChildDetails: controller.getNodeChildDetails,
            onRowGroupOpened: controller.onRowGroupOpened,
            onViewportChanged: controller.onViewportChanged,
            onCellClicked: controller.onCellClicked,
            onRowSelected: controller.onRowSelected,
            onGridReady: controller.onGridReady,
            onModelUpdated: controller.onModelUpdated
        }));
    }));
});

describe("BPTreeViewController", () => {
    let controller: BPTreeViewController;

    beforeEach(inject(($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
        const element = angular.element(`<bp-tree-view />`);
        controller = new BPTreeViewController($q, element, new LocalizationServiceMock($rootScope));
        controller.options = {api: jasmine.createSpyObj("api", [
            "setColumnDefs",
            "showLoadingOverlay",
            "getSelectedRows",
            "setRowData",
            "sizeColumnsToFit",
            "forEachNode",
            "getModel",
            "hideOverlay",
            "showNoRowsOverlay"
        ])};
    }));

    describe("Component lifecylcle methods", () => {
        it("$onChanges, when selectionMode changes, calls resetGridAsync correctly", () => {
            // Arrange
            spyOn(controller, "resetGridAsync");

            // Act
            controller.$onChanges({selectionMode: {} as ng.IChangesObject<any>} as ng.IOnChangesObject);

            // Assert
            expect(controller.resetGridAsync).toHaveBeenCalledWith(false);
        });

        it("$onChanges, when rootNode changes, calls resetGridAsync correctly", () => {
            // Arrange
            spyOn(controller, "resetGridAsync");

            // Act
            controller.$onChanges({rootNode: {} as ng.IChangesObject<any>} as ng.IOnChangesObject);

            // Assert
            expect(controller.resetGridAsync).toHaveBeenCalledWith(false);
        });

        it("$onChanges, when rootNodeVisible changes, calls resetGridAsync correctly", () => {
            // Arrange
            spyOn(controller, "resetGridAsync");

            // Act
            controller.$onChanges({rootNodeVisible: {} as ng.IChangesObject<any>} as ng.IOnChangesObject);

            // Assert
            expect(controller.resetGridAsync).toHaveBeenCalledWith(false);
        });

        it("$onChanges, when columns changes, calls resetGridAsync correctly", () => {
            // Arrange
            spyOn(controller, "resetGridAsync");

            // Act
            controller.$onChanges({columns: {} as ng.IChangesObject<any>} as ng.IOnChangesObject);

            // Assert
            expect(controller.resetGridAsync).toHaveBeenCalledWith(false);
        });

        it("$onDestroy calls setRowData and updateScrollbars", () => {
            // Arrange
            spyOn(controller, "updateScrollbars");

            // Act
            controller.$onDestroy();

            // Assert
            expect(controller.options.api.setRowData).toHaveBeenCalledWith(null);
            expect(controller.updateScrollbars).toHaveBeenCalledWith(true);
        });
    });

    describe("resetGridAsync", () => {
        it ("When selection mode is single, sets rowSelection, rowDeselection and checkbox correctly", () => {
            // Arrange
            (controller.options.api.setColumnDefs as jasmine.Spy).and.callFake(columnDefs => controller.options.columnDefs = columnDefs);
            controller.selectionMode = "single";
            controller.columns = [{isGroup: true}];

            // Act
            controller.resetGridAsync(false);

            // Assert
            expect(controller.options.rowSelection).toEqual("single");
            expect(controller.options.rowDeselection).toEqual(false);
            expect(controller.options.columnDefs[0].cellRendererParams["checkbox"]).toBeUndefined();
        });

        it ("When selection mode is multiple, sets rowSelection, rowDeselection and checkbox correctly", () => {
            // Arrange
            (controller.options.api.setColumnDefs as jasmine.Spy).and.callFake(columnDefs => controller.options.columnDefs = columnDefs);
            controller.selectionMode = "multiple";
            controller.columns = [{isGroup: true}];

            // Act
            controller.resetGridAsync(false);

            // Assert
            expect(controller.options.rowSelection).toEqual("multiple");
            expect(controller.options.rowDeselection).toEqual(true);
            expect(controller.options.columnDefs[0].cellRendererParams["checkbox"]).toBeUndefined();
        });

        it ("When selection mode is checkbox, sets rowSelection, rowDeselection and checkbox correctly", () => {
            // Arrange
            (controller.options.api.setColumnDefs as jasmine.Spy).and.callFake(columnDefs => controller.options.columnDefs = columnDefs);
            controller.selectionMode = "checkbox";
            controller.columns = [{isGroup: true}];

            // Act
            controller.resetGridAsync(false);

            // Assert
            expect(controller.options.rowSelection).toEqual("multiple");
            expect(controller.options.rowDeselection).toEqual(true);
            expect(angular.isFunction(controller.options.columnDefs[0].cellRendererParams["checkbox"])).toEqual(true);
        });

        it ("When columns change, sets column defs correctly", () => {
            // Arrange
            (controller.options.api.setColumnDefs as jasmine.Spy).and.callFake(columnDefs => controller.options.columnDefs = columnDefs);
            controller.columns = [{
                headerName: "header",
                field: "field",
                isGroup: true,
                cellClass: () => [],
                innerRenderer: () => ""
            }] as IColumn[];

            // Act
            controller.resetGridAsync(false);

            // Assert
            expect(controller.options.columnDefs).toEqual([jasmine.objectContaining({
                headerName: "header",
                field: "field",
                cellRenderer: "group",
                cellRendererParams: jasmine.objectContaining({
                    padding: 20
                }),
                suppressMenu: true,
                suppressSorting: true,
            })]);
            expect(angular.isFunction(controller.options.columnDefs[0].cellClass)).toEqual(true);
            expect(angular.isFunction(controller.options.columnDefs[0].cellRendererParams.innerRenderer)).toEqual(true);
        });

        it("When root node is visible, sets row data correctly", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            (controller.options.api.getModel as jasmine.Spy).and.returnValue({ getRowCount() { return 1; }});
            controller.rootNode = {} as ITreeViewNodeVM;
            controller.rootNodeVisible = true;

            // Act
            controller.resetGridAsync(false).then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([]);
                expect(controller.options.api.showLoadingOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([controller.rootNode]);
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                expect(controller.options.api.hideOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.showNoRowsOverlay).not.toHaveBeenCalled();
                done();
            });
            $rootScope.$digest(); // Resolves promises
        }));

        it("When root node loads asynchronously, sets row data correctly", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            (controller.options.api.getModel as jasmine.Spy).and.returnValue({ getRowCount() { return 1; }});
            const children = [{key: "child"}] as ITreeViewNodeVM[];
            controller.rootNode = {
                key: "root",
                isExpandable: true,
                children: [],
                isExpanded: false,
                isSelectable() { return true; },
                loadChildrenAsync() { this.children = children; return $q.resolve(); }
            } as ITreeViewNodeVM;

            // Act
            controller.resetGridAsync(false).then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([]);
                expect(controller.options.api.showLoadingOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.setRowData).toHaveBeenCalledWith(children);
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                expect(controller.options.api.hideOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.showNoRowsOverlay).not.toHaveBeenCalled();
                done();
            });
            $rootScope.$digest(); // Resolves promises
        }));

        it("When root node asynchronous load fails, calls onError correctly", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            controller.onError = jasmine.createSpy("onError");
            (controller.options.api.getModel as jasmine.Spy).and.returnValue({ getRowCount() { return 1; }});
            const reason = "reason";
            controller.rootNode = {
                key: "root",
                isExpandable: true,
                children: [],
                isExpanded: false,
                isSelectable() { return true; },
                loadChildrenAsync() { return $q.reject(reason); }
            } as ITreeViewNodeVM;

            // Act
            controller.resetGridAsync(false).then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([]);
                expect(controller.options.api.showLoadingOverlay).toHaveBeenCalledWith();
                expect(controller.onError).toHaveBeenCalledWith({reason: reason});
                expect(controller.options.api.hideOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.showNoRowsOverlay).not.toHaveBeenCalled();
                done();
            });
            $rootScope.$digest(); // Resolves promises
        }));

        it("When root node has children, sets row data correctly", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            (controller.options.api.getModel as jasmine.Spy).and.returnValue({ getRowCount() { return 1; }});
            controller.rootNode = {
                children: [{key: "child"}] as ITreeViewNodeVM[]
            } as ITreeViewNodeVM;

            // Act
            controller.resetGridAsync(false).then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([]);
                expect(controller.options.api.showLoadingOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.setRowData).toHaveBeenCalledWith(controller.rootNode.children);
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                expect(controller.options.api.hideOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.showNoRowsOverlay).not.toHaveBeenCalled();
                done();
            });
            $rootScope.$digest(); // Resolves promises
        }));

        it("When root node is undefined, sets row data correctly", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            (controller.options.api.getModel as jasmine.Spy).and.returnValue({ getRowCount() { return 0; }});

            // Act
            controller.resetGridAsync(false).then(() => {

                // Assert
                expect(controller.options.api.setRowData).toHaveBeenCalledWith([]);
                expect(controller.options.api.showLoadingOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.sizeColumnsToFit).toHaveBeenCalled();
                expect(controller.options.api.hideOverlay).toHaveBeenCalledWith();
                expect(controller.options.api.showNoRowsOverlay).toHaveBeenCalledWith();
                done();
            });
            $rootScope.$digest(); // Resolves promises
        }));

        it("When saveSelection is true, restores selection", (done: DoneFn) => inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            const rows = [{key: "a"}, {key: "b"}, {key: "c"}];
            const nodes = rows.map(row => {
                const node = jasmine.createSpyObj("row", ["setSelected"]) as agGrid.RowNode;
                node.data = row;
                return node;
            });
            (controller.options.api.getSelectedRows as jasmine.Spy).and.returnValue(rows);
            (controller.options.api.forEachNode as jasmine.Spy).and.callFake(callback => nodes.forEach(callback));
            (controller.options.api.getModel as jasmine.Spy).and.returnValue({ getRowCount() { return rows.length; }});

            // Act
            controller.resetGridAsync(true).then(() => {

                // Assert
                nodes.forEach(node => expect(node.setSelected).toHaveBeenCalledWith(true));
                done();
            });
            $rootScope.$digest(); // Resolves promises
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
        it("onRowGroupOpened, when expandable, sets isExpanded", () => {
            // Arrange
            const vm = { isExpandable: true, isExpanded: false } as ITreeViewNodeVM;
            const node = { data: vm, expanded: true } as agGrid.RowNode;

            // Act
            controller.onRowGroupOpened({node: node});

            // Assert
            expect(vm.isExpanded).toEqual(true);
        });

        it("onRowGroupOpened, when loads asynchronously, calls resetGridAsync correctly", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            const vm = jasmine.createSpyObj("vm", ["loadChildrenAsync"]) as ITreeViewNodeVM;
            (vm.loadChildrenAsync as jasmine.Spy).and.returnValue($q.resolve());
            vm.isExpandable = true;
            vm.isExpanded = false;
            const node = { data: vm, expanded: true } as agGrid.RowNode;
            spyOn(controller, "resetGridAsync");

            // Act
            controller.onRowGroupOpened({node: node});

            // Assert
            expect(vm.loadChildrenAsync).toHaveBeenCalled();
            $rootScope.$digest(); // Resolves promises
            expect(controller.resetGridAsync).toHaveBeenCalledWith(true);
        }));

        it("onRowGroupOpened, when asynchronous load fails, calls onError correctly", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
            // Arrange
            controller.onError = jasmine.createSpy("onError");
            const vm = jasmine.createSpyObj("vm", ["loadChildrenAsync"]) as ITreeViewNodeVM;
            (vm.loadChildrenAsync as jasmine.Spy).and.returnValue($q.reject("reason"));
            vm.isExpandable = true;
            vm.isExpanded = false;
            const node = { data: vm, expanded: true } as agGrid.RowNode;

            // Act
            controller.onRowGroupOpened({node: node});

            // Assert
            expect(vm.loadChildrenAsync).toHaveBeenCalled();
            $rootScope.$digest(); // Resolves promises
            expect(controller.onError).toHaveBeenCalledWith({reason: "reason"});
        }));

        it("onRowGroupOpened, when not expandable, does not set isExpanded", () => {
            // Arrange
            const vm = { isExpandable: false, isExpanded: false } as ITreeViewNodeVM;
            const node = { data: vm, expanded: true } as agGrid.RowNode;

            // Act
            controller.onRowGroupOpened({node: node});

            // Assert
            expect(vm.isExpanded).toEqual(false);
        });

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

        it("onCellClicked, when event target is outside the cell value div, does not call setSelectedParams", () => {
            // Arrange
            controller.options.rowDeselection = true;
            const event = {ctrlKey: true, metaKey: false, shiftKey: false, target: undefined} as MouseEvent;
            const node = jasmine.createSpyObj("node", ["setSelectedParams"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};

            // Act
            controller.onCellClicked({event: event, node: node});

            // Assert
            expect(node.setSelectedParams).not.toHaveBeenCalled();
        });

        it("onCellClicked, when selected node Ctrl+clicked, calls setSelectedParams correctly", () => {
            // Arrange
            controller.options.rowDeselection = true;
            const target = angular.element(`<div class="ag-group-value">`)[0] as EventTarget;
            const event = {ctrlKey: true, metaKey: false, shiftKey: false, target: target} as MouseEvent;
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelectedParams"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(true);

            // Act
            controller.onCellClicked({event: event, node: node});

            // Assert
            expect(node.setSelectedParams).toHaveBeenCalledWith({newValue: false});
        });

        it("onCellClicked, when selected node clicked, calls setSelectedParams correctly", () => {
            // Arrange
            const target = angular.element(`<div class="ag-group-value">`)[0] as EventTarget;
            const event = {ctrlKey: false, metaKey: false, shiftKey: false, target: target} as MouseEvent;
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelectedParams"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(true);

            // Act
            controller.onCellClicked({event: event, node: node});

            // Assert
            expect(node.setSelectedParams).toHaveBeenCalledWith({newValue: true, clearSelection: true});
        });

        it("onCellClicked, when selected node clicked in checkbox mode, calls setSelectedParams correctly", () => {
            // Arrange
            controller.selectionMode = "checkbox";
            controller.options.rowDeselection = true;
            const target = angular.element(`<div class="ag-group-value">`)[0] as EventTarget;
            const event = {ctrlKey: false, metaKey: false, shiftKey: false, target: target} as MouseEvent;
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelectedParams"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(true);

            // Act
            controller.onCellClicked({event: event, node: node});

            // Assert
            expect(node.setSelectedParams).toHaveBeenCalledWith({newValue: false});
        });

        it("onCellClicked, when unselected node clicked, calls setSelectedParams correctly", () => {
            // Arrange
            const target = angular.element(`<div class="ag-group-value">`)[0] as EventTarget;
            const event = {ctrlKey: false, metaKey: false, shiftKey: true, target: target} as MouseEvent;
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelectedParams"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(false);
            node.data = {isSelectable() { return true; }};

            // Act
            controller.onCellClicked({event: event, node: node});

            // Assert
            expect(node.setSelectedParams).toHaveBeenCalledWith({newValue: true, clearSelection: true, rangeSelect: true});
        });

        it("onCellClicked, when unselected node clicked in checkbox mode, calls setSelectedParams correctly", () => {
            // Arrange
            controller.selectionMode = "checkbox";
            const target = angular.element(`<div class="ag-group-value">`)[0] as EventTarget;
            const event = {ctrlKey: false, metaKey: false, shiftKey: true, target: target} as MouseEvent;
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelectedParams"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(false);
            node.data = {isSelectable() { return true; }};

            // Act
            controller.onCellClicked({event: event, node: node});

            // Assert
            expect(node.setSelectedParams).toHaveBeenCalledWith({newValue: true, clearSelection: false, rangeSelect: true});
        });

        it("onCellClicked, when unselected and unselectable node clicked, does not calls setSelectedParams", () => {
            // Arrange
            controller.selectionMode = "checkbox";
            const target = angular.element(`<div class="ag-group-value">`)[0] as EventTarget;
            const event = {ctrlKey: false, metaKey: false, shiftKey: true, target: target} as MouseEvent;
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelectedParams"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(false);
            node.data = {isSelectable() { return false; }};

            // Act
            controller.onCellClicked({event: event, node: node});

            // Assert
            expect(node.setSelectedParams).not.toHaveBeenCalled();
        });

        it("onRowSelected, when selected and selectable, calls onSelect correctly", () => {
            // Arrange
            controller.onSelect = jasmine.createSpy("onSelect");
            const vm = {isSelectable() { return true; }} as ITreeViewNodeVM;
            const node = {data: vm, isSelected: () => true} as agGrid.RowNode;
            (controller.options.api.getSelectedRows as jasmine.Spy).and.returnValue([node.data]);

            // Act
            controller.onRowSelected({node: node});

            // Assert
            expect(controller.onSelect).toHaveBeenCalledWith({vm: vm, isSelected: true, selectedVMs: [vm]});
        });

        it("onRowSelected, when selected and not selectable, deselects", () => {
            // Arrange
            controller.onSelect = jasmine.createSpy("onSelect");
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelected"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(true);
            node.data = {isSelectable() { return false; }};

            // Act
            controller.onRowSelected({node: node});

            // Assert
            expect(node.setSelected).toHaveBeenCalledWith(false);
            expect(controller.onSelect).not.toHaveBeenCalled();
        });

        it("onRowSelected, when selected and ancestor not expanded, deselects", () => {
            // Arrange
            controller.onSelect = jasmine.createSpy("onSelect");
            const node = jasmine.createSpyObj("node", ["isSelected", "setSelected"]) as agGrid.RowNode & {setSelectedParams: jasmine.Spy};
            (node.isSelected as jasmine.Spy).and.returnValue(true);
            node.data = {isSelectable() { return true; }};
            node.parent = { expanded: true, parent: { expanded: false} } as agGrid.RowNode;

            // Act
            controller.onRowSelected({node: node});

            // Assert
            expect(node.setSelected).toHaveBeenCalledWith(false);
            expect(controller.onSelect).not.toHaveBeenCalled();
        });

        it("onRowSelected, when not selected, calls onSelect correctly", () => {
            // Arrange
            controller.onSelect = jasmine.createSpy("onSelect");
            const vm = {} as ITreeViewNodeVM;
            const node = {data: vm, isSelected: () => false} as agGrid.RowNode;
            (controller.options.api.getSelectedRows as jasmine.Spy).and.returnValue([node.data]);

            // Act
            controller.onRowSelected({node: node});

            // Assert
            expect(controller.onSelect).toHaveBeenCalledWith({vm: vm, isSelected: false, selectedVMs: [vm]});
        });

        it("onRowDoubleClicked, when not selected, calls onDoubleClick correctly", () => {
            // Arrange
            controller.onDoubleClick = jasmine.createSpy("onDoubleClick");
            const vm = {} as ITreeViewNodeVM;

            // Act
            controller.onRowDoubleClicked({data: vm});

            // Assert
            expect(controller.onDoubleClick).toHaveBeenCalledWith({vm: vm});
        });

        it("onGridReady calls resetGridAsync correctly", () => {
            // Arrange
            spyOn(controller, "resetGridAsync");

            // Act
            controller.onGridReady();

            // Assert
            expect(controller.resetGridAsync).toHaveBeenCalledWith(false);
        });
    });
});
