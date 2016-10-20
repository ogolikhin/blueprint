import * as angular from "angular";
import {Models} from "../../models";
import {ItemTypePredefined} from "../../models/enums";
import {Helper, IBPTreeController} from "../../../shared";
import {IProjectManager, IArtifactManager} from "../../../managers";
import {Project} from "../../../managers/project-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager";
import {ISelectionManager} from "../../../managers/selection-manager";
import {IArtifactNode} from "../../../managers/project-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = ProjectExplorerController;
    public transclude: boolean = true;
}

export class ProjectExplorerController {
    public tree: IBPTreeController;
    private subscribers: Rx.IDisposable[];
    private selectedArtifactSubscriber: Rx.IDisposable;
    private numberOfProjectsOnLastLoad: number;
    private selectedArtifactNameBeforeChange: string;

    public static $inject: [string] = [
        "projectManager", 
        "artifactManager", 
        "navigationService",
        "selectionManager"
    ];

    constructor(private projectManager: IProjectManager,
                private artifactManager: IArtifactManager,
                private navigationService: INavigationService,
                private selectionManager: ISelectionManager) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onLoadProject, this),
            this.selectionManager.artifactObservable.filter(artifact => !!artifact).subscribeOnNext(this.setSelectedNode, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        if (this.selectedArtifactSubscriber) {
            this.selectedArtifactSubscriber.dispose();
        }
    }

    private setSelectedNode(artifact: IStatefulArtifact) {
        if (this.tree.nodeExists(artifact.id)) {
            console.log("setting selection here");
            this.tree.selectNode(artifact.id);
        }
    }

    private _selected: IArtifactNode;
    private get selected() {
        return this._selected;
    }

    private set selected(value: IArtifactNode) {
        this._selected = value;
        this.selectedArtifactNameBeforeChange = value.name;
        //Dispose of old subscriber and subscribe to new artifact.
        if (this.selectedArtifactSubscriber) {
            this.selectedArtifactSubscriber.dispose();
        }
        this.selectedArtifactSubscriber = value.artifact.getObservable().subscribeOnNext(this.onSelectedArtifactChange);
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
            const typeName = Models.ItemTypePredefined[params.data.predefinedType];
            if (typeName) {
                css.push("is-" + Helper.toDashCase(typeName));
            }
            return css;
        },

        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                let icon = "<i ng-drag-handle></i>";
                let name = Helper.escapeHTMLText(params.data.name);
                let artifactType = (params.data as IArtifactNode).artifact.metadata.getItemTypeTemp();
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
                if (!this.selected || this.numberOfProjectsOnLastLoad !== projects.length) {
                    this.selected = projects[0];
                    this.navigationService.navigateTo(this.selected.id);
                }

                //if node exists in the tree
                if (this.tree.nodeExists(this.selected.id)) {
                    this.tree.selectNode(this.selected.id);
                    this.navigationService.navigateTo(this.selected.id);

                    //replace with a new object from tree, since the selected object may be stale after refresh
                    let selectedObjectInTree: IArtifactNode = <IArtifactNode>this.tree.getNodeData(this.selected.id);
                    if (selectedObjectInTree) {
                        this.selected = selectedObjectInTree;
                    }
                } else {
                    //otherwise, if parent node is in the tree
                    if (this.selected.parentNode && this.tree.nodeExists(this.selected.parentNode.id)) {
                        this.tree.selectNode(this.selected.parentNode.id);
                        this.navigationService.navigateTo(this.selected.parentNode.id);

                        //replace with a new object from tree, since the selected object may be stale after refresh
                        let selectedObjectInTree: IArtifactNode = <IArtifactNode>this.tree.getNodeData(this.selected.parentNode.id);
                        if (selectedObjectInTree) {
                            this.selected = selectedObjectInTree;
                        }
                    } else {
                        //otherwise, try with project node
                        if (this.tree.nodeExists(this.selected.projectId)) {
                            this.tree.selectNode(this.selected.projectId);
                            this.navigationService.navigateTo(this.selected.projectId);
                        } else {
                            //if project node fails too - give up
                            this.artifactManager.selection.setExplorerArtifact(null);
                            this.navigationService.navigateToMain();
                        }
                    }
                }
            } else {
                this.artifactManager.selection.setExplorerArtifact(null);
                this.navigationService.navigateToMain();
            }
            this.numberOfProjectsOnLastLoad = projects.length;
        }
    }

    public onSelectedArtifactChange = (artifact: IStatefulArtifact) => {
        //If the artifact's name changes (on refresh), we reload the project so the change is reflected in the explorer.
        if (artifact.name !== this.selectedArtifactNameBeforeChange) {
            this.onLoadProject(this.projectManager.projectCollection.getValue());
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
        this.doSync(node);
        this.selected = node;
        this.tree.selectNode(node.id);
        this.navigationService.navigateTo(node.id);
    };

    public doSync = (node: IArtifactNode): IStatefulArtifact => {
        //check passed in parameter
        let artifactNode = this.projectManager.getArtifactNode(node.id);

        if (artifactNode.children && artifactNode.children.length) {
            angular.extend(artifactNode, {
                loaded: node.loaded,
                open: node.open
            });
        }
        ;

        return artifactNode.artifact;
    };
}
