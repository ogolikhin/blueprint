import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IDialogOptions, BaseDialogController} from "./dialog.svc";
import * as Grid from "ag-grid/main";
import "ag-grid-enterprise/main";

export class OpenProjectController extends BaseDialogController {

    private getProjectUrl: string = "svc/adminstore/instance/folders/";
    public hasCloseButton: boolean = true;
    private rowData: any = null;
    public selectedItem: any = {};

    static $inject = ["$scope", "localization", "$uibModalInstance", "$http",  "params"];
    /* tslint:disable */
    constructor(private $scope: ng.IScope, private localization: ILocalizationService, $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private $http: ng.IHttpService, params: IDialogOptions) {
        /* tslint:enable */
        super($uibModalInstance, params);
    };


    //Dialog return valuereturned value
    public get returnvalue(): any {
        return {
            Id: this.selectedItem.Id || -1,
            Name: this.selectedItem.Name || "--empty--"
        };
    };

    private columnDefinitions = [{
        headerName: this.localization.get("App_Header_Name"),
        field: "Name",
        cellRenderer: {
            renderer: "group",
            innerRenderer: (params) => {
                if (params.data.Type === "Project") {
                    return "<i class='fonticon-project'></i>" + params.data.Name;
                } else {
                    return params.data.Name;
                }
            },
        },
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering : true
        }];

    private rowClicked = (params: any) => {
        var self = this;
        var node = params.node;
        if (node.data.disabled) {
            return;
        }
        if (node.data.children && !node.data.children.length) {
            if (node.expanded) {
                if (node.allChildrenCount === 0) {
                    self.$http.get(self.getProjectUrl + "/" + node.data.Id + "/children")
                        .then(function (res) {
                            node.data.children = res.data;
                            node.open = true;
                            self.gridOptions.api.setRowData(self.rowData);
                        });
                }
            }
        }
        node.data.open = node.expanded;
        self.$scope.$applyAsync((s) => self.selectedItem = node.data);
    };

    private getNodeChildDetails(rowItem) {
        if (rowItem.children) {
            return {
                group: true,
                expanded: rowItem.open,
                children: rowItem.children || [],
                field: "Name",
                // the key is used by the default group cellRenderer
                key: rowItem.Id
            };
        } else {
            return null;
        }
    }

    private onGidReady = (params: any) => {
        var self = this;
        params.api.setHeaderHeight(0);
        params.api.sizeColumnsToFit();
        self.$http.get(self.getProjectUrl + "/1/" + "/children")
            .then(function (res) {
                angular.forEach(res.data, (v) => {
                    if (v.Type === "Folder") {
                        v.children = [];
                    }
                });

                self.rowData = res.data;
                self.gridOptions.api.setRowData(self.rowData);
            });
    };
    public gridOptions: Grid.GridOptions = {
        columnDefs: this.columnDefinitions,
        headerHeight: 30,
        icons: {
            groupExpanded: "<i class='fonticon-folder-open' />",
            groupContracted: "<i class='fonticon-folder' />"
        },
        suppressHorizontalScroll: true,
        rowBuffer: 200,
        rowHeight: 30,
        enableColResize: true,
        getNodeChildDetails: this.getNodeChildDetails,
        onRowClicked: this.rowClicked,
        onGridReady: this.onGidReady,
    };

}



