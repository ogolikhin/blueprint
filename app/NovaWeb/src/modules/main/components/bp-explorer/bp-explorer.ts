import * as angular from "angular";
import {Models} from "../../models";
import {ItemTypePredefined} from "../../models/enums";
import {Helper, IBPTreeControllerApi} from "../../../shared";
import {IProjectManager, IArtifactManager} from "../../../managers";
import {Project} from "../../../managers/project-manager";
import {IStatefulArtifact, IItemChangeSet} from "../../../managers/artifact-manager";
import {ISelectionManager} from "../../../managers/selection-manager";
import {IArtifactNode} from "../../../managers/project-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = ProjectExplorerController;
    public transclude: boolean = true;
}

export interface IProjectExplorerController {
    // BpTree bindings
    tree: IBPTreeControllerApi;
    columns: any[];
    propertyMap: {[key: string]: string};
    doLoad: Function;
    doSelect: Function;
    doSync: Function;
}

export class ProjectExplorerController implements IProjectExplorerController {
    private subscribers: Rx.IDisposable[];
    private selectedArtifactSubscriber: Rx.IDisposable;
    private numberOfProjectsOnLastLoad: number;
    private selectedArtifactNameBeforeChange: string;
    private isFullReLoad: boolean;

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
        this.isFullReLoad = true;
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for project collection update
            this.projectManager.projectCollection.subscribeOnNext(this.onLoadProject, this),
            this.selectionManager.explorerArtifactObservable.filter(artifact => !!artifact).subscribeOnNext(this.setSelectedNode, this)
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
            this.tree.selectNode(artifact.id);
        } else {
            this.tree.clearSelection();
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
        this.selectedArtifactSubscriber = value.artifact.getProperyObservable()
                        .distinctUntilChanged(changes => changes.item && changes.item.name)
                        .subscribeOnNext(this.onSelectedArtifactChange);


    }

    private onLoadProject = (projects: Project[]) => {
        //NOTE: this method is called during "$onInit" and as a part of "Rx.BehaviorSubject" initialization.
        // At this point the tree component (bp-tree) is not created yet due to component hierachy (dependant)
        // so, just need to do an extra check if the component has created
        if (this.tree) {

            this.tree.reload(projects);

            let currentSelection = this.selected ? this.selected.id : undefined;
            let navigateToId: number;
            if (projects && projects.length > 0) {
                if (!this.selected || this.numberOfProjectsOnLastLoad !== projects.length) {
                    this.selected = projects[0];
                    navigateToId = this.selected.id;
                }

                if (this.tree.nodeExists(this.selected.id)) {
                    //if node exists in the tree
                    if (this.isFullReLoad || this.selected.id !== this.tree.getSelectedNodeId()) {
                        this.tree.selectNode(this.selected.id);
                        navigateToId = this.selected.id;
                    }
                    this.isFullReLoad = true;

                    //replace with a new object from tree, since the selected object may be stale after refresh
                    let selectedObjectInTree: IArtifactNode = <IArtifactNode>this.tree.getNodeData(this.selected.id);
                    if (selectedObjectInTree) {
                        this.selected = selectedObjectInTree;
                    }
                } else {
                    //otherwise, if parent node is in the tree
                    if (this.selected.parentNode && this.tree.nodeExists(this.selected.parentNode.id)) {
                        this.tree.selectNode(this.selected.parentNode.id);
                        navigateToId = this.selected.parentNode.id;

                        //replace with a new object from tree, since the selected object may be stale after refresh
                        let selectedObjectInTree: IArtifactNode = <IArtifactNode>this.tree.getNodeData(this.selected.parentNode.id);
                        if (selectedObjectInTree) {
                            this.selected = selectedObjectInTree;
                        }
                    } else {
                        //otherwise, try with project node
                        if (this.tree.nodeExists(this.selected.projectId)) {
                            this.tree.selectNode(this.selected.projectId);
                            navigateToId = this.selected.projectId;
                        }
                    }
                }
            }

            this.numberOfProjectsOnLastLoad = projects.length;

            if (_.isFinite(navigateToId)) {
                if (navigateToId !== currentSelection) {
                    this.navigationService.navigateTo({ id: navigateToId });

                } else if (navigateToId === currentSelection) {
                    this.navigationService.reloadParentState();
                }
            }
        }
    };

    private onSelectedArtifactChange = (changes: IItemChangeSet) => {
        //If the artifact's name changes (on refresh), we refresh specific node only .
        //To prevent update treenode name while editing the artifact details, use it only for clean artifact.
        if (changes.item) {
            const node = this.tree.getNodeData(changes.item.id) as IArtifactNode;
            if (node) {
                node.name = changes.item.name;
                this.tree.refresh(node.id);
            }
        }
    };

    // BpTree bindings

    public tree: IBPTreeControllerApi;
    public columns = [{
        headerName: "",
        field: "name",
        cellClass: function (params) {
            let css: string[] = [];

            if (params.data.hasChildren) {
                css.push("has-children");
            }
            let typeName: string;
            if (params.data.predefinedType === Models.ItemTypePredefined.CollectionFolder && params.data.parentNode instanceof Project) {
                typeName = Models.ItemTypePredefined[Models.ItemTypePredefined.Collections];
            } else {
                typeName = Models.ItemTypePredefined[params.data.predefinedType];
            }
            if (typeName) {
                css.push("is-" + _.kebabCase(typeName));
            }
            return css;
        },

        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                let icon = "<i ng-drag-handle></i>";
                const name = Helper.escapeHTMLText(params.data.name);
                const artifact = (params.data as IArtifactNode).artifact;
                if (_.isFinite(artifact.itemTypeIconId)) {
                    icon = `<bp-item-type-icon
                                item-type-id="${artifact.itemTypeId}"
                                item-type-icon-id="${artifact.itemTypeIconId}"
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

    // the object defines how data will map to ITreeNode
    // key: data property names, value: ITreeNode property names
    public propertyMap: {[key: string]: string} = {
        id: "id",
        itemTypeId: "itemTypeId",
        name: "name",
        hasChildren: "hasChildren",
        parentNode: "parentNode",
        children: "children",
        loaded: "loaded",
        open: "open"
    };

    public doLoad = (prms: IArtifactNode): any[] => {
        //the explorer must be empty on a first load
        if (prms) {
            //notify the repository to load the node children
            this.projectManager.loadArtifact(prms.id);
            this.isFullReLoad = false;
        } else {
            this.isFullReLoad = true;
        }

        return null;
    };

    public doSelect = (node: IArtifactNode) => {
        this.doSync(node);
        this.selected = node;
        this.navigationService.navigateTo({ id: node.id });
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

        return artifactNode.artifact;
    };
}
