import * as _ from "lodash";
import * as agGrid from "ag-grid/main";
import {IWindowManager, IMainWindow, ResizeCause} from "../../../main/services";
import {ILocalizationService} from "../../../core/localization/localizationService";

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
 *               on-select="$ctrl.onSelect(vm, isSelected)"
 *               on-double-click="$ctrl.onDoubleClick(vm)"
 *               on-error="$ctrl.onError(reason)">
 * </bp-tree-view>
 */
export class BPTreeViewComponent implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPTreeViewController;
    public template: string = require("./bp-tree-view.html");
    public bindings: {[binding: string]: string} = {
        // Input
        gridClass: "@",
        rowBuffer: "<",
        selectionMode: "<",
        rowHeight: "<",
        rootNode: "<",
        rootNodeVisible: "<",
        columns: "<",
        headerHeight: "<",
        sizeColumnsToFit: "<",
        // Output
        onSelect: "&?",
        onDoubleClick: "&?",
        onError: "&?"
    };
}

export interface IBPTreeViewController extends ng.IComponentController {
    // BPTreeViewComponent bindings
    gridClass: string;
    rowBuffer: number;
    selectionMode: "single" | "multiple" | "checkbox";
    rowHeight: number;
    rootNode: ITreeViewNode | ITreeViewNode[];
    rootNodeVisible: boolean;
    columns: IColumn[];
    headerHeight: number;
    onSelect: (param: {vm: ITreeViewNode, isSelected: boolean}) => any;
    onDoubleClick: (param: {vm: ITreeViewNode}) => void;
    onError: (param: {reason: any}) => void;

    // ag-grid bindings
    options: agGrid.GridOptions;
}

export interface ITreeViewNode {
    key: string; // Each row in the dom will have an attribute row-id='key'
    isExpandable?: boolean;
    children?: ITreeViewNode[];
    isExpanded?: boolean;
    isSelectable(): boolean;
    loadChildrenAsync?(): ng.IPromise<any>; // To lazy-load children
}

export interface IColumn {
    headerCellRenderer?: Function;
    headerName?: string;
    field?: string;
    width?: number;
    colWidth?: number;
    minColWidth?: number;
    isGroup?: boolean;
    isCheckboxSelection?: boolean;
    isCheckboxHidden?: boolean;
    cellClass?: (vm: ITreeViewNode) => string[];
    innerRenderer?: (params: IColumnRendererParams) => string;
}

export interface IColumnRendererParams {
    data: ITreeViewNode;
    eGridCell: HTMLElement;
    $scope: ng.IScope;
}

export class BPTreeViewController implements IBPTreeViewController {
    public static $inject = ["$q", "$element", "localization", "$timeout", "windowManager"];

    // BPTreeViewComponent bindings
    public gridClass: string;
    public rowBuffer: number;
    public selectionMode: "single" | "multiple" | "checkbox";
    public rowHeight: number;
    public rootNode: ITreeViewNode | ITreeViewNode[];
    public rootNodeVisible: boolean;
    public columns: IColumn[];
    public headerHeight: number;
    public sizeColumnsToFit: boolean;
    public onSelect: (param: {vm: ITreeViewNode, isSelected: boolean}) => any;
    public onDoubleClick: (param: {vm: ITreeViewNode}) => void;
    public onError: (param: {reason: any}) => void;

    // ag-grid bindings
    public options: agGrid.GridOptions;

    private timers = [];

    constructor(private $q: ng.IQService, private $element: ng.IAugmentedJQuery, private localization: ILocalizationService,
                private $timeout: ng.ITimeoutService, private windowManager: IWindowManager) {
        this.gridClass = angular.isDefined(this.gridClass) ? this.gridClass : "project-explorer";
        this.rowBuffer = angular.isDefined(this.rowBuffer) ? this.rowBuffer : 200;
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.rowHeight = angular.isDefined(this.rowHeight) ? this.rowHeight : 24;
        this.rootNodeVisible = angular.isDefined(this.rootNodeVisible) ? this.rootNodeVisible : false;
        this.columns = angular.isDefined(this.columns) ? this.columns : [];
        this.headerHeight = angular.isDefined(this.headerHeight) ? this.headerHeight : 0;
        this.sizeColumnsToFit = angular.isDefined(this.sizeColumnsToFit) ? this.sizeColumnsToFit : false;

        this.options = {
            angularCompileHeaders: true,
            suppressRowClickSelection: true,
            rowBuffer: this.rowBuffer,
            icons: {
                groupExpanded: "<i />",
                groupContracted: "<i />",
                checkboxChecked: `<i class="ag-checkbox-checked" />`,
                checkboxUnchecked: `<i class="ag-checkbox-unchecked" />`,
                checkboxIndeterminate: `<i class="ag-checkbox-indeterminate" />`
            },
            angularCompileRows: true, // this is needed to compile directives (dynamically added) on the rows
            suppressContextMenu: true,
            suppressMenuMainPanel: true,
            suppressMenuColumnPanel: true,
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
            onModelUpdated: this.onModelUpdated,
            context: {}
        };

        this.options.context.allSelected = false;
        this.options.context.selectAllClass = new HeaderCell(this.options);
    }

    public $onInit() {
        if (this.sizeColumnsToFit) {
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this);
        }
    }

    public $onChanges(onChangesObj: ng.IOnChangesObject): void {
        if (onChangesObj["selectionMode"] || onChangesObj["rootNode"] || onChangesObj["rootNodeVisible"] || onChangesObj["columns"]) {
            this.resetGridAsync(false);
        }
    }

    private onWidthResized(mainWindow: IMainWindow) {
        if (this.options.api && this.sizeColumnsToFit) {
            if (mainWindow.causeOfChange === ResizeCause.browserResize) {
                this.options.api.sizeColumnsToFit();
            } else if (mainWindow.causeOfChange === ResizeCause.sidebarToggle) {
                this.timers[0] = this.$timeout(() => {
                    this.options.api.sizeColumnsToFit();
                }, 900);
            }
        }
    }

    public $onDestroy(): void {
        this.options.api.setRowData(null);
        _.each(this.timers, (timer) => {
            this.$timeout.cancel(timer);
        });

        this.rootNode = null;
    }

    public resetGridAsync(saveSelection: boolean): ng.IPromise<void> {
        if (this.options.api) {
            this.options.rowSelection = this.selectionMode === "single" ? "single" : "multiple";
            this.options.rowDeselection = this.selectionMode !== "single";

            this.options.api.setColumnDefs(this.columns.map(column => ({
                headerName: column.headerName ? column.headerName : "",
                field: column.field,
                width: column.width,
                cellClass: column.cellClass ? (params: agGrid.RowNode) => column.cellClass(params.data as ITreeViewNode) : undefined,
                cellRenderer: column.isGroup ? "group" : undefined,
                cellRendererParams: column.isGroup ? {
                    checkbox: this.selectionMode === "checkbox" && !column.isCheckboxHidden ?
                        (params: any) => (params.data as ITreeViewNode).isSelectable() : undefined,
                    innerRenderer: column.innerRenderer ?
                        (params: any) => column.innerRenderer(params as IColumnRendererParams) : undefined,
                    padding: 20
                } : undefined,
                checkboxSelection: column.isCheckboxSelection,
                suppressMenu: true,
                suppressSorting: true,
                headerCellRenderer: column.headerCellRenderer
            } as agGrid.ColDef)));

            let rowDataAsync: ITreeViewNode[] | ng.IPromise<ITreeViewNode[]>;
            if (this.rootNode) {
                if (this.rootNodeVisible || angular.isArray(this.rootNode)) {
                    rowDataAsync = angular.isArray(this.rootNode) ? this.rootNode : [this.rootNode];
                } else if (angular.isFunction(this.rootNode.loadChildrenAsync)) {
                    const rootNode = this.rootNode;
                    rowDataAsync = rootNode.loadChildrenAsync().then(() => rootNode.children);
                } else {
                    rowDataAsync = this.rootNode.children;
                }
            } else {
                rowDataAsync = [];
            }

            const selectedVMs: {[key: string]: ITreeViewNode} = {};
            if (saveSelection) {
                this.options.api.getSelectedRows().forEach((row: ITreeViewNode) => selectedVMs[row.key] = row);
            } else {
                this.options.api.setRowData([]);
                this.options.api.showLoadingOverlay();
            }

            return this.$q.when(rowDataAsync).then((rowData) => {
                if (this.options.api) {
                    this.options.api.setRowData(rowData);

                    if (this.sizeColumnsToFit) {
                        this.timers[1] = this.$timeout(() => {
                            this.options.api.sizeColumnsToFit();
                        });
                    } else {
                        this.options.columnApi.autoSizeAllColumns();
                    }

                    if (saveSelection) {
                        // Restore selection (don't raise selection events)
                        this.options.onRowSelected = undefined;
                        this.options.api.forEachNode(node => {
                            if (selectedVMs[node.data.key]) {
                                node.setSelected(true);
                            }
                        });
                        this.options.onRowSelected = this.onRowSelected;
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

    public updateScrollbars() {
        const viewport = this.$element[0].querySelector(".ag-body-viewport");
        if (viewport ) {
            this.options.columnApi.autoSizeColumns(this.options.columnDefs.map(columnDef => columnDef.field ));
       }
    };

    // Callbacks

    public getNodeChildDetails(dataItem: any): agGrid.NodeChildDetails {
        const vm = dataItem as ITreeViewNode;
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
        const vm = node.data as ITreeViewNode;
        return vm.key;
    }

    // Event handlers

    public onRowGroupOpened = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const vm = node.data as ITreeViewNode;

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
    };

    public onModelUpdated = (event?: any) => {
        this.updateScrollbars();
    };

    public onViewportChanged = (event?: any) => {
        this.updateScrollbars();
    };

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
        const vm = node.data as ITreeViewNode;

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
    };

    public onRowSelected = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const isSelected = node.isSelected();
        const vm = node.data as ITreeViewNode;
        if (isSelected && (!vm.isSelectable() || !this.isVisible(node))) {
            node.setSelected(false);
        } else if (this.onSelect) {
            this.onSelect({vm: vm, isSelected: isSelected});
        }
    };

    private isVisible(node: agGrid.RowNode): boolean {
        while ((node = node.parent)) {
            if (!node.expanded) {
                return false;
            }
        }
        return true;
    }

    public onRowDoubleClicked = (event: {data: ITreeViewNode}) => {
        const vm = event.data;
        if (this.onDoubleClick && vm.isSelectable() && !vm.isExpandable) {
            this.onDoubleClick({vm: vm});
        }
    };

    public onGridReady = (event?: any) => {
        this.resetGridAsync(false);
    }
}

export class HeaderCell {
    constructor(public gridOptions: agGrid.GridOptions) { }

    selectAll(value: boolean) {
        this.gridOptions.api.forEachNodeAfterFilter(node => node.setSelected(value));
    }
}
