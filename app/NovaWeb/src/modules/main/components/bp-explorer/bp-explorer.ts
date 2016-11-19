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
import {IColumn, IColumnRendererParams} from "../../../shared/widgets/bp-tree";

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string = require("./bp-explorer.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = ProjectExplorerController;
    public transclude: boolean = true;
}

export interface IProjectExplorerController {
    // BpTree bindings
    treeApi: IBPTreeControllerApi;
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
        // If this method is called after onLoadProject, but before onGridReset, the artifact will be
        // selected in onGridReset. This allows code that refreshes explorer, then naviages to a new
        // artifact, to work as expected.
        if (this.isLoading) {
            this.pendingSelectedArtifactId = artifactId;
        } else if (this.treeApi.nodeExists(artifactId)) {
            this.treeApi.selectNode(artifactId);
        } else {
            this.treeApi.deselectAll();
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
            this.selectedArtifactSubscriber = value.model.getProperyObservable()
                        .distinctUntilChanged(changes => changes.item && changes.item.name)
                        .subscribeOnNext(this.onSelectedArtifactChange);
        }
    }

    private isLoading: boolean;
    private pendingSelectedArtifactId: number;

    private onLoadProject = (projects: IArtifactNode[]) => {
        this.isLoading = true;
        this.projects = projects.slice(0); // create a copy
    }

    public onGridReset(): void {
        this.isLoading = false;

        const selectedArtifactId = this.selected ? this.selected.model.id : undefined;
        let navigateToId: number;
        if (this.projects && this.projects.length > 0) {
            if (this.pendingSelectedArtifactId) {
                navigateToId = this.pendingSelectedArtifactId;
                this.pendingSelectedArtifactId = undefined;
            } else if (!selectedArtifactId || this.numberOfProjectsOnLastLoad !== this.projects.length) {
                navigateToId = this.projects[0].model.id;
            } else if (this.treeApi.nodeExists(selectedArtifactId)) {
                navigateToId = selectedArtifactId;
            } else if (this.treeApi.nodeExists(this.selected.model.parentId)) {
                navigateToId = this.selected.model.parentId;
            } else if (this.treeApi.nodeExists(this.selected.model.projectId)) {
                navigateToId = this.selected.model.projectId;
            }
        }

        this.numberOfProjectsOnLastLoad = this.projects.length;

        if (_.isFinite(navigateToId)) {
            if (navigateToId !== selectedArtifactId) {
                this.treeApi.selectNode(navigateToId);
            } else {
                this.navigationService.reloadParentState();
            }
        }
    };

    private onSelectedArtifactChange = (changes: IItemChangeSet) => {
        //If the artifact's name changes (on refresh), we refresh specific node only .
        //To prevent update treenode name while editing the artifact details, use it only for clean artifact.
        if (changes.item) {
            this.treeApi.refresh(changes.item.id);
        }
    };

    // BpTree bindings

    public treeApi: IBPTreeControllerApi;
    public projects: IArtifactNode[];
    public columns: IColumn[] = [{
        cellClass: (vm: IArtifactNode) => {
            const result = [] as string[];

            if (vm.group) {
                result.push("has-children");
            }
            let typeName: string;
            if (vm.model.predefinedType === Models.ItemTypePredefined.CollectionFolder &&
                vm.model.itemTypeId === Models.ItemTypePredefined.Collections) {
                typeName = Models.ItemTypePredefined[Models.ItemTypePredefined.Collections];
            } else {
                typeName = Models.ItemTypePredefined[vm.model.predefinedType];
            }
            if (typeName) {
                result.push("is-" + _.kebabCase(typeName));
            }
            return result;
        },
        isGroup: true,
        innerRenderer: (params: IColumnRendererParams) => {
            const vm = params.data as IArtifactNode;
            let icon = "<i></i>";
            const artifact = vm.model;
            const label = Helper.escapeHTMLText(artifact.name);
            if (_.isFinite(artifact.itemTypeIconId)) {
                icon = `<bp-item-type-icon
                            item-type-id="${artifact.itemTypeId}"
                            item-type-icon-id="${artifact.itemTypeIconId}"></bp-item-type-icon>`;
            }
            return `<span class="ag-group-value-wrapper">${icon}<span>${label}</span></span>`;
        }
    }];

    public doSelect = (node: IArtifactNode) => {
        this.selected = node;
        this.navigationService.navigateTo({ id: node.model.id });
    };

    public onError = (reason: any): void => {
        if (reason) {
            this.messageService.addError(reason["message"] || "Artifact_NotFound");
        }
    }
}
