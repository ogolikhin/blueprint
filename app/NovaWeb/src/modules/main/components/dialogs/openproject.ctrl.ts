import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IDialogSettings, BaseDialogController, IDialogService} from "../../../services/dialog.svc";
import * as pSvc from "../../../services/project.svc";
import * as Grid from "ag-grid/main";

export interface IOpenProjectResult {
    id: number;
    name: string;
    description: string;
}

export class OpenProjectController extends BaseDialogController {

    public hasCloseButton: boolean = true;
    private rowData: any = null;
    private selectedItem: any;

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectService", "dialogService", "params"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private service: pSvc.IProjectService,
        private dialogService: IDialogService,
        params: IDialogSettings
    ) {
        super($uibModalInstance, params);
    };

    //Dialog return value
    public get returnvalue(): IOpenProjectResult {
        return <IOpenProjectResult>{
            id: (this.selectedItem && this.selectedItem["Id"]) || -1,
            name: (this.selectedItem && this.selectedItem["Name"]) || "",
            description: (this.selectedItem && this.selectedItem["Description"]) || ""
        };
    };

    //Temporary solution need
    private showError = (error: any) => {
        this.dialogService.alert(error.message).then(() => { this.cancel(); });
    };

    private columnDefinitions = [{
        headerName: this.localization.get("App_Header_Name"),
        field: "Name",
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                if (params.data.Type === "Project") {
                    return "<i class='fonticon-project'></i>" + params.data.Name;
                } else {
                    return params.data.Name;
                }
            }
        },
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering : true
    }];

    private rowClicked = (params: any) => {
        var self = this;
        var node = params.node;
        if (node.data.Children && !node.data.Children.length && !node.data.alreadyLoadedFromServer) {
            if (node.expanded) {
                self.service.getFolders(node.data.Id)
                    .then((data: pSvc.IProjectNode[]) => {
                        node.data.Children = data;
                        node.data.open = true;
                        node.data.alreadyLoadedFromServer = true;
                        self.gridOptions.api.setRowData(self.rowData);
                    }, (error) => {
                        self.showError(error);
                    });
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
        params.api.sizeColumnsToFit();
        params.columnApi.autoSizeColumns(["Name"]);
        self.service.getFolders()
            .then((data: pSvc.IProjectNode[]) => {
                self.gridOptions.api.setRowData(self.rowData = data);
            }, (error) => {
                self.showError(error);
            });
    };

    private onRowGroupOpened = (params: any) => {
        var self = this;
        console.log(self);
        console.log(params);
    };

    public gridOptions: Grid.GridOptions = {
        columnDefs: this.columnDefinitions,
        headerHeight: 20,
        icons: {
            groupExpanded: "<i class='fonticon-folder-open' />",
            groupContracted: "<i class='fonticon-folder' />"
        },
        suppressContextMenu: true,
        rowBuffer: 200,
        rowHeight: 20,
        enableColResize: true,
        getNodeChildDetails: this.getNodeChildDetails,
        onRowClicked: this.rowClicked,
        onRowDoubleClicked: this.rowClicked,
        onGridReady: this.onGidReady,
        onRowGroupOpened: this.onRowGroupOpened,
        showToolPanel: false
    };
}
