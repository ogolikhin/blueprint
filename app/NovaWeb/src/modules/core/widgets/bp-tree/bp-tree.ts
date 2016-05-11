//import {ILocalizationService} from "../../../core/localization";
import * as Grid from "ag-grid/main";

/*
tslint:disable
*/ /*
Sample template. See following parameters:
<bp-tree 
    grid-class="project_explorer" - wrapper css class name
    grid-columns="$ctrl.columns"  - column definition
    on-load="$ctrl.doLoad(prms)"  - gets data to load tree root nodes
    on-expand="$ctrl.doExpand(prms)" - gets data to load a sub-tree (child node)
    on-select="$ctrl.doSelect(item)"> - to be called then a not is selected
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
        //settings
        gridColumns: "<",
        //events
        onLoad: "&",
        onExpand: "&",
        onSelect: "&"
    };
}

export class BPTreeController  {
    static $inject = ["$element"];
    //properties
    public gridClass: string = "project-explorer";
    //settings
    public gridColumns: any[];
    public onLoad: Function;
    public onSelect: Function;
    public onExpand: Function;

    public options: Grid.GridOptions;
    private rowData: any = null;
    private selectedRow: any;

    constructor(private $element) {
    }

    public $onInit = () => {
        this.options = <Grid.GridOptions>{
            headerHeight: 20,
            showToolPanel: false,
            suppressContextMenu: true,
            rowBuffer: 200,
            rowHeight: 20,
            enableColResize: true,
            editable: true,
            columnDefs: this.gridColumns || [],
            icons: {
                groupExpanded: "<i class='fonticon-folder-open' />",
                groupContracted: "<i class='fonticon-folder' />"
            },
            getNodeChildDetails: this.getNodeChildDetails,
            onCellFocused: this.cellFocused,
            onRowGroupOpened: this.rowGroupOpened,
            onGridReady: this.onGridReady
        };
    };

    private getNodeChildDetails(rowItem) {
        if (rowItem.Children) {
            return {
                group: true,
                expanded: rowItem.open,
                children: rowItem.Children,
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
        if (params && params.columnApi) {
            params.columnApi.autoSizeColumns(["Name"]);
        }
        if (typeof self.onLoad === "function") {
            self.onLoad({ prms: self.options }).then((data: any) => {
                self.options.api.setRowData(self.rowData = data);
            }, (error) => {
                //                self.showError(error);
            });
        }
    };

    private cellFocused = (params: any) => {
        var model = this.options.api.getModel();
        this.selectedRow = model.getRow(params.rowIndex);
        this.selectedRow.setSelected(true, true);
        this.onSelect({item: this.selectedRow.data});
    };

    private rowGroupOpened = (params: any) => {
        let self = this;
        let node = params.node;
        if (node.data.Type === `Folder`) {
            if (typeof self.onExpand === `function`) {
                if (node.data.Children && !node.data.Children.length && !node.data.alreadyLoadedFromServer) {
                    if (node.expanded) {

                        self.onExpand({ prms: node.data })
                            .then((data: any) => { //pSvc.IProjectNode[]
                                node.data.Children = data;
                                node.data.open = true;
                                node.data.alreadyLoadedFromServer = true;
                                self.options.api.setRowData(self.rowData);
                            }, (error) => {
                                //self.showError(error);
                            });
                    }
                }
                node.data.open = node.expanded;
            }
        }
    };
}