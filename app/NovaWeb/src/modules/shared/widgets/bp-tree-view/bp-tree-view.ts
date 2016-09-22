import * as agGrid from "ag-grid/main";
import { ILocalizationService } from "../../../core";

/**
 * Usage:
 * 
 * <bp-tree-view grid-class="project-tree"
 *               selection-mode="'single'"
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
        selectionMode: "<",
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
    selectionMode: "single" | "multiple" | "checkbox";
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
    public selectionMode: "single" | "multiple" | "checkbox";
    public rowHeight: number;
    public rootNode: ITreeViewNodeVM;
    public rootNodeVisible: boolean;
    public columnDefs: agGrid.ColDef[];
    public headerHeight: number;
    public onSelect: (param: {vm: ITreeViewNodeVM}) => void;

    constructor(private $q: ng.IQService, private $element: HTMLElement, private localization: ILocalizationService) {
        this.gridClass = angular.isDefined(this.gridClass) ? this.gridClass : "project-explorer";
        this.selectionMode = angular.isDefined(this.selectionMode) ? this.selectionMode : "single";
        this.rowBuffer = angular.isDefined(this.rowBuffer) ? this.rowBuffer : 200;
        this.rowHeight = angular.isDefined(this.rowHeight) ? this.rowHeight : 24;
        this.rootNodeVisible = angular.isDefined(this.rootNodeVisible) ? this.rootNodeVisible : false;
        this.columnDefs = angular.isDefined(this.columnDefs) ? this.columnDefs : [];
        this.headerHeight = angular.isDefined(this.headerHeight) ? this.headerHeight : 0;
    }

    public $onInit(): void {
        this.options = {
            suppressRowClickSelection: true,
            rowBuffer: this.rowBuffer,
            enableColResize: true,
            icons: {
                groupExpanded: "<i />",
                groupContracted: "<i />"
            },
            angularCompileRows: true, // this is needed to compile directives (dynamically added) on the rows
            suppressContextMenu: true,
            localeTextFunc: (key: string, defaultValue: string) => this.localization.get("ag-Grid_" + key, defaultValue),
            rowSelection: this.selectionMode === "single" ? "single" : "multiple",
            rowDeselection: this.selectionMode !== "single",
            rowHeight: this.rowHeight,
            showToolPanel: false,
            columnDefs: this.columnDefs,
            headerHeight: this.headerHeight,
            getBusinessKeyForNode: this.getBusinessKeyForNode,
            getNodeChildDetails: this.getNodeChildDetails,
            onRowGroupOpened: this.onRowGroupOpened,
            onViewportChanged: this.onViewportChanged,
            onCellClicked: this.onCellClicked,
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

        return this.$q.when(rowDataAsync).then((rowData) => {
            // Save selection
            const selectedVMs: {[key: string]: ITreeViewNodeVM} = {};
            this.options.api.getSelectedRows().forEach((row: ITreeViewNodeVM) => selectedVMs[row.key] = row);

            this.options.api.setRowData(rowData);
            this.options.api.sizeColumnsToFit();

            // Restore selection
            this.options.api.forEachNode(node => {
                if (selectedVMs[node.data.key]) {
                    node.setSelected(true);
                }
            });
        });
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

    // END ag-grid callbacks

    // BEGIN ag-grid event handlers

    public onRowGroupOpened = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const vm = node.data as ITreeViewNodeVM;

        if (vm.isExpandable) {
            const row = this.$element[0].querySelector(`.ag-body .ag-body-viewport-wrapper .ag-row[row-id="${vm.key}"]`);
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

    public onCellClicked = (event: {event: MouseEvent, node: agGrid.RowNode}) => {
        // Only deal with clicks in the .ag-group-value span
        let element = event.event.target as Element;
        while (!(element && element.classList.contains("ag-group-value"))) {
            if (!element || element === this.$element) {
                return;
            }
            element = element.parentElement;
        }

        const node = event.node as agGrid.RowNode &
            {setSelectedParams: (params: {newValue: boolean, clearSelection?: boolean, tailingNodeInSequence?: boolean, rangeSelect?: boolean}) => void};

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
        } else {
            node.setSelectedParams({newValue: true, clearSelection: !multiSelectKeyPressed, rangeSelect: shiftKeyPressed});
        }
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
