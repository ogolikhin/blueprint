import * as _ from "lodash";
import * as agGrid from "ag-grid/main";
import {IWindowManager, IMainWindow, ResizeCause} from "../../../main/services";
import {ILocalizationService} from "../../../core/localization/localizationService";

/**
 * Usage:
 *
 * <bp-tree-view api="$ctrl.api"
 *               grid-class="project-tree"
 *               row-buffer="200"
 *               selection-mode="'single'"
 *               row-height="20"
 *               row-data="$ctrl.rowData"
 *               root-node-visible="false"
 *               columns="$ctrl.columns"
 *               header-height="20"
 *               size-columns-to-fit="true"
 *               on-select="$ctrl.onSelect(vm, isSelected)"
 *               on-double-click="$ctrl.onDoubleClick(vm)"
 *               on-error="$ctrl.onError(reason)"
 *               on-grid-reset="$ctrl.onGridReset(isExpanding)">
 * </bp-tree-view>
 */
export class BPTreeViewComponent implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPTreeViewController;
    public template: string = require("./bp-tree-view.html");
    public bindings: {[binding: string]: string} = {
        // Two-way
        api: "=?",
        // Input
        gridClass: "@",
        rowBuffer: "<",
        selectionMode: "<",
        rowHeight: "<",
        rowData: "<",
        rootNodeVisible: "<",
        columns: "<",
        headerHeight: "<",
        sizeColumnsToFit: "<",
        // Output
        onSelect: "&?",
        onDoubleClick: "&?",
        onError: "&?",
        onGridReset: "&?"
    };
}

export interface IBPTreeViewController extends ng.IComponentController {
    // BPTreeViewComponent bindings
    api: IBPTreeViewControllerApi;
    gridClass: string;
    rowBuffer: number;
    selectionMode: "single" | "multiple" | "checkbox";
    rowHeight: number;
    rowData: ITreeNode[];
    rootNodeVisible: boolean;
    columns: IColumn[];
    headerHeight: number;
    onSelect: (param: {vm: ITreeNode, isSelected: boolean}) => any;
    onDoubleClick: (param: {vm: ITreeNode}) => void;
    onError: (param: {reason: any}) => void;
    onGridReset: (param: {isExpanding: boolean}) => void;

    // ag-grid bindings
    options: agGrid.GridOptions;
}

export interface ITreeNode {
    // agGrid.NodeChildDetails
    /** If true, the node is expandable; otherwise, it is not. */
    group?: boolean;
    /** Array of children, or undefined if children should be loaded through loadChildrenAsync. */
    children?: ITreeNode[];
    /** If true, the node is expanded; otherwise, it is collapsed. */
    expanded?: boolean;
    /** Each row in the dom will have an attribute row-id='key' */
    key: string;

    /** If true, can be selected; otherwise, it can not. */
    selectable: boolean;
    /** Function returning a promise of an array of children, or undefined if children are provided through children. */
    loadChildrenAsync?(): ng.IPromise<ITreeNode[]>;
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
    cellClass?: (vm: ITreeNode) => string[];
    innerRenderer?: (params: IColumnRendererParams) => string;
}

export interface IColumnRendererParams {
    data: ITreeNode;
    eGridCell: HTMLElement;
    $scope: ng.IScope;
}

export interface IHeaderCellRendererParams {
    context?: any;
}

export interface IBPTreeViewControllerApi {
    setSelected(comparator: ITreeNode | ((vm: ITreeNode) => boolean), selected?: boolean, clearSelection?: boolean): boolean;
    ensureNodeVisible(comparator: ITreeNode | ((vm: ITreeNode) => boolean)): void;
    deselectAll(): void;
    updateSelectableNodes(isItemSelectable: (item) => boolean): void;
    refreshRows(comparator: ITreeNode | ((vm: ITreeNode) => boolean)): void;
}

export class BPTreeViewController implements IBPTreeViewController {
    public static $inject = ["$q", "$element", "localization", "$timeout", "windowManager"];

    // BPTreeViewComponent bindings
    public gridClass: string;
    public rowBuffer: number;
    public selectionMode: "single" | "multiple" | "checkbox";
    public rowHeight: number;
    public rowData: ITreeNode[];
    public rootNodeVisible: boolean;
    public columns: IColumn[];
    public headerHeight: number;
    public sizeColumnsToFit: boolean;
    public onSelect: (param: {vm: ITreeNode, isSelected: boolean}) => any;
    public onDoubleClick: (param: {vm: ITreeNode}) => void;
    public onError: (param: {reason: any}) => void;
    public onGridReset: (param: {isExpanding: boolean}) => void;

    // ag-grid bindings
    public options: agGrid.GridOptions;

    private timers = [];

    constructor(private $q: ng.IQService, private $element: ng.IAugmentedJQuery, private localization: ILocalizationService,
                private $timeout: ng.ITimeoutService, private windowManager: IWindowManager) {
        this.gridClass = angular.isDefined(this.gridClass) ? this.gridClass : "project-explorer";
        this.rowBuffer = angular.isDefined(this.rowBuffer) ? this.rowBuffer : 200;
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.rowHeight = angular.isDefined(this.rowHeight) ? this.rowHeight : 24;
        this.rowData = angular.isArray(this.rowData) ? this.rowData : [];
        this.rootNodeVisible = angular.isDefined(this.rootNodeVisible) ? this.rootNodeVisible : true;
        this.columns = angular.isDefined(this.columns) ? this.columns : [];
        this.headerHeight = angular.isDefined(this.headerHeight) ? this.headerHeight : 0;
        this.sizeColumnsToFit = angular.isDefined(this.sizeColumnsToFit) ? this.sizeColumnsToFit : false;

        this.options = {
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
            angularCompileHeaders: true,
            suppressContextMenu: true,
            suppressMenuMainPanel: true,
            suppressMenuColumnPanel: true,
            localeTextFunc: (key: string, defaultValue: string) => this.localization.get("ag-Grid_" + key, defaultValue),
            context: {},
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

        this.options.context.allSelected = false;
        this.options.context.selectAllClass = new HeaderCell(this.options);
    }

    public $onInit() {
        if (this.sizeColumnsToFit) {
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this);
        }
    }

    public $onChanges(onChangesObj: ng.IOnChangesObject): void {
        if (onChangesObj["selectionMode"] || onChangesObj["rowData"] || onChangesObj["rootNodeVisible"] || onChangesObj["columns"]) {
            this.resetGridAsync(false, 0);
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
        this.api = null;
        this.rowData = null;
    }

    public api: IBPTreeViewControllerApi = {
        setSelected: (comparator: ITreeNode | ((vm: ITreeNode) => boolean), selected: boolean = true, clearSelection: boolean = true) => {
            let result = false;
            this.options.api.forEachNode((node: agGrid.RowNode) => {
                const vm = node.data as ITreeNode;
                if (_.isFunction(comparator) ? comparator(vm) : vm === comparator) {
                    node.setSelected(selected, clearSelection);
                    result = true;
                }
            });
            return result;
        },
        ensureNodeVisible: (comparator: ITreeNode | ((vm: ITreeNode) => boolean)): void => {
            if (_.isFunction(comparator)) {
                this.options.api.ensureNodeVisible((node: agGrid.RowNode) => comparator(node.data as ITreeNode));
            } else if (comparator) {
                this.options.api.ensureNodeVisible(comparator);
            }
        },
        deselectAll: (): void => {
            this.options.api.deselectAll();
        },
        updateSelectableNodes: (isItemSelectable: (item) => boolean): void => {
            this.options.api.forEachNode(node => {
                node.data.selectable = isItemSelectable(node.data.model);
            });
            this.options.api.refreshView();
        },
        refreshRows: (comparator: ITreeNode | ((vm: ITreeNode) => boolean)): void => {
            const rowNodes: agGrid.RowNode[] = [];
            this.options.api.forEachNode((node: agGrid.RowNode) => {
                const vm = node.data as ITreeNode;
                if (_.isFunction(comparator) ? comparator(vm) : vm === comparator) {
                    rowNodes.push(node);
                }
            });
            this.options.api.refreshRows(rowNodes);
        }
    };

    public resetGridAsync(isExpanding: boolean, fitColumnDelay: number = 500): ng.IPromise<void> {
        if (this.options.api) {
            this.options.rowSelection = this.selectionMode === "single" ? "single" : "multiple";
            this.options.rowDeselection = this.selectionMode !== "single";

            this.options.api.setColumnDefs(this.columns.map(column => ({
                   headerName: column.headerName ? column.headerName : "",
                   field: column.field,
                   width: column.width,
                cellClass: column.cellClass ? (params: agGrid.RowNode) => column.cellClass(params.data as ITreeNode) : undefined,
                   cellRenderer: column.isGroup ? "group" : column.innerRenderer,
                   cellRendererParams: column.isGroup ? {
                        checkbox: this.selectionMode === "checkbox" && !column.isCheckboxHidden ?
                            (params: any) => (params.data as ITreeNode).selectable : undefined,
                        innerRenderer: column.innerRenderer ?
                            (params: any) => column.innerRenderer(params as IColumnRendererParams) : undefined,
                        padding: 20
                    } : undefined,
                    checkboxSelection: column.isCheckboxSelection,
                    suppressMenu: true,
                    suppressSorting: true,
                    headerCellRenderer: column.headerCellRenderer
                } as agGrid.ColDef)));

            const selectedVMs: {[key: string]: ITreeNode} = {};
            this.options.api.getSelectedRows().forEach((row: ITreeNode) => selectedVMs[row.key] = row);
            if (!isExpanding) {
                this.options.api.setRowData([]);
                this.options.api.showLoadingOverlay();
            }

            return this.$q.all(this.rowData.filter(vm => this.isLazyLoaded(vm)).map(vm => this.loadExpanded(vm))).then(() => {
                if (this.options.api) {
                    this.options.api.setRowData(this.rootNodeVisible ? this.rowData : _.flatten(this.rowData.map(r => r.children)));

                    if (this.sizeColumnsToFit) {
                        this.timers[1] = this.$timeout(() => {
                            this.options.api.sizeColumnsToFit();
                        }, fitColumnDelay);
                    } else {
                        this.options.columnApi.autoSizeAllColumns();
                    }

                    if (!_.isEmpty(selectedVMs)) {
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
                if (_.isFunction(this.onError)) {
                    this.onError({reason: reason});
                }
            }).finally(() => {
                if (this.options.api) {
                    this.options.api.hideOverlay();
                    if (this.options.api.getModel().getRowCount() === 0) {
                        this.options.api.showNoRowsOverlay();
                    }
                }
                if (_.isFunction(this.onGridReset)) {
                    this.onGridReset({isExpanding: isExpanding});
                }
            });
        }

        return this.$q.resolve();
    }

    private isLazyLoaded(vm: ITreeNode): boolean {
        return vm.expanded && !_.isArray(vm.children) && _.isFunction(vm.loadChildrenAsync);
    }

    private loadExpanded(vm: ITreeNode): ng.IPromise<any> {
        return vm.loadChildrenAsync().then(children => {
            vm.children = children;
            return this.$q.all(children.filter(this.isLazyLoaded).map(this.loadExpanded));
        });
    }

    public updateScrollbars() {
        const viewport = this.$element[0].querySelector(".ag-body-viewport");
        if (viewport) {
            this.options.columnApi.autoSizeAllColumns();
        }
    };

    // Callbacks

    public getNodeChildDetails(dataItem: any): agGrid.NodeChildDetails {
        const vm = dataItem as ITreeNode;
        if (vm.group) {
            return {
                group: true,
                children: vm.children || [],
                expanded: vm.expanded,
                key: vm.key
            } as agGrid.NodeChildDetails;
        }
        return null;
    }

    public getBusinessKeyForNode(node: agGrid.RowNode): string {
        const vm = node.data as ITreeNode;
        return vm.key;
    }

    // Event handlers

    public onRowGroupOpened = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const vm = node.data as ITreeNode;

        if (vm.group) {
            const row = this.$element[0].querySelector(`.ag-body .ag-body-viewport-wrapper .ag-row[row-id="${vm.key}"]`);
            if (row) {
                row.classList.remove(node.expanded ? "ag-row-group-contracted" : "ag-row-group-expanded");
                row.classList.add(node.expanded ? "ag-row-group-expanded" : "ag-row-group-contracted");
            }
            vm.expanded = node.expanded;
            if (this.isLazyLoaded(vm)) {
                if (row) {
                    row.classList.add("ag-row-loading");
                }
                this.loadExpanded(vm).then(() => this.resetGridAsync(true)).catch(reason => {
                    if (_.isFunction(this.onError)) {
                        this.onError({reason: reason});
                    }
                });
            }
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
        const vm = node.data as ITreeNode;

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
        } else if (vm.selectable) {
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
        const vm = node.data as ITreeNode;
        if (isSelected && (!vm.selectable || !this.isVisible(node))) {
            node.setSelected(false);
        } else if (_.isFunction(this.onSelect)) {
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

    public onRowDoubleClicked = (event: {data: ITreeNode}) => {
        const vm = event.data;
        if (_.isFunction(this.onDoubleClick) && vm.selectable && !vm.group) {
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
