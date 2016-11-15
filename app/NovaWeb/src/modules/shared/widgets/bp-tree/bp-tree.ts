import * as agGrid from "ag-grid/main";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {IArtifactNode} from "../../../managers/project-manager";

/**
 * Usage:
 *
 * <bp-tree api="$ctrl.tree"
 *          root-nodes="$ctrl.projects"
 *          grid-columns="$ctrl.columns"
 *          enable-editing-on="name"
 *          enable-dragndrop="true"
 *          on-select="$ctrl.doSelect(item)"
 *          on-error="$ctrl.onError(reason)"
 *          on-grid-reset="$ctrl.onGridReset()">
 * </bp-tree>
 */

export class BPTreeComponent implements ng.IComponentOptions {
    public template: string = require("./bp-tree.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPTreeController;
    public bindings: any = {
        // Two-way
        api: "=?",
        // Input
        gridClass: "@",
        enableEditingOn: "@",
        enableDragndrop: "<",
        rowHeight: "<",
        rowBuffer: "<",
        headerHeight: "<",
        rootNodes: "<",
        gridColumns: "<",
        // Output
        onSelect: "&?",
        onError: "&?",
        onGridReset: "&?"
    };
}

export interface IBPTreeController {
    // BPTreeComponent bindings
    api: IBPTreeControllerApi;
    gridClass: string;
    enableEditingOn: string;
    enableDragndrop: boolean;
    rowBuffer: number;
    rowHeight: number;
    headerHeight: number;
    rootNodes: IArtifactNode[];
    gridColumns: any[];
    onSelect?: Function;                //to be called on time of ag-grid row selection
    onError: (param: {reason: any}) => void;
    onGridReset: () => void;
}

export interface IBPTreeControllerApi {
    getSelectedNodeId(): number;
    //to select a row in in ag-grid (by id)
    selectNode(id: number);
    deselectAll();
    nodeExists(id: number): boolean;
    getNodeData(id: number): Object;
    refresh(id?: number);
}

export class BPTreeController implements IBPTreeController {
    static $inject = ["$q", "localization", "$element"];

    // BPTreeViewComponent bindings
    public gridClass: string;
    public enableEditingOn: string;
    public enableDragndrop: boolean;
    public rowBuffer: number;
    public rowHeight: number;
    public headerHeight: number;
    public rootNodes: IArtifactNode[] = [];
    public gridColumns: any[];
    public onSelect: Function;
    public onError: (param: {reason: any}) => void;
    public onGridReset: () => void;

    // ag-grid bindings
    public options: agGrid.GridOptions;

    private editableColumns: string[] = [];
    private _datasource: any[] = [];
    private selectedRowNode: agGrid.RowNode;

    private _innerRenderer: Function;

    constructor(private $q: ng.IQService, private localization: ILocalizationService, private $element?) {
        this.gridClass = this.gridClass ? this.gridClass : "project-explorer";
        this.enableDragndrop = this.enableDragndrop ? true : false;
        this.rowBuffer = this.rowBuffer ? this.rowBuffer : 200;
        this.rowHeight = this.rowHeight ? this.rowHeight : 24;
        this.headerHeight = this.headerHeight ? this.headerHeight : 0;
        this.editableColumns = this.enableEditingOn && this.enableEditingOn !== "" ? this.enableEditingOn.split(",") : [];

        if (_.isArray(this.gridColumns)) {
            this.gridColumns.map(function (gridCol) {
                // if we are grouping and the caller doesn't provide the innerRenderer, we use the default one
                if (gridCol.cellRenderer === "group") {
                    if (gridCol.cellRendererParams && _.isFunction(gridCol.cellRendererParams.innerRenderer)) {
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
        this.options = <agGrid.GridOptions>{
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
            onGridReady: this.onGridReady,
            getBusinessKeyForNode: this.getBusinessKeyForNode,
            onViewportChanged: this.updateViewport,
            onModelUpdated: this.updateViewport,
            localeTextFunc: (key: string, defaultValue: string) => this.localization.get("ag-Grid_" + key, defaultValue)
        };
    };

    public $onChanges(onChangesObj: ng.IOnChangesObject): void {
        if (onChangesObj["rootNodes"]) {
            this.resetGridAsync();
        }
    }

    public $onDestroy = () => {
        this.selectedRowNode = null;
        this.api = null;
        //this.reload(null);
        this.options.api.destroy();
    };

    public api: IBPTreeControllerApi = {
        getSelectedNodeId: () => {
            return this.selectedRowNode ? this.selectedRowNode.data.id : null;
        },

        //to select a tree node in ag grid
        selectNode: (id: number) => {
            this.options.api.getModel().forEachNode((it: agGrid.RowNode) => {
                if (it.data.id === id) {
                    it.setSelected(true, true);
                }
            });
            this.options.api.ensureNodeVisible((it: agGrid.RowNode) => it.data.id === id);
        },

        deselectAll: () => {
            const selectedNodes = this.options.api.getSelectedNodes() as agGrid.RowNode[];
            if (selectedNodes && selectedNodes.length) {
                selectedNodes.map(node => {
                    node.setSelected(false);
                });
                this.clearFocus();
            }
        },

        nodeExists: (id: number) => {
            let found: boolean = false;
            this.options.api.getModel().forEachNode(function (it) {
                if (it.data.id === id) {
                    found = true;
                }
            });

            return found;
        },

        getNodeData: (id: number) => {
            let result: Object = null;
            this.options.api.getModel().forEachNode(function (it) {
                if (it.data.id === id) {
                    result = it.data;
                }
            });
            return result;
        },

        refresh: (id?: number) => {
            if (id) {
                let nodes = [];
                this.options.api.getModel().forEachNode(function (node) {
                    if (node.data.id === id) {
                        nodes.push(node);
                    }
                });
                this.options.api.refreshRows(nodes);
            } else {
                this.options.api.refreshView();
            }
        }
    };

    private resetGridAsync(): ng.IPromise<void> {
        if (this.options.api) {
            return this.$q.all(this.rootNodes.filter(n => n.open && !n.loaded).map(n => n.loadChildrenAsync())).then(() => {
                if (this.options.api) {
                    this.options.api.setRowData(this.rootNodes);

                    if (this.selectedRowNode) {
                        this.options.api.forEachNode(node => {
                            const vm = node.data as IArtifactNode;
                            if (vm.id === this.selectedRowNode.data.id) {
                                node.setSelected(true, true);
                            }
                        });
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
                    this.onGridReset();
                }
            });
        }

        return this.$q.resolve();
    }

    private getNode(id: number, nodes?: IArtifactNode[]): IArtifactNode {
        let item: IArtifactNode;

        if (nodes) {
            nodes.map(function (node: IArtifactNode) {
                if (!item && node.id === id) {  ///needs to be changed toCamelCase
                    item = node;
                } else if (!item && node.children) {
                    item = this.getNode(id, node.children);
                }
            }.bind(this));
        }

        return item;
    };

    private clearFocus() {
        this.options.api.setFocusedCell(-1, this.gridColumns[0].field);
    }

    private updateViewport = (params?: any) => {
        const viewport = this.$element[0].querySelector(".ag-body-viewport") as HTMLElement;
        if (viewport && viewport.clientWidth) {
            this.options.columnApi.autoSizeColumns(this.options.columnDefs.map(columnDef => columnDef.field ));

            const container = viewport.querySelector(".ag-body-container") as HTMLElement;
            if (container && viewport.clientWidth > container.clientWidth) {
                this.options.api.sizeColumnsToFit();
            }
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

    // Callbacks

    private getNodeChildDetails(node: IArtifactNode) {
        if (node.hasChildren) {
            return {
                group: true,
                expanded: node.open,
                children: node.children || [],
                field: "name",
                key: node.id // the key is used by the default group cellRenderer
            };
        } else {
            return null;
        }
    };

    private getBusinessKeyForNode(node: agGrid.RowNode) {
        return node.data.id;
        //return node.key; //it is initially undefined for non folder???
    };

    // Event handlers

    private onGridReady = (params: any) => {
        if (params && params.api) {
            params.api.sizeColumnsToFit();
        }
    };

    private rowGroupOpened = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const vm = node.data as IArtifactNode;

        if (vm.hasChildren) {
            const row = this.$element[0].querySelector(`.ag-body .ag-body-viewport-wrapper .ag-row[row-id="${vm.id}"]`);
            if (row) {
                row.classList.remove(node.expanded ? "ag-row-group-contracted" : "ag-row-group-expanded");
                row.classList.add(node.expanded ? "ag-row-group-expanded" : "ag-row-group-contracted");
            }
            if (node.expanded && !vm.loaded && _.isFunction(vm.loadChildrenAsync)) {
                if (row) {
                    row.classList.add("ag-row-loading");
                }
                vm.loadChildrenAsync().then(() => this.resetGridAsync()).catch(reason => {
                    if (_.isFunction(this.onError)) {
                        this.onError({reason: reason});
                    }
                });
            }
        }
        vm.open = node.expanded;
    };

    private rowSelected = (event: {node: agGrid.RowNode}) => {
        const node = event.node;
        const isSelected = node.isSelected();

        if (isSelected) {
            if (!this.selectedRowNode || this.selectedRowNode.data.id !== node.data.id) {
                this.selectedRowNode = node;
                this.clearFocus();
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

        if (_.isFunction(this.onSelect)) {
            this.onSelect({item: node.data});
        }
    };
}
