import * as _ from "lodash";
import {IDialogService} from "../../shared";
import {IStatefulArtifactFactory, IStatefulArtifact} from "../artifact-manager/artifact";
import {ArtifactNode} from "./artifact-node";
import {IDispose} from "../models";
import {Models, AdminStoreModels, Enums} from "../../main/models";
import {IProjectService, ProjectServiceStatusCode} from "./project-service";
import {IArtifactManager} from "../../managers";
import {IMetaDataService} from "../artifact-manager/metadata";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {HttpStatusCode} from "../../core/http/http-status-code";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";

export interface IArtifactNode extends IDispose {
    artifact: IStatefulArtifact;
    children?: IArtifactNode[];
    parentNode: IArtifactNode;
    id: number;
    name: string;
    projectId: number;
    //parentId: number;
    permissions: Enums.RolePermissions;
    predefinedType: Enums.ItemTypePredefined;
    hasChildren?: boolean;
    loaded?: boolean;
    open?: boolean;
    getNode(id: number, item?: IArtifactNode): IArtifactNode;
}

export interface IProjectManager extends IDispose {
    projectCollection: Rx.BehaviorSubject<IArtifactNode[]>;

    // eventManager
    initialize();
    add(project: AdminStoreModels.IInstanceItem);
    remove(projectId: number): void;
    removeAll(): void;
    refresh(id: number, forceOpen?: boolean): ng.IPromise<void>;
    refreshCurrent(): ng.IPromise<void>;
    refreshAll(): ng.IPromise<void>;
    loadArtifact(id: number): void;
    getProject(id: number): IArtifactNode;
    getArtifactNode(id: number): IArtifactNode;
    getSelectedProject(): IArtifactNode;
    triggerProjectCollectionRefresh();
    getDescendantsToBeDeleted(artifact: IStatefulArtifact): ng.IPromise<Models.IArtifactWithProject[]>;
}

export class ProjectManager implements IProjectManager {

    private _projectCollection: Rx.BehaviorSubject<IArtifactNode[]>;
    private subscribers: Rx.IDisposable[];
    static $inject: [string] = [
        "$q",
        "localization",
        "messageService",
        "dialogService",
        "projectService",
        "navigationService",
        "artifactManager",
        "metadataService",
        "statefulArtifactFactory",
        "loadingOverlayService"
    ];

    constructor(private $q: ng.IQService,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private dialogService: IDialogService,
                private projectService: IProjectService,
                private navigationService: INavigationService,
                private artifactManager: IArtifactManager,
                private metadataService: IMetaDataService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private loadingOverlayService: ILoadingOverlayService) {

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
            this.refreshCurrent().finally(() => {
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
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<ArtifactNode[]>([]));
    }

    public triggerProjectCollectionRefresh() {
        this.projectCollection.onNext(this.projectCollection.getValue());
    }

    public refreshAll(): ng.IPromise<any> {
        let refreshQueue = [];

        this.projectCollection.getValue().forEach((project) => {
            refreshQueue.push(this.refreshProject(project));
        });

        return this.$q.all(refreshQueue).finally(() => {
            this.triggerProjectCollectionRefresh();
        });
    }

    public refresh(projectId: number, forceOpen?: boolean): ng.IPromise<void> {
        return this.refreshProject(this.getProject(projectId), forceOpen);
    }

    public refreshCurrent(): ng.IPromise<void> {
        return this.refreshProject(this.getSelectedProject());
    }

    private refreshProject(projectNode: IArtifactNode, forceOpen?: boolean): ng.IPromise<void> {
        if (!projectNode) {
            return this.$q.reject();
        }

        let selectedArtifact = this.artifactManager.selection.getArtifact();

        //if selected artifact is dirty and is in the project being refreshed - perform autosave
        let autosave: ng.IPromise<any>;
        if (selectedArtifact.artifactState.dirty && selectedArtifact.projectId === projectNode.id) {
            autosave = selectedArtifact.autosave();
        } else {
            autosave = this.$q.resolve();
        }

        return autosave.then(() => {
            this.doRefresh(projectNode, selectedArtifact, forceOpen);
        }).catch(() => {
            //something went wrong - ask user if they want to force refresh
            return this.dialogService.confirm(this.localization.get("Confirmation_Continue_Refresh"));
        }).then(() => {
            return this.doRefresh(projectNode, selectedArtifact, forceOpen);
        });
    }

    private doRefresh(project: IArtifactNode, expandToArtifact: IStatefulArtifact, forceOpen?: boolean): ng.IPromise<void> {
        let selectedArtifactNode = this.getArtifactNode(expandToArtifact.id);

        //if the artifact provided is not in the current project - just expand project node
        if (expandToArtifact.projectId !== project.id) {
            expandToArtifact = this.getArtifact(project.id);
        }

        //try with selected artifact
        const loadChildren = forceOpen || (selectedArtifactNode ? selectedArtifactNode.open : false);
        return this.projectService.getProjectTree(project.id, expandToArtifact.id, loadChildren).then((data: Models.IArtifact[]) => {
            return this.processProjectTree(project, data).catch(() => {
                this.clearProject(project);
                return this.$q.reject();
            });
        }).catch((error: any) => {
            if (!error) {
                this.clearProject(project);
                return this.$q.reject();
            }

            if (error.statusCode === HttpStatusCode.NotFound && error.errorCode === ProjectServiceStatusCode.ResourceNotFound) {
                //if we're selecting project
                if (expandToArtifact.id === expandToArtifact.projectId) {
                    this.dialogService.alert("Refresh_Project_NotFound");
                    this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(this.getProject(project.id)), 1);
                    return this.$q.reject();
                }

                //try with selected artifact's parent
                return this.projectService.getProjectTree(project.id, expandToArtifact.parentId, true).then((data: Models.IArtifact[]) => {
                    this.messageService.addWarning("Refresh_Artifact_Deleted");
                    return this.processProjectTree(project, data).catch(() => {
                        this.clearProject(project);
                        return this.$q.reject();
                    });
                }).catch((innerError: any) => {
                    if (innerError.statusCode === HttpStatusCode.NotFound && innerError.errorCode === ProjectServiceStatusCode.ResourceNotFound) {
                        //try it with project
                        return this.projectService.getArtifacts(project.id).then((data: Models.IArtifact[]) => {
                            this.messageService.addWarning("Refresh_Artifact_Deleted");
                            return this.processProjectTree(project, data).catch(() => {
                                this.clearProject(project);
                                return this.$q.reject();
                            });
                        }).catch((err: any) => {
                            this.dialogService.alert("Refresh_Project_NotFound");
                            this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(this.getProject(project.id)), 1);
                            return this.$q.reject();
                        });
                    }

                    this.messageService.addError(error["message"]);
                    this.clearProject(project);
                    return this.$q.reject();
                });
            }

            this.messageService.addError(error["message"]);
            this.clearProject(project);
            return this.$q.reject();
        });
    }

    private processProjectTree(project: IArtifactNode, data: Models.IArtifact[]): ng.IPromise<void> {
        const oldProjectId: number = project.id;
        const oldProjectPermissions: number = project.permissions;
        let oldProject = this.getProject(oldProjectId);
        this.artifactManager.removeAll(oldProjectId);

        return this.metadataService.get(oldProjectId).then(() => {

            //reload project info
            return this.projectService.getProject(oldProjectId);
        }).then((result: AdminStoreModels.IInstanceItem) => {

            //add some additional info
            _.assign(result, {
                projectId: oldProjectId,
                itemTypeId: Enums.ItemTypePredefined.Project,
                prefix: "PR",
                permissions: oldProjectPermissions,
                predefinedType: Enums.ItemTypePredefined.Project,
                hasChildren: true
            });

            //create project node
            const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(result);
            this.artifactManager.add(statefulArtifact);
            let newProjectNode: IArtifactNode = new ArtifactNode(statefulArtifact);

            //populate it
            newProjectNode.children = data.map((it: Models.IArtifact) => {
                const statefulProject = this.statefulArtifactFactory.createStatefulArtifact(it);
                this.artifactManager.add(statefulProject);
                return new ArtifactNode(statefulProject, newProjectNode);
            });
            newProjectNode.loaded = true;
            newProjectNode.open = true;

            //open any children that have children
            this.openChildNodes(newProjectNode.children, data);

            //update project collection
            this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(oldProject), 1, newProjectNode);
            oldProject.dispose();
        });
    }

    private clearProject(project: IArtifactNode) {
        project.children = [];
        project.loaded = false;
        project.open = false;
        //this.projectCollection.onNext(this.projectCollection.getValue());
    }

    private openChildNodes(childrenNodes: IArtifactNode[], childrenData: Models.IArtifact[]) {
        //go through each node
        _.forEach(childrenNodes, (node) => {
            let childData = childrenData.filter(function (it) {
                return it.id === node.id;
            });
            //if it has children - expand the node
            if (childData[0].hasChildren && childData[0].children) {
                node.children = childData[0].children.map((it: Models.IArtifact) => {
                    const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                    this.artifactManager.add(statefulArtifact);
                    return new ArtifactNode(statefulArtifact, node);
                });
                node.loaded = true;
                node.open = true;

                //process its children
                this.openChildNodes(node.children, childData[0].children);
            }
        });
    }

    public add(project: AdminStoreModels.IInstanceItem): ng.IPromise<void> {
        if (!project) {
            throw new Error("Project_NotFound"); // need to throw an error as mainView may not be active yet
        }

        let projectNode: IArtifactNode = this.getProject(project.id);
        if (!projectNode) {
            return this.metadataService.get(project.id).then(() => {
                _.assign(project, {
                    projectId: project.id,
                    itemTypeId: Enums.ItemTypePredefined.Project,
                    prefix: "PR",
                    itemTypeName: "Project",
                    permissions: project.permissions,
                    predefinedType: Enums.ItemTypePredefined.Project,
                    hasChildren: true
                });

                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(project);
                this.artifactManager.add(statefulArtifact);
                projectNode = new ArtifactNode(statefulArtifact);
                this.projectCollection.getValue().unshift(projectNode);
                return this.loadArtifact(project.id);
            }).catch((err: any) => {
                if (err) {
                    this.messageService.addError(err);
                }
                return this.$q.reject(err);
            });
        }

        // the project has been loaded already
        return this.$q.resolve();
    }

    private removeArtifact(artifact: IStatefulArtifact) {
        let node: IArtifactNode = this.getArtifactNode(artifact.id);
        if (node) {
            node.parentNode.children = node.parentNode.children.filter((child) => child.id !== artifact.id);
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public remove(projectId: number) {
        this.artifactManager.removeAll(projectId);
        const projects = this.projectCollection.getValue().filter((project) => {
            if (project.projectId === projectId) {
                project.dispose();
                return false;
            }
            return true;
        });

        this.projectCollection.onNext(projects);
    }

    public removeAll() {
        this.projectCollection.getValue().forEach((project) => {
            this.artifactManager.removeAll(project.projectId);
            project.dispose();
        });
        this.projectCollection.onNext([]);
    }

    public loadArtifact(id: number): ng.IPromise<any> {
        let node: IArtifactNode = this.getArtifactNode(id);
        if (node) {
            return this.projectService.getArtifacts(node.projectId, node.artifact.id).then((data: Models.IArtifact[]) => {
                node.children = data.map((it: Models.IArtifact) => {
                    const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                    this.artifactManager.add(statefulArtifact);
                    return new ArtifactNode(statefulArtifact, node);
                });
                node.loaded = true;
                node.open = true;

                this.projectCollection.onNext(this.projectCollection.getValue());
            }).catch((error: any) => {
                //ignore authentication errors here
                if (error) {
                    this.messageService.addError(error["message"] || "Artifact_NotFound");
                    return this.$q.reject();
                }

                node.children = [];
                node.loaded = false;
                node.open = false;
                //node.hasChildren = false;
                this.projectCollection.onNext(this.projectCollection.getValue());
            });
        }

        throw new Error("Artifact_NotFound"); // need to throw an error as mainView may not be active yet
    }

    public getProject(id: number): IArtifactNode {
        for (let project of this.projectCollection.getValue()) {
            if (project.id === id) {
                return project;
            }
        }
        return undefined;
    }

    public getSelectedProject(): IArtifactNode {
        let artifact = this.artifactManager.selection.getArtifact();
        if (!artifact) {
            return null;
        }
        let project = this.getProject(artifact.projectId);
        return project;
    }

    public getArtifactNode(id: number): IArtifactNode {
        let found: IArtifactNode;
        let projects = this.projectCollection.getValue();
        for (let i = 0, it: IArtifactNode; !found && (it = projects[i++]); ) {
            found = it.getNode(id);
        }
        return found;
    };

    private getArtifact(id: number): IStatefulArtifact {
        let found = this.getArtifactNode(id);
        return found ? found.artifact : null;
    };

    public getDescendantsToBeDeleted(artifact: IStatefulArtifact):  ng.IPromise<Models.IArtifactWithProject[]> {
        let projectName: string;
        return this.projectService.getProject(artifact.projectId).then((project: AdminStoreModels.IInstanceItem) => {
            projectName = project.name;
            return this.projectService.getArtifacts(project.id, artifact.id);
        }).then((data: Models.IArtifact[]) => {
            return data.map(a => _.assign({projectName: projectName}, a) as Models.IArtifactWithProject);
        });
    }
}
