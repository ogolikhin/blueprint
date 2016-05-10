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

    //public stripHTMLTags = (stringToSanitize: string): string => {
    //    var stringSanitizer = window.document.createElement("DIV");
    //    stringSanitizer.innerHTML = stringToSanitize;
    //    return stringSanitizer.textContent || stringSanitizer.innerText || "";
    //};

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
    
    public columns = [{
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

    public doLoad = (prms: any): ng.IPromise<any[]> => {
        //check passed in parameter
        return this.service.getFolders();
    };

    public doExpand = (prms: any): ng.IPromise<any[]> => {
        //check passesd in parameter
        var id = (prms && prms.Id) ? prms.Id : null;
        return this.service.getFolders(id);
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        this.$scope.$applyAsync((s) => {
            this.selectedItem = item;
            if (item.Description) {
                var description = item.Description;
                var virtualDiv = window.document.createElement("DIV");
                virtualDiv.innerHTML = description;
                var aTags = virtualDiv.querySelectorAll("a");
                for (var a = 0; a < aTags.length; a++) {
                    aTags[a].setAttribute("target", "_blank");
                }
                description = virtualDiv.innerHTML;
                this.selectedItem.Description = this.$sce.trustAsHtml(description);
            }
        });
    }
}
