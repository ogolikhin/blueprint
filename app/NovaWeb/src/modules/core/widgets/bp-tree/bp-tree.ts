import "angular";
import * as Grid from "ag-grid/main";
import {Helper} from "../../../core/utils/helper";

/*
tslint:disable
*/ /*
Sample template. See following parameters:
<bp-tree 
    grid-class="project_explorer" - wrapper css class name
    row-buffer="200" - number of rendered outside the scrollable viewable area the ag-grid renders. Having a buffer means the grid will have rows ready to show as the user slowly scrolls vertically.
    row-height="24" - row height in pixels
    header-height="20" - header height in pixels. Set to 0 to disable
    enable-editing-on="name,desc" - list of column where to activate inline editing
    enable-dragndrop="true" - enable rows drag and drop
    grid-columns="$ctrl.columns"  - column definition
    on-load="$ctrl.doLoad(prms)"  - gets data to load tree root nodes or a sub-tree (child node)
    on-select="$ctrl.doSelect(item)"> - to be called then a node is selected
    on-row-click="$ctrl.doRowClick(prms)" - to be called when a row is clicked
    on-row-dblclick="$ctrl.doRowDblClick(prms)" - to be called when a row is double-clicked (will cancel single-click)
    on-row-post-create="$ctrl.doRowPostCreate(prms)" - to be called after a row is created
</bp-tree>
*/ /*
tslint:enable
*/


export class BPTreeComponent implements ng.IComponentOptions {
    public template: string = require("./bp-tree.html");
    public controller: Function = BPTreeController;
    public bindings: any = {
        //properties
        gridClass: "@",
        enableEditingOn: "@",
        enableDragndrop: "<",
        rowHeight: "<",
        rowBuffer: "<",
        headerHeight: "<",
        //settings
        gridColumns: "<",
        //to set connection with parent
        bpRef: "=?",
        //events
        onLoad: "&?",
        onSelect: "&?",
        onRowClick: "&?",
        onRowDblClick: "&?",
        onRowPostCreate: "&?"
    };
}
export interface ITreeNode {
    id: number;
    hasChildren: boolean;
    children: ITreeNode[];
    loaded: boolean;
    open: boolean;
}
export interface IBPTreeController {
    onLoad?: Function;
    onSelect?: Function;
    onRowClick?: Function;
    onRowDblClick?: Function;
    onRowPostCreate?: Function;

    addNode(data: ITreeNode);
    selectNode(id: number);
    setDataSource(data: any[], id?: number);
}


export class BPTreeController  {
    static $inject = ["$element", "$timeout"];
    //properties
    public gridClass: string;
    public enableEditingOn: string;
    public enableDragndrop: boolean;
    public rowBuffer: number;
    public rowHeight: number;
    public headerHeight: number;
    //settings
    public gridColumns: any[];

    //events
    public onLoad: Function;
    public onSelect: Function;
    public onRowClick: Function;
    public onRowDblClick: Function;
    public onRowPostCreate: Function;

    public bpRef: BPTreeController;

    public options: Grid.GridOptions;
    private editableColumns: string[] = [];
    public rowData: ITreeNode[] = null;
    private selectedRow: any;
    private clickTimeout: any;

    constructor(private $element, private $timeout: ng.ITimeoutService) {
        this.bpRef = this;

        this.gridClass = this.gridClass ? this.gridClass : "project-explorer";
        this.enableDragndrop = this.enableDragndrop ? true : false;
        this.rowBuffer = this.rowBuffer ? this.rowBuffer : 200;
        this.rowHeight = this.rowHeight ? this.rowHeight : 24;
        this.headerHeight = this.headerHeight ? this.headerHeight : 0;
        this.editableColumns = this.enableEditingOn && this.enableEditingOn !== "" ? this.enableEditingOn.split(",") : [];

        if (angular.isArray(this.gridColumns)) {
            this.gridColumns.map(function (gridCol) {
                // if we are grouping and the caller doesn't provide the innerRenderer, we use the default one
                if (gridCol.cellRenderer === "group") {
                    if (!gridCol.cellRendererParams ||
                        (gridCol.cellRendererParams && !gridCol.cellRendererParams.innerRenderer) ||
                        (gridCol.cellRendererParams && !angular.isFunction(gridCol.cellRendererParams.innerRenderer))
                    ) {
                        if (!gridCol.cellRendererParams) {
                            gridCol.cellRendererParams = {};
                        }

                        gridCol.cellRendererParams.innerRenderer = this.innerRenderer;
                    }
                }
            }.bind(this));
        } else {
            this.gridColumns = [];
        }
    }

    public addNode(data: ITreeNode)  {
        this.rowData = [data].concat(this.rowData);
        this.options.api.setRowData(this.rowData);
    }

    public setDataSource(data?: any[], id?: number) {
        if (angular.isArray(data)) {
            data.map(function (it) {
                if (it.hasChildren && !angular.isArray(it.children)) {
                    it.children = [];
                };
            });
            let node = this.findNode(id);
            if (node) {
                node.data.children = data;
                node.data.loaded = true;
                node.open = true;
            } else {
                this.rowData = data.concat(this.rowData || []);
            }
        }
        this.options.api.setRowData(this.rowData); 
    }

    public selectNode(id: number) {
        let node = this.findNode(id);
        if (node) {
            this.options.api.refreshView();        }
    }

    private findNode(id: number): any {
        let node: any;
        this.options.api.forEachNode(function (it) {
            if (it.data.id === id) {
                node = it;
            }
        });
        return node;
    }

    public $onInit = () => {
        this.options = <Grid.GridOptions>{
            angularCompileRows: true, // this is needed to compile directives (dynamically added) on the rows
            headerHeight: this.headerHeight,
            showToolPanel: false,
            suppressContextMenu: true,
            rowBuffer: this.rowBuffer,
            rowHeight: this.rowHeight,
            enableColResize: true,
            columnDefs: this.gridColumns,
            icons: {
                groupExpanded: "<i class='fonticon-' />",
                groupContracted: "<i class='fonticon-' />"
            },
            getNodeChildDetails: this.getNodeChildDetails,
            onCellFocused: this.cellFocused,
            onRowClicked: this.rowClicked,
            onRowDoubleClicked: this.rowDoubleClicked,
            onRowGroupOpened: this.rowGroupOpened,
            processRowPostCreate: this.rowPostCreate,
            onGridReady: this.onGridReady
        };
    };

    private innerRenderer = (params: any) => {
        var currentValue = params.value;
        var inlineEditing = this.editableColumns.indexOf(params.colDef.field) !== -1 ? " bp-tree-inline-editing" : "";

        return "<span" + inlineEditing + ">" + Helper.escapeHTMLText(currentValue) + "</span>";
    };

    private getNodeChildDetails(rowItem) {
        if (rowItem.children) {
            return {
                group: true,
                expanded: rowItem.open,
                children: rowItem.children,
                field: "Name",
                key: rowItem.Id // the key is used by the default group cellRenderer
            };
        } else {
            return null;
        }
    };

    private onGridReady = (params: any) => {
        let self = this;
        if (params && params.api) {
            params.api.sizeColumnsToFit();
        }
        if (angular.isFunction(self.onLoad)) {
            //this verifes and updates current node to inject children
            //NOTE:: this method may uppdate grid datasource using setDataSource method
            let nodes = self.onLoad({ prms: null });
            if (angular.isArray(nodes)) {
                self.setDataSource(nodes);
            }
        }
    };

    private rowGroupOpened = (params: any) => {
        let self = this;
        let node = params.node;
        if (node.data.hasChildren && !node.data.loaded) {
            if (angular.isFunction(self.onLoad)) {
                let nodes = self.onLoad({ prms: node.data });
                //this verifes and updates current node to inject children
                //NOTE:: this method may uppdate grid datasource using setDataSource method
                if (angular.isArray(nodes)) {
                    node.data.children = nodes;
                    node.data.loaded = true;
                    node.data.open = true;
                    self.setDataSource(); // pass nothing to just reload 
                }
            }
        }
        node.data.open = node.expanded;
    };

    private cellFocused = (params: any) => {
        var model = this.options.api.getModel();
        this.selectedRow = model.getRow(params.rowIndex);
        this.selectedRow.setSelected(true, true);
        if (typeof this.onSelect === `function`) {
            this.onSelect({item: this.selectedRow.data});
        }
    };

    private rowFocus = (target: any) => {
        function findAncestor(el, cls) {
            while ((el = el.parentElement) && !el.classList.contains(cls)) {
            }
            return el;
        }

        var clickedCell = findAncestor(target, "ag-cell");
        if (clickedCell) {
            clickedCell.focus();
        }
    };

    private rowClicked = (params: any) => {
            var self = this;

            self.clickTimeout = self.$timeout(function () {
                if (self.clickTimeout.$$state.status === 2) {
                    return; // click event canceled by double-click
                }

            self.rowFocus(params.event.target);

            if (typeof self.onRowClick === `function`) {
                self.onRowClick({prms: params});
        }
        }, 250);
    };

    private rowDoubleClicked = (params: any) => {
        // this is just to cancel the (single) click event in case of double-click
        this.$timeout.cancel(this.clickTimeout);

        if (typeof this.onRowDblClick === `function`) {
            this.onRowDblClick({prms: params});
        }
    };

    private rowPostCreate = (params: any) => {
        if (angular.isFunction(this.onRowPostCreate)) {
            this.onRowPostCreate({prms: params});
        } else {
            if (this.enableDragndrop) {
                let node = params.node;
                let path = node.childIndex;
                while (node.level) {
                    node = node.parent;
                    path = node.childIndex + "/" + path;
                }
                let row = angular.element(params.eRow)[0];
                row.setAttribute("bp-tree-dragndrop", path);
            }
        }
    };
}