import * as agGrid from "ag-grid/main";
import { ILocalizationService } from "../../../core";

/**
 * Usage:
 * 
 * <bp-tree-view grid-class="project-tree"
 *               row-height="20"
 *               root-node="$ctrl.rootNode"
 *               root-node-visible="false"
 *               column-defs="$ctrl.columnDefs"
 *               header-height="20"
 *               on-select="$ctrl.onSelect(vm)">
 * </bp-tree-view>
 */
export class BPTreeViewComponent implements ng.IComponentOptions {
    public controller: Function = BPTreeViewController;
    public template: string = require("./bp-tree-view.html");
    public bindings: {[binding: string]: string} = {
        gridClass: "@",
        rowBuffer: "<",
        rowHeight: "<",
        rootNode: "<",
        rootNodeVisible: "<",
        columnDefs: "<",
        headerHeight: "<",
        onSelect: "&?",
    };
}

export interface IBPTreeViewController extends ng.IComponentController {
    // Template bindings
    gridClass: string;
    options: agGrid.GridOptions;

    // Grid options
    rowBuffer: number;
    rowHeight: number;
    rootNode: ITreeViewNodeVM;
    rootNodeVisible: boolean;
    columnDefs: agGrid.ColDef[];
    headerHeight: number;
    onSelect: (param: {vm: ITreeViewNodeVM}) => void;
}

export interface ITreeViewNodeVM {
    key: string; // Each row in the dom will have an attribute row-id='key'
    isExpandable: boolean;
    children: ITreeViewNodeVM[];
    isExpanded: boolean;
    loadChildrenAsync?(): ng.IPromise<any>; // To lazy-load children
}

export class BPTreeViewController implements IBPTreeViewController {
    public static $inject = ["$q", "$element", "localization"];

    // Template bindings
    public gridClass: string;
    public options: agGrid.GridOptions;

    // Grid options
    public rowBuffer: number;
    public rowHeight: number;
    public rootNode: ITreeViewNodeVM;
    public rootNodeVisible: boolean;
    public columnDefs: agGrid.ColDef[];
    public headerHeight: number;
    public onSelect: (param: {vm: ITreeViewNodeVM}) => void;

    constructor(private $q: ng.IQService, private $element: HTMLElement, private localization: ILocalizationService) {
        this.gridClass = this.gridClass || "project-explorer";
        this.rowBuffer = this.rowBuffer || 200;
        this.rowHeight = this.rowHeight || 24;
        this.rootNodeVisible = this.rootNodeVisible || false;
        this.columnDefs = this.columnDefs || [];
        this.headerHeight = this.headerHeight || 0;
    }

    public $onInit(): void {
        this.options = {
            rowBuffer: this.rowBuffer,
            enableColResize: true,
            icons: {
                groupExpanded: "<i />",
                groupContracted: "<i />"
            },
            angularCompileRows: true, // this is needed to compile directives (dynamically added) on the rows
            suppressContextMenu: true,
            localeTextFunc: (key: string, defaultValue: string) => this.localization.get("ag-Grid_" + key, defaultValue),
            rowSelection: "single",
            rowHeight: this.rowHeight,
            showToolPanel: false,
            columnDefs: this.columnDefs,
            headerHeight: this.headerHeight,
            getBusinessKeyForNode: this.getBusinessKeyForNode,
            getNodeChildDetails: this.getNodeChildDetails,
            onRowGroupOpened: this.onRowGroupOpened,
            onViewportChanged: this.onViewportChanged,
            onCellFocused: this.onCellFocused,
            onRowSelected: this.onRowSelected,
            onGridReady: this.onGridReady,
            onModelUpdated: this.onModelUpdated
        };
    }

    public $onChanges(onChangesObj: ng.IOnChangesObject): void {
        if (this.options && (onChangesObj["rootNode"] || onChangesObj["rootNodeVisible"])) {
            this.resetRowDataAsync();
        }
    }

    public $onDestroy(): void {
        this.options.api.setRowData(null);
        this.updateScrollbars(true);
    }

    public resetRowDataAsync(): ng.IPromise<void> {
        let rowDataAsync: ITreeViewNodeVM[] | ng.IPromise<ITreeViewNodeVM[]>;
        if (this.rootNode) {
            if (this.rootNodeVisible) {
                rowDataAsync = [this.rootNode];
            } else if (this.rootNode.loadChildrenAsync) {
                rowDataAsync = this.rootNode.loadChildrenAsync().then(() => this.rootNode.children);
            } else {
                rowDataAsync = this.rootNode.children;
            }
        } else {
            rowDataAsync = [];
        }

        const self = this;
        return this.$q.when(rowDataAsync).then((rowData) => {
            self.options.api.setRowData(rowData);
            self.options.api.sizeColumnsToFit();
        });
    }

    public updateScrollbars(destroy: boolean = false) {
        let viewport = this.$element[0].querySelector(".ag-body-viewport");
        let perfectScrollBar = (<any>window).PerfectScrollbar;

        if (viewport && angular.isDefined(perfectScrollBar)) {
            if (destroy) {
                perfectScrollBar.destroy(viewport);
            } else {
                if (viewport.getAttribute("data-ps-id")) {
                    // perfect-scrollbar has been initialized on the element (data-ps-id is not falsy)
                    let allColumnIds = [];
                    this.options.columnDefs.forEach(function(columnDef) {
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

    // BEGIN ag-grid callbacks

    public getNodeChildDetails(dataItem: any): agGrid.NodeChildDetails {
        let vm = dataItem as ITreeViewNodeVM;
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
        let vm = node.data as ITreeViewNodeVM;
        return vm.key;
    }

    // END ag-grid callbacks

    // BEGIN ag-grid event handlers

    public onRowGroupOpened = (event: {node: agGrid.RowNode}) => {
        let node = event.node;
        let vm = node.data as ITreeViewNodeVM;

        if (vm.isExpandable) {
            let row = this.$element[0].querySelector(`.ag-body .ag-body-viewport-wrapper .ag-row[row-id="${vm.key}"]`);
            if (row) {
                row.classList.remove(node.expanded ? "ag-row-group-contracted" : "ag-row-group-expanded");
                row.classList.add(node.expanded ? "ag-row-group-expanded" : "ag-row-group-contracted");
            }
            if (node.expanded && vm.loadChildrenAsync) {
                if (row) {
                    row.classList.add("ag-row-loading");
                }
                vm.loadChildrenAsync().then(() => this.resetRowDataAsync());
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

    public onCellFocused = (event: {rowIndex: number}) => {
        this.options.api.getModel().getRow(event.rowIndex).setSelected(true);
    }

    public onRowSelected = (event: {node: agGrid.RowNode}) => {
        let node = event.node;
        if (this.onSelect && node.isSelected()) {
            let vm = node.data as ITreeViewNodeVM;
            this.onSelect({vm: vm});
        }
    }

    public onGridReady = (event?: any) => {
        this.resetRowDataAsync();
    }

    // END ag-grid event handlers
}
