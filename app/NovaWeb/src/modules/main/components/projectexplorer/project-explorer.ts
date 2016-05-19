import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IBPTreeController, ITreeNode} from "../../../core/widgets/bp-tree/bp-tree";
import * as $manager from "../../managers/project-manager";

export class ProjectExplorerComponent implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public transclude: boolean = true;
}

class ProjectExplorerController {
    private tree: IBPTreeController;

    private selectedItem: any;
    public static $inject: [string] = ["$scope", "localization", "projectManager", "$log", "$timeout"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private manager: $manager.IProjectManager,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService) {

    }

    public $onInit = () => {
        this.manager.subscribe($manager.SubscriptionEnum.CurrentProjectChanged, this.activateProject.bind(this));
        this.manager.subscribe($manager.SubscriptionEnum.ProjectLoaded, this.loadProject.bind(this));
        this.manager.subscribe($manager.SubscriptionEnum.ProjectChildrenLoaded, this.loadProjectChildren.bind(this));
        this.manager.subscribe($manager.SubscriptionEnum.ProjectClosed, this.closeProject.bind(this));
    };

    private activateProject() {
        if (this.manager.CurrentProject) {
            this.selectItem(this.manager.CurrentProject.id);
        }
    }

    private loadProject = (project: $manager.Models.IProject, alreadyOpened: boolean) => {
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

    private loadProjectChildren = (project: $manager.Models.IProject, artifactId) => {
        var nodes = project.getArtifact(artifactId).artifacts;
        this.tree.setDataSource(nodes, artifactId);
    }

    private closeProject(projects: $manager.Models.IProject[]) {
        projects.map(function (it: $manager.Models.IProject) {
            this.tree.removeNode(it.id);
        }.bind(this)); 
        this.tree.setDataSource();
    }

    private selectItem(id: number) {
        this.tree.selectNode(this.manager.CurrentProject.id);
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
        //notify the repository to load the node children
        this.manager.notify($manager.SubscriptionEnum.ProjectChildrenLoad, this.manager.CurrentProject.id, artifactId);
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        //this.$scope.$applyAsync((s) => {});
        this.selectItem(item);
    };

    public doRowClick = (prms: any) => {
//        var selectedNode = prms.node;
        var cell = prms.eventSource.eBodyRow.firstChild;
        if (cell.className.indexOf("ag-cell-inline-editing") === -1) {
            //console.log("clicked on row: I should load artifact [" + selectedNode.Models.id + ": " + selectedNode.Models.name + "]");
        }
    };
}
