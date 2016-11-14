import * as angular from "angular";
import {Models} from "../../models";
import {ItemTypePredefined} from "../../models/enums";
import {Helper, IBPTreeControllerApi} from "../../../shared";
import {IProjectManager, IArtifactManager} from "../../../managers";
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
    doLoad: Function;
    doSelect: Function;
}

export class ProjectExplorerController implements IProjectExplorerController {
    private subscribers: Rx.IDisposable[];
    private selectedArtifactSubscriber: Rx.IDisposable;
    private numberOfProjectsOnLastLoad: number;
    private selectedArtifactNameBeforeChange: string;
    private selectedArtifactId: number;
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

    private setSelectedNode(artifactId: number) {
        if (this.tree.nodeExists(artifactId)) {
            this.tree.selectNode(artifactId);

            if (!this.selected || this.selected.id !== artifactId) {
                let selectedObjectInTree: IArtifactNode = <IArtifactNode>this.tree.getNodeData(artifactId);
                if (selectedObjectInTree) {
                    this.selected = selectedObjectInTree;
                }
            }
        } else {
            this.tree.deselectAll();

            this.selected = null;
        }
    }

    private _selected: IArtifactNode;
    private get selected() {
        return this._selected;
    }

    private set selected(value: IArtifactNode) {
        this._selected = value;

        //Dispose of old subscriber and subscribe to new artifact.
        if (this.selectedArtifactSubscriber) {
            this.selectedArtifactSubscriber.dispose();
        }

        if (value) {
            this.selectedArtifactNameBeforeChange = value.name;
            this.selectedArtifactId = value.id;

            this.selectedArtifactSubscriber = value.artifact.getProperyObservable()
                        .distinctUntilChanged(changes => changes.item && changes.item.name)
                        .subscribeOnNext(this.onSelectedArtifactChange);
        }
    }

    private onLoadProject = (projects: IArtifactNode[]) => {
        //NOTE: this method is called during "$onInit" and as a part of "Rx.BehaviorSubject" initialization.
        // At this point the tree component (bp-tree) is not created yet due to component hierachy (dependant)
        // so, just need to do an extra check if the component has created
        if (this.tree) {

            this.tree.reload(projects);

            const currentSelectionId = this.selectedArtifactId;
            let navigateToId: number;
            if (projects && projects.length > 0) {
                if (!this.selectedArtifactId || this.numberOfProjectsOnLastLoad !== projects.length) {
                    this.setSelectedNode(projects[0].artifact.id);
                    navigateToId = this.selectedArtifactId;
                }

                if (this.tree.nodeExists(this.selectedArtifactId)) {
                    //if node exists in the tree
                    if (this.isFullReLoad || this.selectedArtifactId !== this.tree.getSelectedNodeId()) {
                        navigateToId = this.selectedArtifactId;
                    }
                    this.isFullReLoad = true;

                    //replace with a new object from tree, since the selected object may be stale after refresh
                    this.setSelectedNode(this.selectedArtifactId);
                } else {
                    //otherwise, if parent node is in the tree
                    if (this.selected.parentNode && this.tree.nodeExists(this.selected.parentNode.id)) {
                        this.tree.selectNode(this.selected.parentNode.id);
                        navigateToId = this.selected.parentNode.id;

                        //replace with a new object from tree, since the selected object may be stale after refresh
                        this.setSelectedNode(this.selected.parentNode.id);
                    } else {
                        //otherwise, try with project node
                        if (this.tree.nodeExists(this.selected.artifact.projectId)) {
                            this.tree.selectNode(this.selected.artifact.projectId);
                            navigateToId = this.selected.artifact.projectId;
                        }
                    }
                }
            }

            this.numberOfProjectsOnLastLoad = projects.length;

            if (_.isFinite(navigateToId)) {
                if (navigateToId !== currentSelectionId) {
                    this.navigationService.navigateTo({ id: navigateToId });

                } else if (navigateToId === currentSelectionId) {
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
            const node = params.data as IArtifactNode;
            let css: string[] = [];

            if (node.hasChildren) {
                css.push("has-children");
            }
            let typeName: string;
            if (node.artifact.predefinedType === Models.ItemTypePredefined.CollectionFolder &&
                node.parentNode.artifact.predefinedType === Models.ItemTypePredefined.Project) {
                typeName = Models.ItemTypePredefined[Models.ItemTypePredefined.Collections];
            } else {
                typeName = Models.ItemTypePredefined[node.artifact.predefinedType];
            }
            if (typeName) {
                css.push("is-" + _.kebabCase(typeName));
            }
            return css;
        },

        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                const node = params.data as IArtifactNode;
                let icon = "<i ng-drag-handle></i>";
                const name = Helper.escapeHTMLText(node.name);
                const artifact = node.artifact;
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
        this.selected = node;
        this.navigationService.navigateTo({ id: node.id });
    };
}
