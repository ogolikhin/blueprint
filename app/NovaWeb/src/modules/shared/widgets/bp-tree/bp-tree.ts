import * as angular from "angular";
import * as Grid from "ag-grid/main";
import {ILocalizationService} from "../../../core";
import {RowNode} from "ag-grid/main";

/**
 * Usage:
 *
 * <bp-tree bp-ref="$ctrl.tree"
 *          grid-columns="$ctrl.columns"
 *          enable-editing-on="name"
 *          enable-dragndrop="true"
 *          property-map="$ctrl.propertyMap"
 *          data-source="$ctrl.datasource"
 *          on-load="$ctrl.doLoad(prms)"
 *          on-select="$ctrl.doSelect(item)"
 *          on-sync="$ctrl.doSync(item)">
 * </bp-tree>
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

    getSelectedNodeId: number;
    isEmpty: boolean;
    //to select a row in in ag-grid (by id)
    selectNode(id: number);
    nodeExists(id: number): boolean;
    getNodeData(id: number): Object;
    //to reload datasource with data passed, if id specified the data will be loaded to node's children collection
    reload(data?: any[], id?: number);
    showLoading();
    showNoRows();
    hideOverlays();
}

export class BPTreeController implements IBPTreeController {
    static $inject = ["localization", "$element"];
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
    private selectedRowNode: RowNode;

    private _innerRenderer: Function;

    constructor(private localization: ILocalizationService, private $element?) {
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
                    if (gridCol.cellRendererParams && angular.isFunction(gridCol.cellRendererParams.innerRenderer)) {
                        this._innerRenderer = gridCol.cellRendererParams.innerRenderer;
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
            suppressRowClickSelection: true,
            suppressMenuMainPanel: true,
            suppressMenuColumnPanel: true,
            rowSelection: "single",
            rowBuffer: this.rowBuffer,
            rowHeight: this.rowHeight,
            enableColResize: true,
            columnDefs: this.gridColumns,
            icons: {
                groupExpanded: "<i />",
                groupContracted: "<i />"
            },
            getNodeChildDetails: this.getNodeChildDetails,
            onCellClicked: this.cellClicked,
            onRowSelected: this.rowSelected,
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
        this.selectedRowNode = null;
        this.bpRef = null;
        //this.reload(null);
        this.options.api.destroy();
    };

    private mapData(data: any, propertyMap?: any): ITreeNode {
        propertyMap = propertyMap || this.propertyMap;

        if (!propertyMap) {
            return data;
        }

        let item = {} as ITreeNode;

        for (let property in data) {
            item[propertyMap[property] ? propertyMap[property] : property] = data[property];
        }

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

    public get isEmpty(): boolean {
        return !Boolean(this._datasource && this._datasource.length);
    }

    public get getSelectedNodeId(): number {
        return this.selectedRowNode ? this.selectedRowNode.data.id : null;
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
        this.options.api.getModel().forEachNode((it: RowNode) => {
            if (it.data.id === id) {
                it.setSelected(true, true);
            }
        });
        this.options.api.ensureNodeVisible((it: RowNode) => it.data.id === id);
    }

    public nodeExists(id: number): boolean {
        let found: boolean = false;
        this.options.api.getModel().forEachNode(function (it) {
            if (it.data.id === id) {
                found = true;
            }
        });

        return found;
    }

    public getNodeData(id: number): Object {
        let result: Object = null;
        this.options.api.getModel().forEachNode(function (it) {
            if (it.data.id === id) {
                result = it.data;
            }
        });
        return result;
    }

    //sets a new datasource or add a datasource to specific node  children collection
    public reload(data?: any[], nodeId?: number) {
        let nodes: ITreeNode[] = [];

        this._datasource = this._datasource || [];
        if (data) {
            nodes = data.map(function (it) {
                return this.mapData(it, this.propertyMap);
            }.bind(this)) as ITreeNode[];
        }

        if (nodeId) {
            const node = this.getNode(nodeId, this._datasource);
            if (node) {
                node.open = true;
                node.loaded = true;
                node.children = nodes;
            }
        } else {
            this._datasource = nodes;
        }

        this.options.api.setRowData(this._datasource);

        if (this.selectedRowNode) {
            this.options.api.forEachNode((node) => {
                if (node.data.id === this.selectedRowNode.data.id) {
                    node.setSelected(true, true);
                    this.selectedRowNode = node;
                }
            });
        }
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

    private updateViewport = (params?: any) => {
        const viewport = this.$element[0].querySelector(".ag-body-viewport") as HTMLElement;
        if (viewport && viewport.clientWidth) {
            this.options.columnApi.autoSizeColumns(this.options.columnDefs.map(columnDef => columnDef.field ));

            const container = viewport.querySelector(".ag-body-container") as HTMLElement;
            if (container && viewport.clientWidth > container.clientWidth) {
                this.options.api.sizeColumnsToFit();
            }
        }

        if (params && params.lastRow && parseInt(params.lastRow, 10) >= 0) { // the grid contains at least one item
            this.hideOverlays();
        }
    };

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
        if (params && params.api) {
            params.api.sizeColumnsToFit();
        }

        if (_.isFunction(this.onLoad)) {
            //this verifies and updates current node to inject children
            //NOTE: this method may update grid datasource using setDataSource method
            let nodes = this.onLoad({prms: null});
            if (_.isArray(nodes)) {
                //this.addNode(nodes);
                this.reload(nodes);
            }
        }
    };

    private rowGroupOpened = (params: any) => {
        let node = params.node;

        let row = this.$element[0].querySelector(`.ag-body .ag-body-viewport-wrapper .ag-row[row-id="${node.data.id}"]`);
        if (row) {
            row.classList.remove(node.expanded ? "ag-row-group-contracted" : "ag-row-group-expanded");
            row.classList.add(node.expanded ? "ag-row-group-expanded" : "ag-row-group-contracted");
        }

        if (node.data.hasChildren && !node.data.loaded) {
            if (node.expanded && _.isFunction(this.onLoad)) {
                if (row) {
                    row.classList.add("ag-row-loading");
                }

                let nodes = this.onLoad({prms: node.data});
                //this verifes and updates current node to inject children
                //NOTE:: this method may uppdate grid datasource using setDataSource method
                if (_.isArray(nodes)) {
                    this.reload(nodes, node.data.id); // pass nothing to just reload
                }
            }
        }

        node.data.open = node.expanded;

        if (_.isFunction(this.onSync)) {
            this.onSync({item: node.data});
        }
    };

    public rowSelected = (event: {node: RowNode}) => {
        const node = event.node;
        const isSelected = node.isSelected();

        if (isSelected) {
            if (!this.selectedRowNode || this.selectedRowNode.data.id !== node.data.id) {
                if (_.isFunction(this.onSelect)) {
                    this.onSelect({item: node.data});
                }
                this.selectedRowNode = node;
            }
        } else {
            if (this.selectedRowNode.data.id === node.data.id) {
                node.setSelected(true, true);
            }
        }
    };

    private cellClicked = (params: {event: MouseEvent, rowIndex: number}) => {
        let element = params.event.target as HTMLElement;
        while (element && element.parentElement && element.parentElement !== this.$element[0]) {
            if (element.classList.contains("ag-group-contracted") || element.classList.contains("ag-group-expanded")) {
                return; // exit if the user clicked on the arrow to expand/contract the folder
            }
            element = element.parentElement;
        }

        const model = this.options.api.getModel();
        const node = model.getRow(params.rowIndex);

        node.setSelected(true, true);
    };

    private rowPostCreate = (params: any) => {
        if (_.isFunction(this.onRowPostCreate)) {
            this.onRowPostCreate({prms: params});
        }
    };
}
