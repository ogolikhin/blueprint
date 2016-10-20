import * as angular from "angular";
import * as agGrid from "ag-grid/main";
import {ILocalizationService} from "../../core";
import {IBPTreeViewController, ITreeViewNodeVM, IColumn} from "../../shared/widgets/bp-tree-view";
import {BPTreeViewController} from "../../shared/widgets/bp-tree/bp-tree";

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
export class BPCollectionViewComponent implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPCollectionViewController;
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

//export interface IBPTreeViewController extends ng.IComponentController {
//    // Template bindings
//    gridClass: string;
//    options: agGrid.GridOptions;

//    // Grid options
//    rowBuffer: number;
//    selectionMode: "single" | "multiple" | "checkbox";
//    rowHeight: number;
//    rootNode: ITreeViewNodeVM | ITreeViewNodeVM[];
//    rootNodeVisible: boolean;
//    columns: IColumn[];
//    headerHeight: number;
//    onSelect: (param: {vm: ITreeViewNodeVM, isSelected: boolean, selectedVMs: ITreeViewNodeVM[]}) => void;
//    onDoubleClick: (param: {vm: ITreeViewNodeVM}) => void;
//    onError: (param: {reason: any}) => void;
//}

//export interface ITreeViewNodeVM {
//    key: string; // Each row in the dom will have an attribute row-id='key'
//    isExpandable?: boolean;
//    children?: ITreeViewNodeVM[];
//    isExpanded?: boolean;
//    isSelectable(): boolean;
//    loadChildrenAsync?(): ng.IPromise<any>; // To lazy-load children
//}

//export interface IColumn {
//    headerName?: string;
//    field?: string;
//    isGroup?: boolean;
//    isCheckboxSelection?: boolean;
//    cellClass?: (vm: ITreeViewNodeVM) => string[];
//    innerRenderer?: (vm: ITreeViewNodeVM, eGridCell: HTMLElement) => string;
//    isSortable?: boolean;
//    filter?: "number" | "text" | "set";
//}

export class BPCollectionViewController extends BPTreeViewController {
    public static $inject = ["$q", "$element", "localization"];

    // Template bindings
    public gridClass: string;
    public options: agGrid.GridOptions;

    // Grid options
    public rowBuffer: number;
    public selectionMode: "single" | "multiple" | "checkbox";
    public rowHeight: number;
    public rootNode: ITreeViewNodeVM | ITreeViewNodeVM[];
    public rootNodeVisible: boolean;
    public columns: IColumn[];
    public headerHeight: number;
    public onSelect: (param: {vm: ITreeViewNodeVM, isSelected: boolean, selectedVMs: ITreeViewNodeVM[]}) => void;
    public onDoubleClick: (param: {vm: ITreeViewNodeVM}) => void;
    public onError: (param: {reason: any}) => void;

    constructor(private $q: ng.IQService, private $element: ng.IAugmentedJQuery, private localization: ILocalizationService) {
        super($q,  $element,  localization);
        //this.options = {
        //    suppressRowClickSelection: true,
        //    rowBuffer: this.rowBuffer,
        //    enableColResize: true,
        //    enableSorting: true,
        //    enableFilter: true,
        //    icons: {
        //        groupExpanded: "<i />",
        //        groupContracted: "<i />",
        //        checkboxChecked: `<i class="ag-checkbox-checked" />`,
        //        checkboxUnchecked: `<i class="ag-checkbox-unchecked" />`,
        //        checkboxIndeterminate: `<i class="ag-checkbox-indeterminate" />`
        //    },
        //    angularCompileRows: true, // this is needed to compile directives (dynamically added) on the rows
        //    suppressContextMenu: true,
        //    suppressMenuMainPanel: true,
        //    suppressMenuColumnPanel: true,
        //    localeTextFunc: (key: string, defaultValue: string) => this.localization.get("ag-Grid_" + key, defaultValue),
        //    rowSelection: this.selectionMode === "single" ? "single" : "multiple",
        //    rowDeselection: this.selectionMode !== "single",
        //    rowHeight: this.rowHeight,
        //    rowData: [],
        //    showToolPanel: false,
        //    columnDefs: [],
        //    headerHeight: this.headerHeight,

        //    // Callbacks
        //    getBusinessKeyForNode: this.getBusinessKeyForNode,
        //    getNodeChildDetails: this.getNodeChildDetails,

        //    // Event handlers
        //    onRowGroupOpened: this.onRowGroupOpened,
        //    onViewportChanged: this.onViewportChanged,
        //    onCellClicked: this.onCellClicked,
        //    onRowSelected: this.onRowSelected,
        //    onRowDoubleClicked: this.onRowDoubleClicked,
        //    onGridReady: this.onGridReady,
        //    onModelUpdated: this.onModelUpdated
        //};
    }

    
}
