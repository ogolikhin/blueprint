//import "angular";
//import {Helper} from "../../../core/utils/helper";
import {IBPTreeController, ITreeNode} from "../../../core/widgets/bp-tree/bp-tree";
import {IProjectManager, Models, SubscriptionEnum } from "../../managers/project-manager";


export class ProjectExplorerComponent implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public transclude: boolean = true;
}

export class ProjectExplorerController {
    public tree: IBPTreeController;
    
    public static $inject: [string] = ["projectManager"];
    constructor(private manager: IProjectManager) {
        this.manager.subscribe(SubscriptionEnum.CurrentProjectChanged, this.activateProject.bind(this));
        this.manager.subscribe(SubscriptionEnum.ProjectLoaded, this.loadProject.bind(this));
        this.manager.subscribe(SubscriptionEnum.ProjectChildrenLoaded, this.loadProject.bind(this));
        this.manager.subscribe(SubscriptionEnum.ProjectClosed, this.closeProject.bind(this));
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

    private activateProject(project: Models.IProject) {
        if (project) {
            this.tree.selectNode(project.id);
        }
    }

    private loadProject = (artifact: Models.IArtifact) => {
        artifact = angular.extend(artifact, {
            loaded: true,
            open: true
        });
        this.tree.reload(this.manager.ProjectCollection);
    }

    public closeProject(projects: Models.IProject[]) {
        this.tree.reload(this.manager.ProjectCollection);
    }

    public columns = [{
        headerName: "",
        field: "name",
        cellClassRules: {
            "has-children": function (params) { return params.data.hasChildren; },
            "is-folder": function (params) { return params.data.predefinedType=== Models.ArtifactTypeEnum.Folder; },
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
        this.manager.selectArtifact(node.id);
    };

}
