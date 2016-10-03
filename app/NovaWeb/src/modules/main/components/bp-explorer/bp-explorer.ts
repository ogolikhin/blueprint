import * as angular from "angular";
import { Models} from "../../models";
import { Helper, IBPTreeController } from "../../../shared";
import { IProjectManager, IArtifactManager} from "../../../managers";
import { Project } from "../../../managers/project-manager";
import { IStatefulArtifact } from "../../../managers/artifact-manager";
import { IArtifactNode } from "../../../managers/project-manager";
import { INavigationService } from "../../../core/navigation/navigation.svc";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = ProjectExplorerController;
    public transclude: boolean = true;
}

export class ProjectExplorerController {
    public tree: IBPTreeController;
    private selected: IArtifactNode;
    private subscribers: Rx.IDisposable[];

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
        this.subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onLoadProject, this),
        ];
    }
    
    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
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

    private onLoadProject = (projects: Project[]) => {
        //NOTE: this method is called during "$onInit" and as a part of "Rx.BehaviorSubject" initialization.
        // At this point the tree component (bp-tree) is not created yet due to component hierachy (dependant) 
        // so, just need to do an extra check if the component has created
        if (this.tree) {
            
            this.tree.reload(projects);

            if (projects && projects.length > 0) {
                if (!this.selected || this.selected.projectId !== projects[0].projectId) {
                    this.selected = projects[0];
                    this.navigationService.navigateToArtifact(this.selected.id);
                }

                if (this.tree.nodeExists(this.selected.id)) {
                    this.tree.selectNode(this.selected.id);
                    //this.navigationService.navigateToArtifact(this.selected.id);
                } else {
                    if (this.selected.parentNode && this.tree.nodeExists(this.selected.parentNode.id)) {
                        this.tree.selectNode(this.selected.parentNode.id);
                        this.navigationService.navigateToArtifact(this.selected.parentNode.id);
                    } else {
                        if (this.tree.nodeExists(this.selected.projectId)) {
                            this.tree.selectNode(this.selected.projectId);
                            this.navigationService.navigateToArtifact(this.selected.projectId);
                        } else {
                            this.artifactManager.selection.setExplorerArtifact(null);
                            this.navigationService.navigateToMain();
                        }
                    }
                }
            } else {
                this.artifactManager.selection.setExplorerArtifact(null);
                this.navigationService.navigateToMain();
            }
        }
    }

    public doLoad = (prms: Models.IProject): any[] => {
        //the explorer must be empty on a first load
        if (prms) {
            //notify the repository to load the node children
            this.projectManager.loadArtifact(prms.id);
        }
        
        return null;
    };

    public doSelect = (node: IArtifactNode) => {
        if (!this.selected || this.selected.id !== node.id || this.selected.id !== this.artifactManager.selection.getArtifact().id) {
            this.doSync(node);
            this.selected = node;
            this.tree.selectNode(node.id);
            this.navigationService.navigateToArtifact(node.id);
        }
    };

    public doSync = (node: IArtifactNode): IStatefulArtifact => {
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
