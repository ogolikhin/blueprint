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
        this.projectManager.projectCollection.asObservable().subscribeOnNext(this.onLoadProject, this);
        this.projectManager.currentArtifact.asObservable().subscribeOnNext(this.onSelectArtifact, this);
    }
    public $onInit(o) { }

    public $onDestroy() { }


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


    private onLoadProject = (projects: Models.IProject[]) => {
        if (this.tree) {
            this.tree.reload(projects);
        }
    }
    private onSelectArtifact = (artifact: Models.IArtifact) => {
        if (this.tree && artifact) {
            this.tree.selectNode(artifact.id);
        }
    }

    private closeProject = (projects: Models.IProject[]) => {
        if (this.tree) {
            this.tree.reload(projects);
        }
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
        this.projectManager.loadArtifact(prms as Models.IArtifact);
    };


    public doSelect = (node: ITreeNode) => {
        //check passed in parameter
        this.projectManager.currentArtifact.onNext(this.doSync(node));
    };

    public doSync = (node: ITreeNode): Models.IArtifact => {
        //check passed in parameter
        let artifact = this.projectManager.getArtifact(node.id);
        this.projectManager.updateArtifact(artifact, artifact.hasChildren ? {
            loaded: node.loaded,
            open: node.open
        } : null);
        return artifact;
    };

}
