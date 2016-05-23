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
        
        propertyMap: "<",

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
    name: string;
    type: number;
    hasChildren: boolean;
    parentNode?: ITreeNode;
    children?: ITreeNode[];
    loaded?: boolean;
    open?: boolean;
}
export interface IBPTreeController {
    onLoad?: Function;                  //to be called to load ag-grid data a data node to the datasource
    onSelect?: Function;                //to be called on time of ag-grid row selection
    onRowClick?: Function;
    onRowDblClick?: Function;
    onRowPostCreate?: Function;

    addNode(data: any, index?: number, propertyMap?: any); //to add a data node to the datasource
    addNodeChildren(id:number, data: any[], propertyMap?: any); //to add a data node to the datasource
    removeNode(id: number);                     //to remove a data node (by id) from the datasource
    selectNode(id: number);                     //to select a row in in ag-grid (by id)
    setDataSource(data?: any[]);     //
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
    public propertyMap: any;

    //events
    public onLoad: Function;
    public onSelect: Function;
    public onRowClick: Function;
    public onRowDblClick: Function;
    public onRowPostCreate: Function;

    public bpRef: BPTreeController;

    public options: Grid.GridOptions;
    private editableColumns: string[] = [];
    private _datasource: any[] = [];
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

    private mapData(data: any, propertyMap?: any): ITreeNode {
        propertyMap = propertyMap || this.propertyMap;
        if (!this.propertyMap) {
            return data; 
        }
        let item = {} as ITreeNode;

        for (let property in data) {
            item[this.propertyMap[property] ? this.propertyMap[property] : property ] = data[property];
        }
        if (item.hasChildren) {
            if (angular.isArray(item.children) && item.children.length) {
                item.children = item.children.map(function (it) {
                    return this.mapData(it, propertyMap);
                }.bind(this)) as ITreeNode[];
            } else {
                item.children = [];
            }
        };
        
        return item;
    }

    //to add a data node to the datasource
    public addNode(data: any[], index: number = 0, propertyMap?: any)  {
        this._datasource = this._datasource || [];

        data = data.map(function (it) {
            it = this.mapData(it, propertyMap);
            return it;
        }.bind(this));
        
        this._datasource.splice.apply(this._datasource, [index < 0 ? 0 : index, 0].concat(data));
        
    }
    public addNodeChildren(nodeId: number, data: any[], propertyMap?: any) {
        let node = this.getNode(nodeId, this._datasource);
        if (node) {
            data = data.map(function (it) {
                it = this.mapData(it, propertyMap);
                //set parent node reference
                it.parentNode = node;
                return it;
            }.bind(this));
            node.children = data;
            node.loaded = true;
            node.open = true;
        }
        
    }

    //to remove a data node (by id) from the datasource
    public removeNode(id: number) {
        let node = this.getNode(id, this._datasource || []);
        if (node) {
            if (node.parentNode) {
                node.parentNode.children = node.parentNode.children.filter(function (it: ITreeNode) {
                    return it.id !== id;
                });
            } else {
                this._datasource = this._datasource.filter(function (it: ITreeNode) {
                    return it.id !== id;
                });
            }
        }
        this.options.api.clearRangeSelection();
    }

    //to select a tree node in ag grid
    public selectNode(id: number) {
        this.options.api.getModel().forEachNode(function (it) {
            if (it.data.id === id) {
                it.setSelected(true, true);            } 
        });
    }

    //sets a new datasource or add a datasource to specific node  children collection
    public setDataSource(data?: any[]) {
        if (data) {
            this._datasource = data.map(function (it) {
                return this.mapData(it);
            }.bind(this))
        }
        this.options.api.setRowData(this._datasource);
    }

    private getNode(id: number, nodes?: ITreeNode[]): ITreeNode {
        let item: ITreeNode;
        if (nodes) {
            nodes.map(function (node: ITreeNode) {
                if (!item && node.id === id) {  ///needs to be changed camelCase 
                    item = node;
                } else if (!item && node.children) {
                    item = this.getNode(id, node.children);
                }
            }.bind(this));
        }
        return item;
    };

    private innerRenderer = (params: any) => {
        var currentValue = params.value;
        var inlineEditing = this.editableColumns.indexOf(params.colDef.field) !== -1 ? "bp-tree-inline-editing " : "";
        var cancelDragndrop = this.enableDragndrop ? "ng-cancel-drag" : "";

        return `<span ${inlineEditing}${cancelDragndrop}>${Helper.escapeHTMLText(currentValue)}</span>`;
    };

    private getNodeChildDetails(rowItem) {
        if (rowItem.children) {
            return {
                group: true,
                expanded: rowItem.open,
                children: rowItem.children,
                field: "name",
                key: rowItem.id // the key is used by the default group cellRenderer
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
        if (angular.isFunction(this.onSelect)) {
            this.onSelect({item: this.selectedRow.data});
        }
    };

    private rowFocus = (target: any) => {
        var clickedCell = Helper.findAncestorByCssClass(target, "ag-cell");
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

            if (angular.isFunction(self.onRowClick)) {
                self.onRowClick({prms: params});
            } else {
                params.node.setSelected(true, true);
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