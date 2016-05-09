//import {ILocalizationService} from "../../../core/localization";
import * as Grid from "ag-grid/main";

export interface IBpTreeController {
}

export class BPTree implements ng.IComponentOptions {
    public template: string = require("./bp-tree.html");
    public controller: Function = BPTreeController;
    public bindings: any = {
        columns: `@`,
        onReady: `&`,
        onSelected: `&`,
        onExpand: `&`,
    };

    public transclude: boolean = true;
    public require = "^parent";
}

export class BPTreeController implements IBpTreeController {
    static $inject = ["$element"];

    public options: Grid.GridOptions ;
    public columns: any[] = [];
    public onReady: Function;
    public onSelect: Function;
    public onExpand: Function;


    constructor(private $element ) {
        // the accordionId is needed in case multiple accordions are present in the same page
        this.options = <Grid.GridOptions> {
            headerHeight: 20,
            showToolPanel: false,
            suppressContextMenu: true,
            rowBuffer: 200,
            rowHeight: 20,
            enableColResize: true,

            columnDefs: this.columns,
            icons: {
                groupExpanded: "<i class='fonticon-folder-open' />",
                groupContracted: "<i class='fonticon-folder' />"
            },
            getNodeChildDetails: this.getNodeChildDetails,
            onCellFocused: this.cellFocused,
            onRowGroupOpened: this.rowGroupOpened,
            onGridReady: this.onGidReady
        };

    }
    public $onInit = () => {
        //this.options.getNodeChildDetails = this.NodeChildDetails;
        //this.
    };
    private onGidReady = (params: any) => {
        var self = this;
        params.api.sizeColumnsToFit();
        params.columnApi.autoSizeColumns(["Name"]);
        this.onReady(self.options.api);
        //self.service.getFolders()
        //    .then((data: pSvc.IProjectNode[]) => {
        //        self.gridOptions.api.setRowData(self.rowData = data);
        //    }, (error) => {
        //        self.showError(error);
        //    });
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

    private cellFocused = (params: any) => {
        var self = this;
        var rowModel = self.options.api.getModel();
        var rowsToSelect = rowModel.getRow(params.rowIndex);
        rowsToSelect.setSelected(true, true);
        //this.selected = {
        //    id: (rowsToSelect && rowsToSelect["Id"]) || -1,
        //    name: (rowsToSelect && rowsToSelect["Name"]) || "",
        //    description: (rowsToSelect && rowsToSelect["Description"]) || ""
        //}
        this.onSelect(rowsToSelect);
        //self.$scope.$applyAsync((s) => {
        //    self.selectedItem = rowsToSelect.data;
        //    if (rowsToSelect.data.Description) {
        //        var description = rowsToSelect.data.Description;
        //        var virtualDiv = window.document.createElement("DIV");
        //        virtualDiv.innerHTML = description;
        //        var aTags = virtualDiv.querySelectorAll("a");
        //        for (var a = 0; a < aTags.length; a++) {
        //            aTags[a].setAttribute("target", "_blank");
        //        }
        //        description = virtualDiv.innerHTML;
        //        self.selectedItem.Description = this.$sce.trustAsHtml(description);
        //    }
        //});
    };


    private rowGroupOpened = (params: any) => {
//        var self = this;
        var node = params.node;
        this.onExpand(node);
        //if (node.data.Type === "Folder") {
        //    if (node.data.Children && !node.data.Children.length && !node.data.alreadyLoadedFromServer) {
        //        if (node.expanded) {
        //            self.service.getFolders(node.data.Id)
        //                .then((data: pSvc.IProjectNode[]) => {
        //                    node.data.Children = data;
        //                    node.data.open = true;
        //                    node.data.alreadyLoadedFromServer = true;
        //                    self.gridOptions.api.setRowData(self.rowData);
        //                }, (error) => {
        //                    self.showError(error);
        //                });
        //        }
        //    }
        //    node.data.open = node.expanded;
        //}
    };
}