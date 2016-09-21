﻿import { Models} from "../../models";
import { Helper, IBPTreeController, ITreeNode } from "../../../shared";
import { IProjectManager, IArtifactManager} from "../../../managers";
import { SelectionSource, IArtifactState } from "../../../managers/artifact-manager";
import { IStatefulArtifact, IArtifactNode} from "../../../managers/models";
import { INavigationService } from "../../../core/navigation/navigation.svc";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: Function = ProjectExplorerController;
    public transclude: boolean = true;
}

export class ProjectExplorerController {
    public tree: IBPTreeController;
    private _selectedArtifactId: number;
    private _subscribers: Rx.IDisposable[];

    public static $inject: [string] = ["projectManager", "artifactManager", "navigationService"];
    
    constructor(
        private projectManager: IProjectManager,
        private artifactManager: IArtifactManager,
        private navigationService: INavigationService
    ) { 
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onLoadProject, this),
            //subscribe for current artifact change (need to distinct artifact)
            // this.artifactManager.selection.artifactObservable.subscribeOnNext(this.onSelectArtifact, this)
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
        itemTypeId: "itemTypeId",
        name: "name",
        hasChildren: "hasChildren",
        parentNode: "parentNode",
        children: "children",
        loaded: "loaded",
        open: "open"
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
        
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                let icon = "<i ng-drag-handle></i>";
                let name = Helper.escapeHTMLText(params.data.name);
                let artifactType = (params.data as IArtifactNode).artifact.metadata.getItemType();
                if (artifactType && artifactType.iconImageId && angular.isNumber(artifactType.iconImageId)) {
                    icon = `<bp-item-type-icon
                                item-type-id="${artifactType.id}"
                                item-type-icon="${artifactType.iconImageId}"
                                ng-drag-handle></bp-item-type-icon>`;
                }
                return `${icon}<span>${name}</span>`;
            },
            padding: 20
        },
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
        }
    }

    private onSelectArtifact = (artifact: IStatefulArtifact) => {
        // so, just need to do an extra check if the component has created
        if (this.tree && artifact) {
            this._selectedArtifactId = artifact.id;
            this.tree.selectNode(this._selectedArtifactId);
        }
    }

    public doLoad = (prms: Models.IProject): any[] => {
        //the explorer must be empty on a first load
        if (!prms) {
            return null;
        }
        //notify the repository to load the node children
        this.projectManager.loadArtifact(prms.id);
        return null;
    };

    public doSelect = (node: ITreeNode) => {
        //check passed in parameter
        if (node && this._selectedArtifactId !== node.id) {
            this._selectedArtifactId = this.doSync(node).id;
            this.navigationService.navigateToArtifact(this._selectedArtifactId);
        }
    };

    public doSync = (node: ITreeNode): IStatefulArtifact => {
        //check passed in parameter
        let artifactNode = this.projectManager.getArtifactNode(node.id);
        if (artifactNode.children && artifactNode.children.length) {
            angular.extend(artifactNode, {
                loaded: node.loaded,
                open: node.open
            });
        };
        return artifactNode.artifact;
    };
}
