﻿import { IProjectManager, Models} from "../..";
import { Helper, IBPTreeController, ITreeNode } from "../../../shared";
import { ISelectionManager, ISelection, SelectionSource } from "./../../services/selection-manager";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./project-explorer.html");
    public controller: Function = ProjectExplorerController;
    public transclude: boolean = true;
}

export class ProjectExplorerController {
    public tree: IBPTreeController;
    private _selectedArtifactId: number;
    private _subscribers: Rx.IDisposable[]; 
    public static $inject: [string] = ["projectManager", "selectionManager"];
    constructor(
        private projectManager: IProjectManager,
        private selectionManager: ISelectionManager) { }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onLoadProject, this),
            //subscribe for current artifact change (need to distinct artifact)
            this.projectManager.currentArtifact.distinctUntilChanged().subscribeOnNext(this.onSelectArtifact, this),
        ];
    }
    
    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }


    // the object defines how data will map to ITreeNode
    // key: data property names, value: ITreeNode property names
    public propertyMap = {
        id: "id",
        itemTypeId: "type",
        name: "name",
        hasChildren: "hasChildren",
        artifacts: "children"
    }; 

    public columns = [{
        headerName: "",
        field: "name",
        cellClass: function (params) {
            let css: string[] = [];

            if (params.data.hasChildren) {
                css.push("has-children");
            }
            if (params.data.predefinedType === Models.ItemTypePredefined.PrimitiveFolder) {
                css.push("is-folder");
            } else if (params.data.predefinedType === Models.ItemTypePredefined.Project) {
                css.push("is-project");
            } else {
                css.push("is-" + Helper.toDashCase(Models.ItemTypePredefined[params.data.predefinedType]));
            }

            return css;
        },
        
        //cellClassRules: {
        //    "has-children": function (params) { return params.data.hasChildren; },
        //    "is-folder": function (params) { return params.data.predefinedType === Models.ItemTypePredefined.PrimitiveFolder; },
        //    "is-project": function (params) { return params.data.predefinedType === Models.ItemTypePredefined.Project; }
        //},
        cellRenderer: "group",
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering: true
    }];


    private onLoadProject = (projects: Models.IProject[]) => {
        //NOTE: this method is called during "$onInit" and as a part of "Rx.BehaviorSubject" initialization.
        // At this point the tree component (bp-tree) is not created yet due to component hierachy (dependant) 
        // so, just need to do an extra check if the component has created
        if (this.tree) {
            this.tree.reload(projects);
            if (angular.isDefined(this._selectedArtifactId)) {
                this.tree.selectNode(this._selectedArtifactId);
            }
            if (projects && projects.length > 0) {
                this.selectionManager.selection = this.createSelection(projects[0]);
            } else {
                this.selectionManager.clearSelection();
            }
        }
    }
    private onSelectArtifact = (artifact: Models.IArtifact) => {
        // so, just need to do an extra check if the component has created
        if (this.tree && artifact) {
            this._selectedArtifactId = artifact.id;
            this.tree.selectNode(this._selectedArtifactId);
        }
    }

    public doLoad = (prms: Models.IProject): any[] => {
        //the explorer must be empty on a first load
        //if (!prms) {
        //    return null;
        //}
        //notify the repository to load the node children
        this.projectManager.loadArtifact(prms as Models.IArtifact);
        return null;
    };


    public doSelect = (node: ITreeNode) => {
        //check passed in parameter
        const artifact = this.doSync(node);
        this.projectManager.setCurrentArtifact(artifact);

        this.selectionManager.selection = this.createSelection(artifact);

    };

    private createSelection(artifact: Models.IArtifact): ISelection {
        const project = artifact.id === artifact.projectId ? artifact : this.projectManager.getArtifact(artifact.projectId);
        return { source: SelectionSource.Explorer, project: project, artifact: artifact, subArtifact: null};
    }

    public doSync = (node: ITreeNode): Models.IArtifact => {
        //check passed in parameter
        let artifact = this.projectManager.getArtifact(node.id);
        if (artifact.hasChildren) {
            angular.extend(artifact, {
                loaded: node.loaded,
                open: node.open
            });
        };
        return artifact;
    };


}