import "angular";
import {ILocalizationService} from "../../../core/localization";
import * as pSvc from "../../../services/project.svc";
import {IMainViewController} from "../../main.view";

export class ProjectExplorerComponent implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public require: any = {
        parent: "^bpMainView"
    };
    public transclude: boolean = true;
}

class ProjectExplorerController {
    public parent: IMainViewController;

    private selectedItem: any;
    //private clickTimeout: any;

    public static $inject: [string] = ["$scope", "localization", "projectService", "$element", "$log", "$timeout"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private service: pSvc.IProjectService,
        private $element,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService) {
    }

    //private showError = (error: any) => {
    //    alert(error.message); //.then(() => { this.cancel(); });
    //};

    public stripHTMLTags = (stringToSanitize: string): string => {
        var stringSanitizer = window.document.createElement("DIV");
        stringSanitizer.innerHTML = stringToSanitize;
        return stringSanitizer.textContent || stringSanitizer.innerText || "";
    };

    public escapeHTMLText = (stringToEscape: string): string => {
        var stringEscaper = window.document.createElement("TEXTAREA");
        stringEscaper.textContent = stringToEscape;
        return stringEscaper.innerHTML;
    };

    public columns = [{
        headerName: "", //this.localization.get("App_Header_Name"),
        field: "Name",
        //editable: true, // we can't use ag-grid's editor as it doesn't work on folders and it gets activated by too many triggers
        //cellEditor: this.cellEditor,
        cellClassRules: {
            "has-children": function (params) { return params.data.Type === "Folder" && params.data.HasChildren; },
            "is-project": function (params) { return params.data.Type === "Project"; }
        },
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params: any) => {
                var currentValue = params.value;
                var formattedCurrentValue = "<span>" + this.escapeHTMLText(currentValue) + "</span>";
                return formattedCurrentValue;
            }
        },
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering: true
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
                //self.selectedItem.Description = this.$sce.trustAsHtml(description);
            }
        });
    }
}

