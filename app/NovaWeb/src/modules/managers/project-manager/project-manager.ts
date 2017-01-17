import {IDialogService, IDialogSettings} from "../../shared";
import {IStatefulArtifact} from "../artifact-manager/artifact/artifact";
import {IStatefulArtifactFactory} from "../artifact-manager/artifact/artifact.factory";
import {IDispose} from "../models";
import {Models, AdminStoreModels, Enums, TreeModels} from "../../main/models";
import {IProjectService, ProjectServiceStatusCode} from "./project-service";
import {IArtifactManager} from "../../managers";
import {IMetaDataService} from "../artifact-manager/metadata";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {HttpStatusCode} from "../../core/http/http-status-code";
import {IMessageService} from "../../core/messages/message.svc";
import {IMainBreadcrumbService} from "../../main/components/bp-page-content/mainbreadcrumb.svc";
import {MoveCopyArtifactInsertMethod} from "../../main/components/dialogs/move-copy-artifact/move-copy-artifact";
import {OpenProjectController} from "../../main/components/dialogs/open-project/open-project";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IApplicationError} from "../../core/error/applicationError";
import {IInstanceItem} from "../../main/models/admin-store-models";
import {IItemInfoResult} from "../../core/navigation/item-info.svc";

export interface IArtifactNode extends Models.IViewModel<IStatefulArtifact> {
    children?: this[];
    expanded?: boolean;
    unloadChildren(): void;
    getNode(comparator: IStatefulArtifact | ((model: IStatefulArtifact) => boolean), item?: this): this;
}

export interface IProjectManager extends IDispose {
    projectCollection: Rx.BehaviorSubject<Models.IViewModel<IStatefulArtifact>[]>;

    // eventManager
    initialize(): void;
    add(projectId: number): ng.IPromise<void>;
    openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void>;
    openProjectWithDialog(): void;
    remove(projectId: number): void;
    removeAll(): void;
    refresh(id: number, selectionId?: number, forceOpen?: boolean): ng.IPromise<void>;
    refreshCurrent(): ng.IPromise<void>;
    refreshAll(): ng.IPromise<void>;
    getProject(id: number): Models.IViewModel<IStatefulArtifact>;
    getSelectedProjectId(): number;
    triggerProjectCollectionRefresh(): void;
    getDescendantsToBeDeleted(artifact: IStatefulArtifact): ng.IPromise<Models.IArtifactWithProject[]>;
    calculateOrderIndex(insertMethod: MoveCopyArtifactInsertMethod, selectedArtifact: Models.IArtifact): ng.IPromise<number>;
    openProject(project: AdminStoreModels.IInstanceItem | IItemInfoResult): ng.IPromise<void>;
}

export class ProjectManager implements IProjectManager {
    private factory: TreeModels.TreeNodeVMFactory;
    private _projectCollection: Rx.BehaviorSubject<IArtifactNode[]>;
    private subscribers: Rx.IDisposable[];
    static $inject: [string] = [
        "$q",
        "messageService",
        "dialogService",
        "projectService",
        "artifactManager",
        "metadataService",
        "statefulArtifactFactory",
        "loadingOverlayService",
        "mainbreadcrumbService",
        "localization"
    ];

    constructor(private $q: ng.IQService,
                private messageService: IMessageService,
                private dialogService: IDialogService,
                private projectService: IProjectService,
                private artifactManager: IArtifactManager,
                private metadataService: IMetaDataService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private loadingOverlayService: ILoadingOverlayService,
                private mainBreadcrumbService: IMainBreadcrumbService,
                private localization: ILocalizationService) {
        this.factory = new TreeModels.TreeNodeVMFactory(projectService, artifactManager, statefulArtifactFactory);
        this.subscribers = [];
    }

    private onChangeInArtifactManagerCollection(artifact: IStatefulArtifact) {
        //Projects will null parentId have been removed from ArtifactManager
        if (artifact.parentId === null) {
            this.removeArtifact(artifact);
        }
    }

    private onChangeInCurrentlySelectedArtifact(artifact: IStatefulArtifact) {
        if (artifact.artifactState.misplaced) {
            const refreshOverlayId = this.loadingOverlayService.beginLoading();
            this.refresh(this.getSelectedProjectId()).finally(() => {
                this.triggerProjectCollectionRefresh();
                this.loadingOverlayService.endLoading(refreshOverlayId);
            });
        }
    }

    private disposeSubscribers() {
        this.subscribers.forEach((s) => s.dispose());
        this.subscribers = [];
    }

    public dispose() {
        this.removeAll();
        this.disposeSubscribers();

        if (this._projectCollection) {
            this._projectCollection.dispose();
            delete this._projectCollection;
        }
    }

    public initialize() {
        this.disposeSubscribers();

        if (this._projectCollection) {
            this._projectCollection.dispose();
            delete this._projectCollection;
        }

        this.subscribers.push(this.artifactManager.collectionChangeObservable.subscribeOnNext(this.onChangeInArtifactManagerCollection, this));
        this.subscribers.push(this.artifactManager.selection.currentlySelectedArtifactObservable
            .subscribeOnNext(this.onChangeInCurrentlySelectedArtifact, this));
    }

    public get projectCollection(): Rx.BehaviorSubject<IArtifactNode[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<IArtifactNode[]>([]));
    }

    public triggerProjectCollectionRefresh() {
        this.projectCollection.onNext(this.projectCollection.getValue());
    }

    public refreshAll(): ng.IPromise<any> {
        let refreshQueue = [];
        return this.artifactManager.autosave().then(() => {
            this.projectCollection.getValue().forEach((project) => {
                refreshQueue.push(this.refreshProject(project));
            });

            return this.$q.all(refreshQueue).finally(() => {
                this.triggerProjectCollectionRefresh();
            });
        });
    }

    public refresh(projectId: number, selectionId?: number, forceOpen?: boolean): ng.IPromise<void> {
        return this.artifactManager.autosave().then(() => {
            return this.refreshProject(this.getProject(projectId), selectionId, forceOpen);
        });
    }

    public refreshCurrent(): ng.IPromise<void> {
        return this.refresh(this.getSelectedProjectId());
    }

    private refreshProject(projectNode: IArtifactNode, selectionId?: number, forceOpen?: boolean): ng.IPromise<void> {
        if (!projectNode) {
            return this.$q.reject();
        }
        let selectedArtifact = {} as IStatefulArtifact;
        if (selectionId) {
            selectedArtifact.id = selectionId;
            selectedArtifact.projectId = projectNode.model.id;
        } else {
            selectedArtifact = this.artifactManager.selection.getArtifact();
        }

        return this.doRefresh(projectNode.model.id, selectedArtifact, forceOpen);
    }

    public openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void> {
        const artifactToExpand = {} as IStatefulArtifact;
        artifactToExpand.id = artifactIdToExpand;
        artifactToExpand.projectId = projectId;
        return this.doRefresh(projectId, artifactToExpand, false);
    }

    public openProjectWithDialog(): void {
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../main/components/dialogs/open-project/open-project.html"),
            controller: OpenProjectController,
            css: "nova-open-project" // removed modal-resize-both as resizing the modal causes too many artifacts with ag-grid
        }).then((project) => {
            this.openProject(project);
        });
    }

    public openProject(project: AdminStoreModels.IInstanceItem | IItemInfoResult): ng.IPromise<void> { // opens and selects project
        if (!project.hasOwnProperty("id")) {
            throw new Error("project does not have id");
        }
        /*fixme: this function should change.
         what it needs to do is just to insert the project into the tree as a root node. Expanding it shall be done else-ware*/
        const openProjectLoadingId = this.loadingOverlayService.beginLoading();
        //let openProjects = _.map(this.projectCollection.getValue(), "model.id");
        return this.add(project.id).finally(() => {
            /*const label = _.includes(openProjects, projectId) ? "duplicate" : "new";
            this.analytics.trackEvent("open", "project", label, project.id, {
                openProjects: openProjects
            });*/
            this.loadingOverlayService.endLoading(openProjectLoadingId);
        });
    }

    private loadProject(projectId: number, project: IArtifactNode): ng.IPromise<void> {
        //try it with project
        return this.projectService.getArtifacts(projectId).then((data: Models.IArtifact[]) => {
            this.messageService.addInfo("Refresh_Artifact_Deleted");
            return this.processProjectTree(projectId, data, projectId).catch(() => {
                this.clearProject(project);
                return this.$q.reject();
            });
        }).catch((err: any) => {
            this.dialogService.alert("Refresh_Project_NotFound");
            if (project && project.model) {
                this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(this.getProject(project.model.id)), 1);
            }
            return this.$q.reject();
        });
    }

    private doRefresh(projectId: number, expandToArtifact: IStatefulArtifact, forceOpen?: boolean): ng.IPromise<void> {
        const project = this.getProject(projectId);

        let selectedArtifactNode = this.getArtifactNode(expandToArtifact ? expandToArtifact.id : project.model.id);

        //if the artifact provided is not in the current project - just expand project node
        if (!expandToArtifact || expandToArtifact.projectId !== projectId) {
            expandToArtifact = this.getArtifact(projectId);
        }

        //try with selected artifact
        const loadChildren = forceOpen || (selectedArtifactNode ? selectedArtifactNode.expanded : false);
        return this.projectService.getProjectTree(projectId, expandToArtifact.id, loadChildren).then((data: Models.IArtifact[]) => {
            return this.processProjectTree(projectId, data, expandToArtifact.id).catch(() => {
                this.clearProject(project);
                return this.$q.reject();
            });
        }).catch((error: IApplicationError) => {
            if (!error) {
                this.clearProject(project);
                return this.$q.reject();
            }

            if (error.statusCode === HttpStatusCode.NotFound && error.errorCode === ProjectServiceStatusCode.ResourceNotFound) {
                //if we're selecting project
                if (expandToArtifact.id === expandToArtifact.projectId) {
                    this.dialogService.alert("Refresh_Project_NotFound");
                    this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(this.getProject(projectId)), 1);
                    return this.$q.reject();
                }

                // if there is no parent for artifact we are trying to load project
                // in case when artifact is deleted
                if (!expandToArtifact.parentId) {
                    return this.loadProject(projectId, project);
                }

                //try with selected artifact's parent
                return this.projectService.getProjectTree(projectId, expandToArtifact.parentId, true).then((data: Models.IArtifact[]) => {
                    this.messageService.addInfo("Refresh_Artifact_Deleted");
                    return this.processProjectTree(projectId, data, expandToArtifact.parentId).catch(() => {
                        this.clearProject(project);
                        return this.$q.reject();
                    });
                }).catch((innerError: any) => {
                    if (!innerError) {
                        this.clearProject(project);
                        return this.$q.reject();
                    }

                    if (innerError.statusCode === HttpStatusCode.NotFound && innerError.errorCode === ProjectServiceStatusCode.ResourceNotFound) {
                        //try it with project
                        return this.loadProject(projectId, project);
                    }

                    this.messageService.addError(innerError.message);
                    this.clearProject(project);
                    return this.$q.reject();
                });
            }

            this.messageService.addError(error.message);
            this.clearProject(project);
            return this.$q.reject();
        });
    }

    private processProjectTree(projectId: number, data: Models.IArtifact[], artifactToSelectId: number): ng.IPromise<void> {
        const oldProject = this.getProject(projectId);

        // if old project is opened
        if (oldProject) {
            this.artifactManager.removeAll(projectId);
        }

        return this.metadataService.get(projectId)
            .then(() => {
                //reload project info
                return this.projectService.getProject(projectId);
            })
            .then((project: AdminStoreModels.IInstanceItem) => {
                //add some additional info
                _.assign(project, {
                    projectId: projectId,
                    itemTypeId: Enums.ItemTypePredefined.Project,
                    prefix: "PR",
                    itemTypeName: "Project",
                    predefinedType: Enums.ItemTypePredefined.Project,
                    hasChildren: true
                });

                //create project node
                const statefulProject = this.statefulArtifactFactory.createStatefulArtifact(project);
                this.artifactManager.add(statefulProject);
                let newProjectNode: IArtifactNode = this.factory.createStatefulArtifactNodeVM(statefulProject, true);

                //populate it
                newProjectNode.children = data.map((it: Models.IArtifact) => {
                    const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                    this.artifactManager.add(statefulArtifact);
                    return this.factory.createStatefulArtifactNodeVM(statefulArtifact);
                });

                //open any children that have children
                this.openChildNodes(newProjectNode.children, data);

                if (oldProject) {
                    this.refreshSelectedArtifactIfDisposed(projectId);

                    //update project collection
                    this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(oldProject), 1, newProjectNode);
                    oldProject.unloadChildren();
                } else {
                    this.projectCollection.getValue().unshift(newProjectNode);
                    this.projectCollection.onNext(this.projectCollection.getValue());
                }
            });
    }

    private refreshSelectedArtifactIfDisposed(projectId: number): void {
        const currentlySelectItem = this.artifactManager.selection.getArtifact();

        if (currentlySelectItem && currentlySelectItem.projectId === projectId && currentlySelectItem.isDisposed) {
            const updatedSelectedArtifact = this.artifactManager.get(currentlySelectItem.id);

            if (updatedSelectedArtifact && !updatedSelectedArtifact.isDisposed) {
                this.artifactManager.selection.setArtifact(updatedSelectedArtifact);
            } else {
                this.artifactManager.selection.clearAll();
            }
        }
    }

    private clearProject(project: IArtifactNode) {
        if (project) {
            project.children = undefined;
            project.expanded = false;
        }
    }

    private openChildNodes(childrenNodes: IArtifactNode[], childrenData: Models.IArtifact[]): void {
        _.forEach(childrenNodes, (node) => {
            let childData = childrenData.filter(it => it.id === node.model.id);

            //if it has children - expand the node
            if (childData[0].hasChildren && childData[0].children) {
                node.children = childData[0].children.map(
                    (it: Models.IArtifact) => {
                        const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                        this.artifactManager.add(statefulArtifact);

                        return this.factory.createStatefulArtifactNodeVM(statefulArtifact);
                    });

                node.expanded = true;

                this.openChildNodes(node.children, childData[0].children);
            }
        });
    }

    public add(projectId: number): ng.IPromise<void> {
        let projectNode: IArtifactNode = this.getProject(projectId);
        if (!projectNode) {
            return this.projectService.getProject(projectId).then((projectInfo: IInstanceItem) => {
                const project = {
                    id: projectInfo.id,
                    name: projectInfo.name,
                    description: projectInfo.description,
                    parentFolderId: undefined,
                    type: AdminStoreModels.InstanceItemType.Project,
                    hasChildren: true,
                    permissions: projectInfo.permissions
                } as AdminStoreModels.IInstanceItem;

                return this.metadataService.get(projectId).then(() => {
                    _.assign(project, {
                        projectId: projectId,
                        itemTypeId: Enums.ItemTypePredefined.Project,
                        prefix: "PR",
                        itemTypeName: "Project",
                        predefinedType: Enums.ItemTypePredefined.Project,
                        hasChildren: true
                    });

                    const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(project);
                    this.artifactManager.add(statefulArtifact);
                    projectNode = this.factory.createStatefulArtifactNodeVM(statefulArtifact, true);
                    this.projectCollection.getValue().unshift(projectNode);
                    this.projectCollection.onNext(this.projectCollection.getValue());
                }).catch((err: any) => {
                    if (err) {
                        this.messageService.addError(err);
                    }
                    return this.$q.reject(err);
                });
            });
        }

        // the project has been loaded already
        return this.$q.resolve();
    }

    private removeArtifact(artifact: IStatefulArtifact) {
        let node: IArtifactNode = this.getArtifactNode(artifact.parentId);
        if (node) {
            node.children = node.children.filter((child) => child.model.id !== artifact.id);
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public remove(projectId: number) {
        this.artifactManager.removeAll(projectId);
        const projects = this.projectCollection.getValue().filter((project) => {
            if (project.model.projectId === projectId) {
                project.unloadChildren();
                return false;
            }
            return true;
        });

        this.projectCollection.onNext(projects);
    }

    public removeAll() {
        this.projectCollection.getValue().forEach((project) => {
            this.artifactManager.removeAll(project.model.projectId);
            project.unloadChildren();
        });
        this.projectCollection.onNext([]);
    }

    public getProject(id: number): IArtifactNode {
        for (let project of this.projectCollection.getValue()) {
            if (project.model.id === id) {
                return project;
            }
        }
        return undefined;
    }

    public getSelectedProjectId(): number {
        let artifact = this.artifactManager.selection.getArtifact();
        if (!artifact) {
            return null;
        }
        return artifact.projectId;
    }

    private getArtifactNode(id: number): IArtifactNode {
        let found: IArtifactNode;
        let projects = this.projectCollection.getValue();
        for (let i = 0, it: IArtifactNode; !found && (it = projects[i++]); ) {
            found = it.getNode(model => model.id === id);
        }
        return found;
    };

    private getArtifact(id: number): IStatefulArtifact {
        let found = this.getArtifactNode(id);
        return found ? found.model : null;
    };

    public getDescendantsToBeDeleted(artifact: IStatefulArtifact): ng.IPromise<Models.IArtifactWithProject[]> {
        let projectName: string;
        return this.projectService.getProject(artifact.projectId).then((project: AdminStoreModels.IInstanceItem) => {
            projectName = project.name;
            return this.projectService.getArtifacts(project.id, artifact.id);
        }).then((data: Models.IArtifact[]) => {
            return data.map(a => _.assign({projectName: projectName}, a) as Models.IArtifactWithProject);
        });
    }

    public calculateOrderIndex(insertMethod: MoveCopyArtifactInsertMethod, selectedArtifact: Models.IArtifact): ng.IPromise<number> {
        let promise: ng.IPromise<void>;
        let orderIndex: number;
        let index: number;

        let siblings: Models.IArtifact[];
        //get parent node
        let parentArtifactNode: IArtifactNode = this.getArtifactNode(selectedArtifact.parentId);

        //if parent isn't found, or if its children aren't loaded
        if (!parentArtifactNode || (!parentArtifactNode.children || parentArtifactNode.children.length === 0)) {
            //get children from server
            promise = this.projectService.getArtifacts(selectedArtifact.projectId, selectedArtifact.parentId).then((data: Models.IArtifact[]) => {
                siblings = data;
            });
        } else {
            //otherwise, get children from cache
            siblings = _.map(parentArtifactNode.children, (node) => node.model);
            promise = this.$q.resolve();
        }

        return promise.then(() => {
            //sort by order index
            siblings = _.sortBy(siblings, (a) => a.orderIndex);
            index = _.findIndex(siblings, (a) => a.id === selectedArtifact.id);

            //compute new order index
            if (index === 0 && insertMethod === MoveCopyArtifactInsertMethod.Above) { //first
                orderIndex = selectedArtifact.orderIndex / 2;
            } else if (index === siblings.length - 1 && insertMethod === MoveCopyArtifactInsertMethod.Below) { //last
                orderIndex = selectedArtifact.orderIndex + 10;
            } else {    //in between
                if (insertMethod === MoveCopyArtifactInsertMethod.Above) {
                    orderIndex = (siblings[index - 1].orderIndex + selectedArtifact.orderIndex) / 2;
                } else if (insertMethod === MoveCopyArtifactInsertMethod.Below) {
                    orderIndex = (siblings[index + 1].orderIndex + selectedArtifact.orderIndex) / 2;
                } else {
                    //leave undefined
                }
            }
            return orderIndex;
        });
    }
}
