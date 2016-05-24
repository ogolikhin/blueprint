﻿import "angular";
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
    // the object defines how data will map to ITreeNode
    // on left
    public propertyMap = {
        id: "id",
        typeId: "type",
        name: "name",
        hasChildren: "hasChildren",
        artifacts: "children"
    }; 

    private activateProject(project: Models.IProject) {
        this.tree.selectNode(project.id);
    }

    private loadProject = (project: Models.IProject) => {
        this.tree.addNode([ 
            angular.extend({
                predefinedType: Models.ArtifactTypeEnum.Project,
                hasChildren: true,
                loaded: true,
                open: true
            }, project)
        ]); 
        
        this.tree.setDataSource();
    }

    private loadProjectChildren = (artifact: Models.IArtifact) => {
        this.tree.addNodeChildren(artifact.id, artifact.artifacts);
        this.tree.setDataSource();
    }

    private closeProject(projects: Models.IProject[]) {
        projects.map(function (it: Models.IProject) {
            this.tree.removeNode(it.id);
        }.bind(this)); 
        this.tree.setDataSource();
    }


    public columns = [{
        headerName: "",
        field: "name",
        cellClassRules: {
            "has-children": function (params) { return params.data.hasChildren; },
            "is-folder": function (params) { return params.data.predefinedType === Models.ArtifactTypeEnum.Folder; },
            "is-project": function (params) { return params.data.predefinedType === Models.ArtifactTypeEnum.Project; }
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

    public doSelect = (node: ITreeNode) => {
        //check passed in parameter
        //this.$scope.$applyAsync((s) => {});
        this.manager.selectArtifact(node.id);
    };

}
