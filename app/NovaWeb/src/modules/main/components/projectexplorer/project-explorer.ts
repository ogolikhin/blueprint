import "angular";
import {ILocalizationService} from "../../../core/localization";
import * as Repository from "../../repositories/project-repository";
import {IMainViewController} from "../../main.view";

export class ProjectExplorerComponent implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public require: any = {
        mainView: "^bpMainView"
    };
    public transclude: boolean = true;
}

class BaseController {
    public handlers: Function[] = [];
    public $onDestroy = () => {
        if (this[`$scope`] && this.handlers.length) {
            this.handlers.map(function (handler) {
                this.$scope.$on("$destroy", handler);
            });
        }
    }
}

class ProjectExplorerController extends BaseController {
    private mainView: IMainViewController;
    private tree: any;
    
    private selectedItem: any;
    public static $inject: [string] = ["$scope", "localization", "projectRepository", "$log", "$timeout"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private repository: Repository.IProjectRepository,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService) {

        super();
    }

    public $onInit = () => {
        this.repository.Notificator.subscribe(Repository.SubscriptionEnum.ProjectLoaded, this.loadProject)
    };

    public loadProject = (data: Repository.Data.IProject) => {
        var project = this.repository.CurrentProject;
        this.tree.setDataSource([{
            Id: this.repository.CurrentProject.id,
            Type: `Project`,
            Name: this.repository.CurrentProject.name,
            Children: this.repository.CurrentProject.artifacts
        }]);
    }

    public datasource: any;

    public columns = [{
        headerName: "",
        field: "Name",
        cellClassRules: {
            "has-children": function (params) { return params.data.Type === "Folder" && params.data.HasChildren; },
            "is-project": function (params) { return params.data.Type === "Project"; }
        },
        cellRenderer: "group",
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering: true
    }];

    public loadElements = (prms: any): ng.IPromise<any[]> => {
        //check passed in parameter
        return this.repository.GetFolders();
    };

    public expandGroup = (prms: any): ng.IPromise<any[]> => {
        //check passesd in parameter
        var id = (prms && prms.Id) ? prms.Id : null;
        return this.repository.GetFolders(id);
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
