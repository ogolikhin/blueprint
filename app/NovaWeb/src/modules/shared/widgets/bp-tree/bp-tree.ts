import "angular";
import * as Grid from "ag-grid/main";
import { ILocalizationService } from "../../../core";
import { RowNode } from "ag-grid/main";

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
    public controller: ng.Injectable<ng.IControllerConstructor> = BPTreeController;
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
        propertyMap: "<",
        //to set connection with parent
        bpRef: "=?",
        //events
        onLoad: "&?",
        onSelect: "&?",
        onSync: "&?",
        onRowClick: "&?",
        onRowDblClick: "&?",
        onRowPostCreate: "&?"
    };
}
export interface ITreeNode {
    id: number;
    name: string;
    itemTypeId: number;
    hasChildren: boolean;
    parentNode?: ITreeNode;
    children?: ITreeNode[];
    loaded?: boolean;
    open?: boolean;
}
export interface IBPTreeController {
    onLoad?: Function;                  //to be called to load ag-grid data a data node to the datasource
    onSelect?: Function;                //to be called on time of ag-grid row selection
    onSync?: Function;                //to be called on time of ag-grid row selection
    onRowClick?: Function;
    onRowDblClick?: Function;
    onRowPostCreate?: Function;

    isEmpty: boolean;
    //to select a row in in ag-grid (by id)
    selectNode(id: number);                    
    //to reload datasource with data passed, if id specified the data will be loaded to node's children collection
    reload(data?: any[], id?: number);
    showLoading();
    showNoRows();
    hideOverlays();
}


export class BPTreeController implements IBPTreeController  {
    static $inject = ["localization", "$element", "$timeout"];
    //properties
    public gridClass: string;
    public enableEditingOn: string;
    public enableDragndrop: boolean;
    public rowBuffer: number;
    public rowHeight: number;
    public headerHeight: number;
    //settings
    public gridColumns: any[];
    public propertyMap: any;

    //events
    public onLoad: Function;
    public onSelect: Function;
    public onSync: Function;
    public onRowClick: Function;
    public onRowDblClick: Function;
    public onRowPostCreate: Function;
   

    public bpRef: BPTreeController;

    public options: Grid.GridOptions;
    private editableColumns: string[] = [];
    private _datasource: any[] = [];
    private selectedRow: any;
    private clickTimeout: any;
    private selectionSubject: Rx.Subject<RowNode>;

    private _innerRenderer: Function;

    constructor(private localization: ILocalizationService, private $element?, private $timeout?: ng.ITimeoutService) {
        this.bpRef = this;

        this.selectionSubject = new Rx.Subject<RowNode>();
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
                    if (gridCol.cellRendererParams && angular.isFunction(gridCol.cellRendererParams.innerRenderer)) {
                        this._innerRenderer = gridCol.cellRendererParams.innerRenderer;
                        gridCol.cellRendererParams.innerRenderer = this.innerRenderer;
                    }
                }
            }.bind(this));
        } else {
            this.gridColumns = [];
        }

        this.selectionSubject
            .filter(node => node != null && angular.isFunction(this.onSelect))
            .distinctUntilChanged(node => node.id)
            .debounce(200)
            .subscribe(node => this.onSelect({ item: node.data }));
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
                groupExpanded: "<i />",
                groupContracted: "<i />"
            },
            getNodeChildDetails: this.getNodeChildDetails,
            onCellFocused: this.cellFocused,
            onRowClicked: this.rowClicked,
            onRowDoubleClicked: this.rowDoubleClicked,
            onRowGroupOpened: this.rowGroupOpened,
            processRowPostCreate: this.rowPostCreate,
            onGridReady: this.onGridReady,
            getBusinessKeyForNode: this.getBusinessKeyForNode,
            onViewportChanged: this.updateViewport,
            onModelUpdated: this.updateViewport,
            localeTextFunc: (key: string, defaultValue: string) => this.localization.get("ag-Grid_" + key, defaultValue)

        };
    };  
    

    public $onDestroy = () => {
        this.selectedRow = null;
        this.bpRef = null;
        //this.reload(null);
        this.updateViewport(null, true);
        this.options.api.destroy();
        this.selectionSubject.dispose();
    };

    /* tslint:disable */
    private mapData(data: any, propertyMap?: any): ITreeNode {
        propertyMap = propertyMap || this.propertyMap;
        if (!propertyMap) {
            return data;
        }
        let item = {} as ITreeNode;

         for (let property in data) {
             item[propertyMap[property] ? propertyMap[property] : property ] = data[property];
         }
        // for (let property in propertyMap) {
        //     item[property] = data[property];
        // }
        if (item.hasChildren) {
            if (angular.isArray(item.children) && item.children.length) {
                item.children = item.children.map(function (it) {
                    return this.mapData(it, propertyMap);
                }.bind(this)) as ITreeNode[];
            } else {
                item.children = [];
            }
        }
        return item;
    }
    /* tslint:enable */

    public get isEmpty(): boolean {
        return !Boolean(this._datasource && this._datasource.length);
    }


    private getNode(id: number, nodes?: ITreeNode[]): ITreeNode {
        let item: ITreeNode;
        if (nodes) {
            nodes.map(function (node: ITreeNode) {
                if (!item && node.id === id) {  ///needs to be changed toCamelCase
                    item = node;
                } else if (!item && node.children) {
                    item = this.getNode(id, node.children);
                }
            }.bind(this));
        }
        return item;
    };

    //to select a tree node in ag grid
    public selectNode(id: number) {
        this.options.api.getModel().forEachNode(function (it) {
            it.setSelected(it.data.id === id, true);
        });
    }


    //sets a new datasource or add a datasource to specific node  children collection
    public reload(data?: any[], nodeId?: number) {
        let nodes: ITreeNode[] = [];

        this._datasource = this._datasource || [];
        if (data) {
            nodes = data.map(function (it) {
                return this.mapData(it, this.propertyMap) ;
            }.bind(this)) as ITreeNode[];
        }
        if (nodeId) {
            let node = this.getNode(nodeId, this._datasource);
            if (node) {
                node = angular.extend(node, {
                    open: true,
                    loaded: true,
                    children: nodes
                });
            }

        } else {
            this._datasource = nodes;
        }

        //HACk: have to clear cell selection
        this.options.api.setFocusedCell(-1, this.gridColumns[0].field);

        this.options.api.setRowData(this._datasource);
    }

    public showLoading = () => {
        this.options.api.showLoadingOverlay();
    };

    public showNoRows = () => {
        this.options.api.showNoRowsOverlay();
    };

    public hideOverlays = () => {
        this.options.api.hideOverlay();
    };

    private updateViewport = (params?: any, remove?: boolean) => {
        if (params && params.lastRow && parseInt(params.lastRow, 10) >= 0) { // the grid contains at least one item
            this.hideOverlays();
        }

        let viewport = this.$element[0].querySelector(".ag-body-viewport");
        if (viewport && !angular.isUndefined((<any>window).PerfectScrollbar)) {
            if (remove) {
                (<any>window).PerfectScrollbar.destroy(viewport);
            } else {
                if (viewport.getAttribute("data-ps-id")) {
                    // perfect-scrollbar has been initialized on the element (data-ps-id is not null/undefined/"" )
                    let allColumnIds = [];
                    this.options.columnDefs.forEach(function(columnDef) {
                        allColumnIds.push(columnDef.field);
                    });
                    this.options.columnApi.autoSizeColumns(allColumnIds);
                    (<any>window).PerfectScrollbar.update(viewport);
                } else {
                    (<any>window).PerfectScrollbar.initialize(viewport, {
                        minScrollbarLength: 20,
                        scrollXMarginOffset: 4,
                        scrollYMarginOffset: 4
                    });
                }
            }
        }
    };
    /* tslint:disable */
    private innerRenderer = (params: any) => {
        let inlineEditing = this.editableColumns.indexOf(params.colDef.field) !== -1 ? `bp-tree-inline-editing="` + params.colDef.field + `"` : "";

        let enableDragndrop: string;
        if (this.enableDragndrop) {
            let node = params.node;
            let path = node.childIndex;
            while (node.level) {
                node = node.parent;
                path = node.childIndex + "/" + path;
            }
            enableDragndrop = ` bp-tree-dragndrop="${path}"`;
        } else {
            enableDragndrop = "";
        }

        let currentValue = this._innerRenderer(params) || params.value;
        return `<span class="ag-group-value-wrapper" ${inlineEditing}${enableDragndrop}>${currentValue}</span>`;
    };
    /* tslint:enable */
    private getNodeChildDetails(node: ITreeNode) {
        if (node.children) {
            return {
                group: true,
                expanded: node.open,
                children: node.children,
                field: "name",
                key: node.id // the key is used by the default group cellRenderer
            };
        } else {
            return null;
        }
    };

    private getBusinessKeyForNode(node: RowNode) {
        return node.data.id;
        //return node.key; //it is initially undefined for non folder???
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
                //this.addNode(nodes);
                this.reload(nodes);
            }
        }
    };

    private rowGroupOpened = (params: any) => {
        console.log("rowGroupOpened");
        let self = this;

        let node = params.node;
        let row = self.$element[0].querySelector(`.ag-body .ag-body-viewport-wrapper .ag-row[row-id="${node.data.id}"]`);
        if (row) {
            if (node.expanded) {
                row.classList.remove("ag-row-group-contracted");
                row.classList.add("ag-row-group-expanded");
            } else {
                row.classList.remove("ag-row-group-expanded");
                row.classList.add("ag-row-group-contracted");
            }
        }
        if (node.data.hasChildren && !node.data.loaded) {
            if (angular.isFunction(self.onLoad)) {
                if (row) {
                    row.classList.add("ag-row-loading");
                }
                let nodes = self.onLoad({ prms: node.data });
                //this verifes and updates current node to inject children
                //NOTE:: this method may uppdate grid datasource using setDataSource method
                if (angular.isArray(nodes)) {
                    this.reload(nodes, node.data.id); // pass nothing to just reload 
                }
            }
        }
        node.data.open = node.expanded;
        if (angular.isFunction(self.onSync)) {
            self.onSync({ item: node.data });
        }
    };

    private rowSelected = (node: RowNode) => {
        if (!node) {
            return;
        }

        node.setSelected(true, true);
        this.selectionSubject.onNext(node);
    };

    private cellFocused = (params: any) => {
        var model = this.options.api.getModel();
        let selectedRow = model.getRow(params.rowIndex);
        this.rowSelected(selectedRow);
    };
    
    private rowClicked = (params: any) => {
        let self = this;

        this.clickTimeout = this.$timeout(function () {
            if (self.clickTimeout.$$state.status === 2) {
                return; // click event canceled by double-click
            }

        }, 250);
    };

    private rowDoubleClicked = (params: any) => {
        // this is just to cancel the (single) click event in case of double-click
        this.$timeout.cancel(this.clickTimeout);

        if (angular.isFunction(this.onRowDblClick)) {
            this.onRowDblClick({prms: params});
        }
    };

    private rowPostCreate = (params: any) => {
        if (angular.isFunction(this.onRowPostCreate)) {
            this.onRowPostCreate({prms: params});
        }
    };
}
