import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IProjectNotification} from "../../services/project-notification";
import * as pSvc from "../../services/project.svc";
import {IMainViewController} from "../../main.view";

export class ProjectExplorerComponent implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public require: any = {
        mainView: "^bpMainView"
    };
    public transclude: boolean = true;
}

class ProjectExplorerController {
    public mainView: IMainViewController;

    private selectedItem: any;

    public static $inject: [string] = ["$scope", "localization", "projectService", "$element", "$log", "$timeout", "projectNotification"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private service: pSvc.IProjectService,
        private $element,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService,
        private notification: IProjectNotification) {
    }

    public $onInit = () => {
        this.notification.subscribeToOpenProject(function (evt, selected) {
            alert(`Project \"${selected.name} [ID:${selected.id}]\" is selected.`);
        });
    };

    public columns = [{
        headerName: "",
        field: "Name",
        cellClassRules: {
            "has-children": function (params) { return params.data.HasChildren; },
            "is-folder": function (params) { return params.data.Type === "Folder"; },
            "is-project": function (params) { return params.data.Type === "Project"; }
        },
        cellRenderer: "group",
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering: true
    }];

    public loadElements = (prms: any): ng.IPromise<any[]> => {
        //check passed in parameter
        return this.service.getFolders();
    };

    public expandGroup = (prms: any): ng.IPromise<any[]> => {
        //check passesd in parameter
        var id = (prms && prms.Id) ? prms.Id : null;
        return this.service.getFolders(id);
    };

    public selectElement = (item: any) => {
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
    };

    public doRowClick = (prms: any) => {
        var selectedNode = prms.node;
        var cell = prms.eventSource.eBodyRow.firstChild;
        if (cell.className.indexOf("ag-cell-inline-editing") === -1) {
            //console.log("clicked on row: I should load artifact [" + selectedNode.data.Id + ": " + selectedNode.data.Name + "]");
        }
    };
}
