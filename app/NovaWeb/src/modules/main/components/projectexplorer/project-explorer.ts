import {IProjectManager, Models, SubscriptionEnum } from "../..";
import {IBPTreeController, ITreeNode} from "../../../core/widgets/bp-tree/bp-tree";

export class ProjectExplorerComponent implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public transclude: boolean = true;
}

export class ProjectExplorerController {
    public tree: IBPTreeController;
    
    public static $inject: [string] = ["projectManager"];
    constructor(private projectManager: IProjectManager) {
        this.projectManager.subscribe(SubscriptionEnum.ProjectLoaded, this.loadProject.bind(this));
        this.projectManager.subscribe(SubscriptionEnum.ProjectChildrenLoaded, this.loadProject.bind(this));
        this.projectManager.subscribe(SubscriptionEnum.ProjectClosed, this.closeProject.bind(this));
    }
    
    public $onDestroy() {
        //clear all Project Manager event subscription
        this.projectManager.unsubscribe(SubscriptionEnum.ProjectLoaded, this.loadProject.bind(this));
        this.projectManager.unsubscribe(SubscriptionEnum.ProjectChildrenLoaded, this.loadProject.bind(this));
        this.projectManager.unsubscribe(SubscriptionEnum.ProjectClosed, this.closeProject.bind(this));
    }


    // the object defines how data will map to ITreeNode
    // key: data property names, value: ITreeNode property names
    public propertyMap = {
        id: "id",
        typeId: "type",
        name: "name",
        hasChildren: "hasChildren",
        artifacts: "children"
    }; 

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


    private loadProject = (artifact: Models.IArtifact) => {
        artifact = angular.extend(artifact, {
            loaded: true,
            open: true
        });

        this.tree.reload(this.projectManager.ProjectCollection);
        this.tree.selectNode(artifact.id);
    }

    private closeProject(projects: Models.IProject[]) {
        this.tree.reload(this.projectManager.ProjectCollection);
    }

    public doLoad = (prms: any): any[] => {
        //the explorer must be empty on a first load
        if (!prms) {
            return null;
        }
        //check passesed in parameter
        let projectId = angular.isNumber(prms.projectId) ? prms.projectId : -1;
        let artifactId = angular.isNumber(prms.id) ? prms.id : -1;
        //notify the repository to load the node children
        this.projectManager.notify(SubscriptionEnum.ProjectChildrenLoad, projectId, artifactId);
    };


    public doSelect = (node: ITreeNode) => {
                //check passed in parameter
        this.doSync(node);
        let artifact = this.projectManager.getArtifact(node.id);

        this.projectManager.CurrentArtifact = artifact;
        console.log(`Selected node ${this.projectManager.CurrentArtifact.id}`);
    };

    public doSync = (node: ITreeNode) => {
        //check passed in parameter
        let artifact = this.projectManager.getArtifact(node.id);
        this.projectManager.updateArtifact(artifact, artifact.hasChildren ? {
            loaded: node.loaded,
            open: node.open
        } : null);
    };

}
