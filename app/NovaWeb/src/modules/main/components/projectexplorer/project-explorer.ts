import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IBPTreeController, ITreeNode} from "../../../core/widgets/bp-tree/bp-tree";
import * as Repository from "../../repositories/project-repository";

export class ProjectExplorerComponent implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public transclude: boolean = true;
}

class ProjectExplorerController {
    private tree: IBPTreeController;

    private selectedItem: any;
    public static $inject: [string] = ["$scope", "localization", "projectRepository", "$log", "$timeout"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private repository: Repository.IProjectRepository,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService) {

    }

    public $onInit = () => {
        this.repository.Notificator.subscribe(Repository.SubscriptionEnum.ProjectLoaded, this.loadProject.bind(this));
        this.repository.Notificator.subscribe(Repository.SubscriptionEnum.ProjectNodeLoaded, this.loadProjectNode.bind(this));
        this.repository.Notificator.subscribe(Repository.SubscriptionEnum.ProjectClosed, this.closeProject.bind(this));
        this.repository.Notificator.subscribe(Repository.SubscriptionEnum.CurrentProjectChanged, this.activateProject.bind(this));
    };

    private loadProject = (project: Repository.Data.IProject, alreadyOpened: boolean) => {
        if (alreadyOpened) {
            this.tree.selectNode(project.id);
            return;
        };
        this.tree.addNode(<ITreeNode>{
            id: project.id,
            type: `Project`,
            name: project.name,
            hasChildren: true,
            open: true
        }); 
        
        this.tree.setDataSource(project.artifacts, project.id);
    }

    private activateProject() {
        if (this.repository.CurrentProject) {
            this.tree.selectNode(this.repository.CurrentProject.id);
        }
    }

    public loadProjectNode = (project: Repository.Data.IProject, artifactId) => {
        var nodes = project.getArtifact(artifactId).artifacts;
        this.tree.setDataSource(nodes, artifactId);
    }

    public closeProject(projects: Repository.Data.IProject[]) {
        projects.map(function (it: Repository.Data.IProject) {
            this.tree.removeNode(it.id);
        }.bind(this)); 
        this.tree.setDataSource();
    }

    public columns = [{
        headerName: "",
        field: "name",
        cellClassRules: {
            "has-children": function (params) { return params.data.hasChildren; },
            "is-folder": function (params) { return params.data.type === "Folder"; },
            "is-project": function (params) { return params.data.type === "Project"; }
        },
        cellRenderer: "group",
        suppressMenu: true, 
        suppressSorting: true,
        suppressFiltering: true
    }];


    public doLoad = (prms: any): any[] => {
        //the explorer must be empty on a first load
        if (!prms) {
            return null;
        }
        //check passesed in parameter
        let artifactId = angular.isNumber(prms.id) ? prms.id : null;
        //notify the service to load the node children
        this.repository.Notificator.notify(Repository.SubscriptionEnum.ProjectNodeLoad, this.repository.CurrentProject.id, artifactId);
    };

    public selectElement = (item: any) => {
        //check passed in parameter
        this.$scope.$applyAsync((s) => {
            this.selectedItem = item;
            if (item.Description) {
                var description = item.description;
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
//        var selectedNode = prms.node;
        var cell = prms.eventSource.eBodyRow.firstChild;
        if (cell.className.indexOf("ag-cell-inline-editing") === -1) {
            //console.log("clicked on row: I should load artifact [" + selectedNode.data.id + ": " + selectedNode.data.name + "]");
        }
    };
}
