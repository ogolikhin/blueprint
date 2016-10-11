import * as angular from "angular";
import * as agGrid from "ag-grid/main";
import {ILocalizationService} from "../../../core";

/**
 * Usage:
 *
 * <bp-tree-view grid-class="project-tree"
 *               row-buffer="200"
 *               selection-mode="'single'"
 *               row-height="20"
 *               root-node="$ctrl.rootNode"
 *               root-node-visible="false"
 *               columns="$ctrl.columns"
 *               header-height="20"
 *               on-select="$ctrl.onSelect(vm, isSelected, selectedVMs)"
 *               on-double-click="$ctrl.onDoubleClick(vm)"
 *               on-error="$ctrl.onError(reason)">
 * </bp-tree-view>
 */
export class BPTreeViewComponent implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPTreeViewController;
    public template: string = require("./bp-tree-view.html");
    public bindings: {[binding: string]: string} = {
        gridClass: "@",
        rowBuffer: "<",
        selectionMode: "<",
        rowHeight: "<",
        rootNode: "<",
        rootNodeVisible: "<",
        columns: "<",
        headerHeight: "<",
        onSelect: "&?",
        onDoubleClick: "&?",
        onError: "&?"
    };
}

export interface IBPTreeViewController extends ng.IComponentController {
    // Template bindings
    gridClass: string;
    options: agGrid.GridOptions;

    // Grid options
    rowBuffer: number;
    selectionMode: "single" | "multiple" | "checkbox";
    rowHeight: number;
    rootNode: ITreeViewNodeVM;
    rootNodeVisible: boolean;
    columns: IColumn[];
    headerHeight: number;
    onSelect: (param: {vm: ITreeViewNodeVM, isSelected: boolean, selectedVMs: ITreeViewNodeVM[]}) => void;
    onDoubleClick: (param: {vm: ITreeViewNodeVM}) => void;
    onError: (param: {reason: any}) => void;
}

export interface ITreeViewNodeVM {
    key: string; // Each row in the dom will have an attribute row-id='key'
    isExpandable: boolean;
    children: ITreeViewNodeVM[];
    isExpanded: boolean;
    isSelectable(): boolean;
    loadChildrenAsync?(): ng.IPromise<any>; // To lazy-load children
}

export interface IColumn {
    headerName?: string;
    field?: string;
    isGroup?: boolean;
    cellClass?: (vm: ITreeViewNodeVM) => string[];
    innerRenderer?: (vm: ITreeViewNodeVM, eGridCell: HTMLElement) => string;
}

export class BPTreeViewController implements IBPTreeViewController {
    public static $inject = ["$q", "$element", "localization"];

    // Template bindings
    public gridClass: string;
    public options: agGrid.GridOptions;

    // Grid options
    public rowBuffer: number;
    public selectionMode: "single" | "multiple" | "checkbox";
    public rowHeight: number;
    public rootNode: ITreeViewNodeVM;
    public rootNodeVisible: boolean;
    public columns: IColumn[];
    public headerHeight: number;
    public onSelect: (param: {vm: ITreeViewNodeVM, isSelected: boolean, selectedVMs: ITreeViewNodeVM[]}) => void;
    public onDoubleClick: (param: {vm: ITreeViewNodeVM}) => void;
    public onError: (param: {reason: any}) => void;

    constructor(private $q: ng.IQService, private $element: ng.IAugmentedJQuery, private localization: ILocalizationService) {
        this.gridClass = angular.isDefined(this.gridClass) ? this.gridClass : "project-explorer";
        this.rowBuffer = angular.isDefined(this.rowBuffer) ? this.rowBuffer : 200;
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.rowHeight = angular.isDefined(this.rowHeight) ? this.rowHeight : 24;
        this.rootNodeVisible = angular.isDefined(this.rootNodeVisible) ? this.rootNodeVisible : false;
        this.columns = angular.isDefined(this.columns) ? this.columns : [];
        this.headerHeight = angular.isDefined(this.headerHeight) ? this.headerHeight : 0;

        this.options = {
            suppressRowClickSelection: true,
            rowBuffer: this.rowBuffer,
            enableColResize: true,
            icons: {
                groupExpanded: "<i />",
                groupContracted: "<i />",
                checkboxChecked: `<i class="ag-checkbox-checked" />`,
                checkboxUnchecked: `<i class="ag-checkbox-unchecked" />`,
                checkboxIndeterminate: `<i class="ag-checkbox-indeterminate" />`
            },
            angularCompileRows: true, // this is needed to compile directives (dynamically added) on the rows
            suppressContextMenu: true,
            localeTextFunc: (key: string, defaultValue: string) => this.localization.get("ag-Grid_" + key, defaultValue),
            rowSelection: this.selectionMode === "single" ? "single" : "multiple",
            rowDeselection: this.selectionMode !== "single",
            rowHeight: this.rowHeight,
            rowData: [],
            showToolPanel: false,
            columnDefs: [],
            headerHeight: this.headerHeight,

            // Callbacks
            getBusinessKeyForNode: this.getBusinessKeyForNode,
            getNodeChildDetails: this.getNodeChildDetails,

            // Event handlers
            onRowGroupOpened: this.onRowGroupOpened,
            onViewportChanged: this.onViewportChanged,
            onCellClicked: this.onCellClicked,
            onRowSelected: this.onRowSelected,
            onRowDoubleClicked: this.onRowDoubleClicked,
            onGridReady: this.onGridReady,
            onModelUpdated: this.onModelUpdated
        };
    }

    public $onChanges(onChangesObj: ng.IOnChangesObject): void {
        if (onChangesObj["selectionMode"] || onChangesObj["rootNode"] || onChangesObj["rootNodeVisible"] || onChangesObj["columns"]) {
            this.resetGridAsync(false);
        }
    }

    public $onDestroy(): void {
        this.options.api.setRowData(null);
        this.updateScrollbars(true);
    }

    public resetGridAsync(saveSelection: boolean): ng.IPromise<void> {
        if (this.options.api) {
            this.options.rowSelection = this.selectionMode === "single" ? "single" : "multiple";
            this.options.rowDeselection = this.selectionMode !== "single";
            this.options.api.setColumnDefs(this.columns.map(column => {
                return {
                    headerName: column.headerName ? column.headerName : "",
                    field: column.field,
                    cellClass: column.cellClass ? (params: agGrid.RowNode) => column.cellClass(params.data as ITreeViewNodeVM) : undefined,
                    cellRenderer: column.isGroup ? "group" : undefined,
                    cellRendererParams: column.isGroup ? {
                        checkbox: this.selectionMode === "checkbox",
                        innerRenderer: column.innerRenderer ?
                            (params: any) => column.innerRenderer(params.data as ITreeViewNodeVM, params.eGridCell as HTMLElement) : undefined,
                        padding: 20
                    } : undefined,
                    suppressMenu: true,
                    suppressSorting: true,
                } as agGrid.ColDef;
            }));

            let rowDataAsync: ITreeViewNodeVM[] | ng.IPromise<ITreeViewNodeVM[]>;
            if (this.rootNode) {
                if (this.rootNodeVisible) {
                    rowDataAsync = [this.rootNode];
                } else if (angular.isFunction(this.rootNode.loadChildrenAsync)) {
                    rowDataAsync = this.rootNode.loadChildrenAsync().then(() => this.rootNode.children);
                } else {
                    rowDataAsync = this.rootNode.children;
                }
            } else {
                rowDataAsync = [];
            }

            const selectedVMs: {[key: string]: ITreeViewNodeVM} = {};
            if (saveSelection) {
                this.options.api.getSelectedRows().forEach((row: ITreeViewNodeVM) => selectedVMs[row.key] = row);
            } else {
                this.options.api.setRowData([]);
                this.options.api.showLoadingOverlay();
            }

            return this.$q.when(rowDataAsync).then((rowData) => {
                if (this.options.api) {
                    this.options.api.setRowData(rowData);
                    this.options.api.sizeColumnsToFit();

                    if (saveSelection) {

                        // Restore selection
                        this.options.api.forEachNode(node => {
                            if (selectedVMs[node.data.key]) {
                                node.setSelected(true);
                            }
                        });
                    }
                }
            }).catch(reason => {
                if (angular.isFunction(this.onError)) {
                    this.onError({reason: reason});
                }
            }).finally(() => {
                if (this.options.api) {
                    this.options.api.hideOverlay();
                    if (this.options.api.getModel().getRowCount() === 0) {
                        this.options.api.showNoRowsOverlay();
                    }
                }
            });
        }

        return this.$q.resolve();
    }

    public updateScrollbars(destroy: boolean = false) {
        const viewport = this.$element[0].querySelector(".ag-body-viewport");
        const perfectScrollBar = (<any>window).PerfectScrollbar;

        if (viewport && angular.isDefined(perfectScrollBar)) {
            if (destroy) {
                perfectScrollBar.destroy(viewport);
            } else {
                if (viewport.getAttribute("data-ps-id")) {
                    // perfect-scrollbar has been initialized on the element (data-ps-id is not falsy)
                    const allColumnIds = [];
                    this.options.columnDefs.forEach(function (columnDef) {
                        allColumnIds.push(columnDef.field);
                    });
                    this.options.columnApi.autoSizeColumns(allColumnIds);
                    perfectScrollBar.update(viewport);
                } else {
                    perfectScrollBar.initialize(viewport, {
                        minScrollbarLength: 20,
                        scrollXMarginOffset: 4,
                        scrollYMarginOffset: 4
                    });
                }
            }
        }
    };

    // Callbacks

    public getNodeChildDetails(dataItem: any): agGrid.NodeChildDetails {
        const vm = dataItem as ITreeViewNodeVM;
        if (vm.isExpandable) {
            return {
                group: true,
                children: vm.children,
                expanded: vm.isExpanded,
                key: vm.key
            } as agGrid.NodeChildDetails;
        }
        return null;
    }

    public getBusinessKeyForNode(node: agGrid.RowNode): string {
        const vm = node.data as ITreeViewNodeVM;
        return vm.key;
    }

    // Event handlers

    public onRowGroupOpened = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const vm = node.data as ITreeViewNodeVM;

        if (vm.isExpandable) {
            const row = this.$element[0].querySelector(`.ag-body .ag-body-viewport-wrapper .ag-row[row-id="${vm.key}"]`);
            if (row) {
                row.classList.remove(node.expanded ? "ag-row-group-contracted" : "ag-row-group-expanded");
                row.classList.add(node.expanded ? "ag-row-group-expanded" : "ag-row-group-contracted");
            }
            if (node.expanded && angular.isFunction(vm.loadChildrenAsync)) {
                if (row) {
                    row.classList.add("ag-row-loading");
                }
                vm.loadChildrenAsync().then(() => this.resetGridAsync(true)).catch(reason => {
                    if (angular.isFunction(this.onError)) {
                        this.onError({reason: reason});
                    }
                });
            }
            vm.isExpanded = node.expanded;
        }
    }

    public onModelUpdated = (event?: any) => {
        this.updateScrollbars();
    }

    public onViewportChanged = (event?: any) => {
        this.updateScrollbars();
    }

    public onCellClicked = (event: {event: MouseEvent, node: agGrid.RowNode}) => {
        // Only deal with clicks in the .ag-group-value span
        let element = event.event.target as Element;
        while (!(element && element.classList.contains("ag-group-value"))) {
            if (!element || element === this.$element[0]) {
                return;
            }
            element = element.parentElement;
        }

        const node = event.node as agGrid.RowNode &
            {setSelectedParams: (params: {newValue: boolean, clearSelection?: boolean, tailingNodeInSequence?: boolean, rangeSelect?: boolean}) => void};
        const vm = node.data as ITreeViewNodeVM;

        // We set suppressRowClickSelection and handle row selection here because ag-grid's renderedRow.onRowClick()
        // does not work correctly with checkboxes and does not allow selection of group rows.
        const multiSelectKeyPressed = this.selectionMode === "checkbox" || event.event.ctrlKey || event.event.metaKey;
        const shiftKeyPressed = event.event.shiftKey;
        if (node.isSelected()) {
            if (multiSelectKeyPressed) {
                if (this.options.rowDeselection) {
                    node.setSelectedParams({newValue: false});
                }
            } else {
                node.setSelectedParams({newValue: true, clearSelection: true});
            }
        } else if (vm.isSelectable()) {
            node.setSelectedParams({
                newValue: true,
                clearSelection: !multiSelectKeyPressed,
                rangeSelect: shiftKeyPressed
            });
        }
    }

    public onRowSelected = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const isSelected = node.isSelected();
        const vm = node.data as ITreeViewNodeVM;
        if (isSelected && (!vm.isSelectable() || !this.isVisible(node))) {
            node.setSelected(false);
        } else if (this.onSelect) {
            this.onSelect({
                vm: vm,
                isSelected: isSelected,
                selectedVMs: this.options.api.getSelectedRows() as ITreeViewNodeVM[]
            });
        }
    }

    private isVisible(node: agGrid.RowNode): boolean {
        while ((node = node.parent)) {
            if (!node.expanded) {
                return false;
            }
        }
        return true;
    }

    public onRowDoubleClicked = (event: {data: ITreeViewNodeVM}) => {
        if (this.onDoubleClick) {
            this.onDoubleClick({vm: event.data});
        }
    }

    public onGridReady = (event?: any) => {
        this.resetGridAsync(false);
    }
}
