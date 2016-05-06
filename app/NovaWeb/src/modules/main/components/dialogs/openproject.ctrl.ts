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

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectService", "dialogService", "params", "$sce"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private service: pSvc.IProjectService,
        private dialogService: IDialogService,
        params: IDialogSettings,
        private $sce: ng.ISCEService
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

    public stripHTMLTags = (stringToSanitize: string): string => {
        var stringSanitizer = window.document.createElement("DIV");
        stringSanitizer.innerHTML = stringToSanitize;
        return stringSanitizer.textContent || stringSanitizer.innerText || "";
    };

    public escapeHTMLText = (stringToEscape: string): string =>  {
        var stringEscaper = window.document.createElement("TEXTAREA");
        stringEscaper.textContent = stringToEscape;
        return stringEscaper.innerHTML;
    };

    private onEnterKeyOnProject = (e: any) => {
        var key = e.which || e.keyCode;
        if (key === 13) {
            //user pressed Enter key on project
            this.ok();
        }
    };

    private columnDefinitions = [{
        headerName: this.localization.get("App_Header_Name"),
        field: "Name",
        cellClassRules: {
            "has-children": function(params) { return params.data.Type === "Folder" && params.data.HasChildren; },
            "is-project": function(params) { return params.data.Type === "Project"; }
        },
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                var sanitizedName = this.escapeHTMLText(params.data.Name);

                if (params.data.Type === "Project") {
                    var cell = params.eGridCell;
                    cell.addEventListener("keydown", this.onEnterKeyOnProject);
                }
                return sanitizedName;
            }
        },
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering : true
    }];

    private rowGroupOpened = (params: any) => {
        var self = this;
        var node = params.node;
        if (node.data.Type === "Folder") {
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
        }
    };

    private cellFocused = (params: any) => {
        var self = this;
        var rowModel = self.gridOptions.api.getModel();
        var rowsToSelect = rowModel.getRow(params.rowIndex);
        rowsToSelect.setSelected(true, true);
        self.$scope.$applyAsync((s) => {
            self.selectedItem = rowsToSelect.data;
            if (rowsToSelect.data.Description) {
                var description = rowsToSelect.data.Description;
                var virtualDiv = window.document.createElement("DIV");
                virtualDiv.innerHTML = description;
                var aTags = virtualDiv.querySelectorAll("a");
                for (var a = 0; a < aTags.length; a++) {
                    aTags[a].setAttribute("target", "_blank");
                }
                description = virtualDiv.innerHTML;
                self.selectedItem.Description = this.$sce.trustAsHtml(description);
            }
        });
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
        onCellFocused: this.cellFocused,
        onRowGroupOpened: this.rowGroupOpened,
        onGridReady: this.onGidReady,
        showToolPanel: false
    };
}
