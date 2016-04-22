import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IDialogSettings, BaseDialogController, IDialogService} from "../../../services/dialog.svc";
import * as pSvc from "../../../services/project.svc";
import * as Grid from "ag-grid/main";

export class OpenProjectController extends BaseDialogController {

    public hasCloseButton: boolean = true;
    private rowData: any = null;
    public selectedItem: any = {};

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectService", "dialogService", "params" ];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private service: pSvc.IProjectService,
        private dialogService: IDialogService,
        params: IDialogSettings) {
        super($uibModalInstance, params);
    };


    //Dialog return valuereturned value
    public get returnvalue(): any {
        return {
            Id: this.selectedItem.Id || -1,
            Name: this.selectedItem.Name || ""
        };
    };

    private showError(error: any) {
        this.dialogService.alert(error.message).then(() => { this.cancel(); });
    }

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
        if (node.data.Children && !node.data.Children.length) {
            if (node.expanded) {
                if (node.allChildrenCount === 0) {
                    self.service.getFolders(node.data.Id)
                        .then((data: pSvc.IProjectNode[]) => {
                            node.data.Children = data;
                            node.open = true;
                            self.gridOptions.api.setRowData(self.rowData);
                        }, (error) => {
                            self.showError(error);
                        });
                }
            }
        }
        node.data.open = node.expanded;
        self.$scope.$applyAsync((s) => self.selectedItem = node.data);
    };

    private getNodeChildDetails(rowItem) {
        if (rowItem.Children) {
            return {
                group: true,
                expanded: rowItem.open,
                children: rowItem.Children ,
                field: "Name",
                key: rowItem.Id // the key is used by the default group cellRenderer
            };
        } else {
            return null;
        }
    }

    private onGidReady = (params: any) => {
        var self = this;
        params.api.setHeaderHeight(10);
        params.api.sizeColumnsToFit();
        self.service.getFolders()
            .then((data: pSvc.IProjectNode[]) => {
                self.gridOptions.api.setRowData(self.rowData = data);
            }, (error) => {
                self.showError(error);
            });
    };
    public gridOptions: Grid.GridOptions = {
        columnDefs: this.columnDefinitions,
        headerHeight: 20,
        icons: {
            groupExpanded: "<i class='fonticon-folder-open' />",
            groupContracted: "<i class='fonticon-folder' />"
        },
        suppressHorizontalScroll: true,
        suppressContextMenu: true,
        rowBuffer: 200,
        rowHeight: 20,
        enableColResize: true,
        getNodeChildDetails: this.getNodeChildDetails,
        onRowClicked: this.rowClicked,
        onGridReady: this.onGidReady,
        showToolPanel: false
    };

}



