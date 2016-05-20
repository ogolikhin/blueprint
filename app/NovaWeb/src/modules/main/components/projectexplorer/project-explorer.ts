import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IBPTreeController, ITreeNode} from "../../../core/widgets/bp-tree/bp-tree";
import {IProjectManager, Models, SubscriptionEnum } from "../../managers/project-manager";

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
        private manager: IProjectManager,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService) {

    }

    public $onInit = () => {
        this.manager.subscribe(SubscriptionEnum.CurrentProjectChanged, this.activateProject.bind(this));
        this.manager.subscribe(SubscriptionEnum.ProjectLoaded, this.loadProject.bind(this));
        this.manager.subscribe(SubscriptionEnum.ProjectChildrenLoaded, this.loadProjectChildren.bind(this));
        this.manager.subscribe(SubscriptionEnum.ProjectClosed, this.closeProject.bind(this));
    };

    private activateProject() {
        if (this.manager.CurrentProject) {
            this.selectItem(this.manager.CurrentProject.id);
        }
    }

    private loadProject = (project: Models.IProject, alreadyOpened: boolean) => {
        if (alreadyOpened) {
            this.selectItem(project.id);
            return;
        }
        this.tree.addNode(<ITreeNode>{
            id: project.id,
            type: 1,
            name: project.name,
            hasChildren: true,
            open: true
        });

        this.tree.setDataSource(project.children, project.id);
    };

    private loadProjectChildren = (project: Models.IProject, artifactId) => {
        var nodes = project.getArtifact(artifactId).children;
        this.tree.setDataSource(nodes, artifactId);
    };

    private closeProject(projects: Models.IProject[]) {
        projects.map(function (it: Models.IProject) {
            this.tree.removeNode(it.id);
        }.bind(this));
        this.tree.setDataSource();
    }

    private selectItem(id: number) {
        this.tree.selectNode(id);
    }

    public columns = [{
        headerName: "",
        field: "name",
        cellClassRules: {
            "has-children": function (params) { return params.data.hasChildren; },
            "is-folder": function (params) { return params.data.type === Models.ArtifactTypeEnum.Folder; },
            "is-project": function (params) { return params.data.type === Models.ArtifactTypeEnum.Project; }
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
        this.manager.notify(SubscriptionEnum.ProjectChildrenLoad, this.manager.CurrentProject.id, artifactId);
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        //this.$scope.$applyAsync((s) => {});
        this.selectItem(item);
    };
}
