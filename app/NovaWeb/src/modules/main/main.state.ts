import "angular";
import {AuthenticationRequired} from "../shell";
import {ILocalizationService} from "../core/localization";
import * as pSvc from "../services/project.svc";
import * as Grid from "ag-grid/main";

config.$inject = ["$stateProvider", "$urlRouterProvider"];
export function config($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider): void {
    $urlRouterProvider.otherwise("/main");
    $stateProvider.state("main", new MainState());
}

class MainCtrl {
    private rowData: any = null;
    private selectedItem: any;

    public static $inject: [string] = ["$scope", "localization", "projectService", "$element", "$log"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private service: pSvc.IProjectService,
        private $element,
        private $log: ng.ILogService) {
    }

    //Temporary solution need
    private showError = (error: any) => {
        alert(error.message);//.then(() => { this.cancel(); });
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
        params.api.setHeaderHeight(0);
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
        headerHeight: 0,
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
        onGridReady: this.onGidReady,
        showToolPanel: false
    };
}

class MainState extends AuthenticationRequired implements ng.ui.IState {
    public url = "/main";

    public template = require("./main.html");

    public controller = MainCtrl;
    public controllerAs = "main";
}