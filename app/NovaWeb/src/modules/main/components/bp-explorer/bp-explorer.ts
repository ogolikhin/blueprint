import * as angular from "angular";
import {Models} from "../../models";
import {ItemTypePredefined} from "../../models/enums";
import {Helper, IBPTreeControllerApi} from "../../../shared";
import {IProjectManager, IArtifactManager} from "../../../managers";
import {IStatefulArtifact, IItemChangeSet} from "../../../managers/artifact-manager";
import {ISelectionManager} from "../../../managers/selection-manager";
import {IArtifactNode} from "../../../managers/project-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {IMessageService} from "../../../core/messages/message.svc";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = ProjectExplorerController;
    public transclude: boolean = true;
}

export interface IProjectExplorerController {
    // BpTree bindings
    tree: IBPTreeControllerApi;
    projects: IArtifactNode[];
    columns: any[];
    doSelect: Function;
    onError: (reason: any) => any;
    onGridReset: () => void;
}

export class ProjectExplorerController implements IProjectExplorerController {
    private subscribers: Rx.IDisposable[];
    private selectedArtifactSubscriber: Rx.IDisposable;
    private numberOfProjectsOnLastLoad: number;
    private selectedArtifactId: number;
    private isFullReLoad: boolean;

    public static $inject: [string] = [
        "$q",
        "projectManager",
        "artifactManager",
        "navigationService",
        "selectionManager",
        "messageService"
    ];

    constructor(private $q: ng.IQService,
                private projectManager: IProjectManager,
                private artifactManager: IArtifactManager,
                private navigationService: INavigationService,
                private selectionManager: ISelectionManager,
                private messageService: IMessageService) {
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
        const setSelectedNodeInternal = (artifactId: number) => {
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
        };

        if (this.isLoading) {
            this.isLoading.promise.then(() => setSelectedNodeInternal(artifactId));
        } else {
            setSelectedNodeInternal(artifactId);
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
            this.selectedArtifactId = value.id;

            this.selectedArtifactSubscriber = value.artifact.getProperyObservable()
                        .distinctUntilChanged(changes => changes.item && changes.item.name)
                        .subscribeOnNext(this.onSelectedArtifactChange);
        }
    }

    private isLoading: ng.IDeferred<void>;

    private onLoadProject = (projects: IArtifactNode[]) => {
        if (!this.isLoading) {
            this.isLoading = this.$q.defer<void>();
        }
        this.projects = projects.slice(0); // create a copy
    }

    public onGridReset(): void {
        if (this.isLoading) {
            this.isLoading.resolve();
            this.isLoading = undefined;
        }

        const currentSelectionId = this.selectedArtifactId;
        let navigateToId: number;
        if (this.projects && this.projects.length > 0) {
            if (!this.selectedArtifactId || this.numberOfProjectsOnLastLoad !== this.projects.length) {
                this.setSelectedNode(this.projects[0].artifact.id);
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
            } else if (this.selected.parentNode && this.tree.nodeExists(this.selected.parentNode.id)) {
                //otherwise, if parent node is in the tree
                this.tree.selectNode(this.selected.parentNode.id);
                navigateToId = this.selected.parentNode.id;

                //replace with a new object from tree, since the selected object may be stale after refresh
                this.setSelectedNode(this.selected.parentNode.id);
            } else if (this.tree.nodeExists(this.selected.artifact.projectId)) {
                //otherwise, try with project node
                this.tree.selectNode(this.selected.artifact.projectId);
                navigateToId = this.selected.artifact.projectId;
            }
        }

        this.numberOfProjectsOnLastLoad = this.projects.length;

        if (_.isFinite(navigateToId)) {
            if (navigateToId !== currentSelectionId) {
                this.navigationService.navigateTo({ id: navigateToId });
            } else {
                this.navigationService.reloadParentState();
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
    public projects: IArtifactNode[];
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

    public doSelect = (node: IArtifactNode) => {
        this.selected = node;
        this.navigationService.navigateTo({ id: node.id });
    };

    public onError = (reason: any): void => {
        if (reason) {
            this.messageService.addError(reason["message"] || "Artifact_NotFound");
        }
    }
}
