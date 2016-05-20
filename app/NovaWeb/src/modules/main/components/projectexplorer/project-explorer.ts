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
    public propertyMap = {
        id: "id",
        type: "type",
        name: "name",
        hasChildren: "hasChildren",
        artifacts: "children"
    };

    private activateProject() {
        if (this.manager.CurrentProject) {
            this.selectArtifact(this.manager.CurrentProject.id);
        }
    }

    private loadProject = (project: Models.IProject, alreadyOpened: boolean) => {
        if (alreadyOpened) {
            this.selectArtifact(project.id);
            return;
        };

        this.tree.addNode([
            angular.extend({
                type: 0,
                hasChildren: true,
                open: true
            }, project)
        ], 0, {
                id: "id",
                type: "type",
                name: "name",
                hasChildren: "hasChildren",
                artifacts: "children"
            }); 
        
        this.tree.setDataSource();
    }

    private loadProjectChildren = (project: Models.IProject, artifactId) => {
        var nodes = project.getArtifact(artifactId).artifacts;

        this.tree.addNodeChildren(project.id, project.artifacts)

        this.tree.setDataSource();
    }

    private closeProject(projects: Models.IProject[]) {
        projects.map(function (it: Models.IProject) {
            this.tree.removeNode(it.id);
        }.bind(this)); 
        this.tree.setDataSource();
    }

    private selectArtifact(artifact:any) {
        let selectedId = this.manager.selectArtifact(artifact);
        if (selectedId) {
            this.tree.selectNode(selectedId);
        }
        
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
        this.selectArtifact(item);
    };

}
