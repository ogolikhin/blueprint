import * as Models from "../models/models";
import {ILocalizationService} from "../../core/localization";
import {IEventManager, EventSubscriber} from "../../core/event-manager";
import {IProjectRepository} from "../services/project-repository";

export {Models}

export enum SubscriptionEnum { 
    ProjectLoad,
    ProjectLoaded,
    ProjectChildrenLoad,
    ProjectChildrenLoaded,
    ProjectClose,
    ProjectClosed,
    ProjectChanged,
    ArtifactChanged
}

export interface IProjectManager {
    // eventManager
    subscribe(type: SubscriptionEnum, func: Function);
    unsubscribe(type: SubscriptionEnum, func: Function);
    notify(type: SubscriptionEnum, ...prms: any[]);

    ProjectCollection: Models.IProject[];
    CurrentProject: Models.IProject;
    CurrentArtifact: Models.IArtifact;

    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    selectArtifact(artifactId: number): Models.IArtifact;
}


export class ProjectManager implements IProjectManager {
    private _currentProjet: Models.IProject;
    private _currentArtifact: Models.IArtifact;
    private _projectCollection: Models.IProject[];

    static $inject: [string] = ["localization", "eventManager", "projectRepository"];
    constructor(
        private localization: ILocalizationService,
        private eventManager: IEventManager,
        private _repository: IProjectRepository) {

        //subscribe to event
        this.subscribe(SubscriptionEnum.ProjectLoad, this.loadProject.bind(this));
        this.subscribe(SubscriptionEnum.ProjectChildrenLoad, this.loadProjectChildren.bind(this));
        this.subscribe(SubscriptionEnum.ProjectClose, this.closeProject.bind(this));
    }

    public subscribe(type: SubscriptionEnum, func: Function) {
        this.eventManager.attach(EventSubscriber.ProjectManager, SubscriptionEnum[type], func);
    }

    public unsubscribe(type: SubscriptionEnum, func: Function) {
        this.eventManager.detach(EventSubscriber.ProjectManager, SubscriptionEnum[type], func);
    }

    public notify(type: SubscriptionEnum, ...prms: any[]) {
        this.eventManager.dispatch(EventSubscriber.ProjectManager, SubscriptionEnum[type], ...prms);
    }


    public set CurrentProject(project: Models.IProject) {
        if (this._currentProjet && project && this._currentProjet.id === project.id) {
            return;
        }
        this._currentProjet = project;
        this.notify(SubscriptionEnum.ProjectChanged, this._currentProjet);
    }

    public get CurrentProject(): Models.IProject {
        return this._currentProjet;
    }

    public set CurrentArtifact(artifact: Models.IArtifact) {
        if (artifact && angular.isDefined(this._currentArtifact) && this._currentArtifact.id === artifact.id) {
            return;
        }
        if (artifact && artifact.projectId !== this._currentProjet.id) {
            let project = this.getProject(artifact.projectId);
            if (project) {
                this.CurrentProject = project;
            }
        }
        this._currentArtifact = artifact;
        this.notify(SubscriptionEnum.ArtifactChanged, this._currentArtifact);
    }

    public get CurrentArtifact(): Models.IArtifact {
        return this._currentArtifact;
    }

    public get ProjectCollection(): Models.IProject[] {
        if (!this._projectCollection)
            this._projectCollection = [];
        return this._projectCollection;
    }

    private loadProject = (projectId: number, projectName: string) => {
        try {
            let self = this;
            let project = this.getProject(projectId);

            if (project) {
                this._projectCollection = this._projectCollection.filter(function (it) {
                    return it !== project;
                });
                this._projectCollection.unshift(project);
                self.notify(SubscriptionEnum.ProjectLoaded, project);
                this.CurrentProject = project;
            } else {
                this._repository.getArtifacts(projectId)
                    .then((result: Models.IArtifact[]) => {
                        project = new Models.Project(projectId, projectName, result);
                        self._projectCollection.unshift(project);
                        self.notify(SubscriptionEnum.ProjectLoaded, project);
                        self.CurrentProject = project;
                    }).catch((error: any) => {
                        this.eventManager.dispatch(EventSubscriber.Main, "exception", error);
                    });
            } 
        } catch (ex) {
            this.eventManager.dispatch(EventSubscriber.Main, "exception", ex);
        }
    }

    private loadProjectChildren = (projectId: number, artifactId: number) => {
        try {
            let self = this;
            let project = this.getProject(projectId);
            if (!project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }
            let artifact = project.getArtifact(artifactId);
            if (!artifact)
            {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            this._repository.getArtifacts(projectId, artifactId)
                .then((result: Models.IArtifact[]) => {
                    artifact.artifacts = result;
                    self.notify(SubscriptionEnum.ProjectChildrenLoaded, artifact);
                }).catch((error: any) => {
                    this.eventManager.dispatch(EventSubscriber.Main, "exception", error);
                });
        } catch (ex) {
            this.eventManager.dispatch(EventSubscriber.Main, "exception", ex);
        }
    }

    private closeProject(allFlag: boolean) {
        try {
            let self = this;
            let projectsToRemove: Models.IProject[] = [];
            this._projectCollection = this._projectCollection.filter(function (it: Models.IProject) {
                let result = true;
                if (allFlag || it.id === self.CurrentProject.id) {
                    projectsToRemove.push(it);
                    result = false;
                }
                return result;
            });
            self.notify(SubscriptionEnum.ProjectClosed, projectsToRemove);
            this.CurrentProject = this.ProjectCollection[0] || null;
        } catch (ex) {
            this.eventManager.dispatch(EventSubscriber.Main, "exception", ex);
        }

    }

    public getFolders(id?: number) {
        return this._repository.getFolders(id);
    }

    public getProject(id: number) {
        let project = this.ProjectCollection.filter(function (it) {
            return it.id === id;
        })[0];
        return project;
    }
    public selectArtifact(artifactId: number): Models.IArtifact {
        let artifact: Models.IArtifact;
        for (let i = 0, project: Models.IProject; project = this.ProjectCollection[i++]; ) {
            artifact = project.getArtifact(artifactId);
            if (artifact) {
                break;
            }
        }

        return this.CurrentArtifact = artifact;
    }
}