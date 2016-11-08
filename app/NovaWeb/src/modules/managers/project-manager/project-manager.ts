import * as _ from "lodash";
import {ILocalizationService, IMessageService, INavigationService, HttpStatusCode} from "../../core";
import {IDialogService} from "../../shared";
import {IStatefulArtifactFactory, IStatefulArtifact} from "../artifact-manager/artifact";
import {Project, ArtifactNode} from "./project";
import {IDispose} from "../models";
import {Models, AdminStoreModels, Enums} from "../../main/models";
import {IProjectService, ProjectServiceStatusCode} from "./project-service";
import {IArtifactManager} from "../../managers";
import {IMetaDataService} from "../artifact-manager/metadata";
import {ILoadingOverlayService} from "../../core/loading-overlay";

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
}

export interface IProjectManager extends IDispose {
    projectCollection: Rx.BehaviorSubject<Project[]>;

    // eventManager
    initialize();
    add(data: Models.IProject);
    remove(): void;
    removeAll(): void;
    refresh(data: Models.IProject): ng.IPromise<any>;
    refreshAll(): ng.IPromise<any>;
    loadArtifact(id: number): void;
    loadFolders(id?: number): ng.IPromise<AdminStoreModels.IInstanceItem[]>;
    getProject(id: number): Project;
    getArtifactNode(id: number): IArtifactNode;
    getArtifact(id: number): IStatefulArtifact;
    getSelectedProject(): Project;
    triggerProjectCollectionRefresh();
    getDescendantsToBeDeleted(artifact: IStatefulArtifact):  ng.IPromise<Models.IProject>;
}


export class ProjectManager implements IProjectManager {

    private _projectCollection: Rx.BehaviorSubject<Project[]>;
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
            this.refresh(this.getSelectedProject()).finally(() => {
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

    public get projectCollection(): Rx.BehaviorSubject<Project[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Project[]>([]));
    }

    public triggerProjectCollectionRefresh() {
        this.projectCollection.onNext(this.projectCollection.getValue());
    }

    public refreshAll(): ng.IPromise<any> {
        let defered = this.$q.defer<any>();

        let refreshQueue = [];

        this.projectCollection.getValue().forEach((project) => {
            refreshQueue.push(this.refresh(project));
        });

        this.$q.all(refreshQueue).then(() => {
            defered.resolve();
        }).catch(() => {
            defered.reject();
        }).finally(() => {

            this.triggerProjectCollectionRefresh();
        });

        return defered.promise;
    }

    public refresh(projectToRefresh: Models.IProject): ng.IPromise<any> {
        let defered = this.$q.defer<any>();

        let project: Project;
        if (!projectToRefresh) {
            throw new Error("Project_NotFound");
        }
        project = this.getProject(projectToRefresh.id);
        if (!project) {
            throw new Error("Project_NotFound");
        }

        let selectedArtifact = this.artifactManager.selection.getArtifact();

        //if selected artifact is dirty and is in the project being refreshed - perform autosave
        let autosavePromise = this.$q.defer<any>();
        if (selectedArtifact.artifactState.dirty && selectedArtifact.projectId === projectToRefresh.id) {
            autosavePromise.promise = selectedArtifact.autosave();
        } else {
            autosavePromise.resolve();
        }

        autosavePromise.promise.then(() => {
            this.doRefresh(project, selectedArtifact, defered, projectToRefresh);
        }).catch(() => {
            //something went wrong - ask user if they want to force refresh
            this.dialogService.confirm(this.localization.get("Confirmation_Continue_Refresh"))
                .then(() => {
                    this.doRefresh(project, selectedArtifact, defered, projectToRefresh);
                })
                .catch(() => {
                    defered.reject();
                });
        });

        return defered.promise;
    }

    private doRefresh(project: Project, expandToArtifact: IStatefulArtifact, defered: any, projectToRefresh: Models.IProject) {
        let selectedArtifactNode = this.getArtifactNode(expandToArtifact.id);

        //if the artifact provided is not in the current project - just expand project node
        if (expandToArtifact.projectId !== projectToRefresh.id) {
            expandToArtifact = this.getArtifact(projectToRefresh.id);
        }

        //try with selected artifact
        this.projectService.getProjectTree(project.id, expandToArtifact.id, selectedArtifactNode.open)
            .then((data: Models.IArtifact[]) => {
                this.ProcessProjectTree(project, data).then(() => {
                    defered.resolve();
                }).catch(() => {
                    this.ClearProject(project);
                    defered.reject();
                });
            }).catch((error: any) => {
            if (!error) {
                this.ClearProject(project);
                defered.reject();
            }

            if (error.statusCode === HttpStatusCode.NotFound && error.errorCode === ProjectServiceStatusCode.ResourceNotFound) {
                //if we're selecting project
                if (expandToArtifact.id === expandToArtifact.projectId) {
                    this.dialogService.alert("Refresh_Project_NotFound");
                    this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(this.getProject(project.id)), 1);
                    defered.reject();
                } else {
                    //try with selected artifact's parent
                    this.projectService.getProjectTree(project.id, expandToArtifact.parentId, true)
                        .then((data: Models.IArtifact[]) => {
                            this.messageService.addWarning("Refresh_Artifact_Deleted");
                            this.ProcessProjectTree(project, data).then(() => {
                                defered.resolve();
                            }).catch(() => {
                                this.ClearProject(project);
                                defered.reject();
                            });
                        }).catch((innerError: any) => {
                        if (innerError.statusCode === HttpStatusCode.NotFound && innerError.errorCode === ProjectServiceStatusCode.ResourceNotFound) {
                            //try it with project
                            this.projectService.getArtifacts(project.id).then((data: Models.IArtifact[]) => {
                                this.messageService.addWarning("Refresh_Artifact_Deleted");
                                this.ProcessProjectTree(project, data).then(() => {
                                    defered.resolve();
                                }).catch(() => {
                                    this.ClearProject(project);
                                    defered.reject();
                                });
                            }).catch((err: any) => {
                                this.dialogService.alert("Refresh_Project_NotFound");
                                this.projectCollection.getValue().splice(this.projectCollection.getValue().indexOf(this.getProject(project.id)), 1);
                                defered.reject();
                            });
                        } else {
                            this.messageService.addError(error["message"]);
                            this.ClearProject(project);
                            defered.reject();
                        }
                    });
                }
            } else {
                this.messageService.addError(error["message"]);
                this.ClearProject(project);
                defered.reject();
            }
        });
    }

    private ProcessProjectTree(project: Project, data: Models.IArtifact[]): ng.IPromise<any> {
        let defered = this.$q.defer<any>();

        const oldProjectId: number = project.id;
        const oldProjectPermissions: number = project.permissions;
        let oldProject = this.getProject(oldProjectId);
        this.artifactManager.removeAll(oldProjectId);

        this.metadataService.get(oldProjectId).then(() => {

            //reload project info
            this.projectService.getProject(oldProjectId).then((result: AdminStoreModels.IInstanceItem) => {

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
                let newProjectNode: Project = new Project(statefulArtifact);

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

                defered.resolve();
            }).catch(() => {
                defered.reject();
            });
        }).catch(() => {
            defered.reject();
        });

        return defered.promise;
    }

    private ClearProject(project: Project) {
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

    public add(data: Models.IProject): ng.IPromise<any> {
        const defered = this.$q.defer<any>();

        if (!data) {
            throw new Error("Project_NotFound"); // need to throw an error as mainView may not be active yet
        }

        let project: Project = this.getProject(data.id);
        if (!project) {
            this.metadataService.get(data.id).then(() => {
                _.assign(data, {
                    projectId: data.id,
                    itemTypeId: Enums.ItemTypePredefined.Project,
                    prefix: "PR",
                    permissions: data.permissions,
                    predefinedType: Enums.ItemTypePredefined.Project,
                    hasChildren: true
                });

                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(data);
                this.artifactManager.add(statefulArtifact);
                project = new Project(statefulArtifact);
                this.projectCollection.getValue().unshift(project);
                this.loadArtifact(project.id).then(() => {
                    defered.resolve();
                }).catch((err: any) => {
                    if (err) {
                        this.messageService.addError(err);
                    }
                    defered.reject(err);
                });
            }).catch((err: any) => {
                if (err) {
                    this.messageService.addError(err);
                }
                defered.reject(err);
            });
        } else { // the project has been loaded already
            defered.resolve();
        }

        return defered.promise;
    }

    public removeArtifact(artifact: IStatefulArtifact) {
        let node: IArtifactNode = this.getArtifactNode(artifact.id);
        if (node) {
            node.parentNode.children = node.parentNode.children.filter((child) => child.id !== artifact.id);
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public remove() {
        const artifact = this.artifactManager.selection.getArtifact();
        if (artifact) {
            const projectId = artifact.projectId;
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
    }

    public removeAll() {
        this.projectCollection.getValue().forEach((project) => {
            this.artifactManager.removeAll(project.projectId);
            project.dispose();
        });
        this.projectCollection.onNext([]);
    }

    public loadArtifact(id: number): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();

        let node: IArtifactNode = this.getArtifactNode(id);
        if (node) {
            this.projectService.getArtifacts(node.projectId, node.artifact.id).then((data: Models.IArtifact[]) => {
                node.children = data.map((it: Models.IArtifact) => {
                    const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                    this.artifactManager.add(statefulArtifact);
                    return new ArtifactNode(statefulArtifact, node);
                });
                node.loaded = true;
                node.open = true;

                this.projectCollection.onNext(this.projectCollection.getValue());
                deferred.resolve();
            }).catch((error: any) => {
                //ignore authentication errors here
                if (error) {
                    this.messageService.addError(error["message"] || "Artifact_NotFound");
                    deferred.reject();
                } else {
                    node.children = [];
                    node.loaded = false;
                    node.open = false;
                    //node.hasChildren = false;
                    this.projectCollection.onNext(this.projectCollection.getValue());
                    deferred.resolve();
                }
            });
        } else {
            throw new Error("Artifact_NotFound"); // need to throw an error as mainView may not be active yet
        }

        return deferred.promise;
    }

    public loadFolders(id?: number): ng.IPromise<AdminStoreModels.IInstanceItem[]> {
        return this.projectService.getFolders(id);
    }

    public getProject(id: number): Project {
        for (let project of this.projectCollection.getValue()) {
            if (project.id === id) {
                return project;
            }
        }
        return undefined;
    }

    public getSelectedProject(): Project {
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
        /* tslint:disable:whitespace */
        for (let i = 0, it: Project; !found && (it = projects[i++]);) {
            found = it.getNode(id);
        }
        return found;
    };

    public getArtifact(id: number): IStatefulArtifact {
        let found = this.getArtifactNode(id);
        return found ? found.artifact : null;
    };


    public getSubArtifact(artifact: number | Models.IArtifact, subArtifactId: number): Models.ISubArtifact {
        let foundArtifact: Models.ISubArtifact;
        //TODO: Needs to be implemented


        return foundArtifact;
    };


    public getDescendantsToBeDeleted(artifact: IStatefulArtifact):  ng.IPromise<Models.IProject> {
        
        const deferred = this.$q.defer<Models.IProject>();
        
        this.projectService.getProject(artifact.projectId).then((project: AdminStoreModels.IInstanceItem) => {
            this.projectService.getArtifacts(project.id, artifact.id).then((data: Models.IArtifact[]) => {
                const result: Models.IProject = {
                    id : project.id,
                    name: project.name,
                    children: data 
                };
                deferred.resolve(result);
                
            }).catch((error) => {
                deferred.reject(error);
            });

        }).catch((error) => {
            deferred.reject(error);
        });;

        return deferred.promise;
    }

}
