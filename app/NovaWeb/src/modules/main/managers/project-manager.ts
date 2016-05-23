import * as Models from "../models/models";
import {INotificationService} from "../../core/notification";
import {IProjectRepository} from "../services/project-repository";
import {IProjectNotification, SubscriptionEnum } from "../services/project-notification";

export {SubscriptionEnum, Models}

export interface IProjectManager {
    // Notification
    subscribe(type: SubscriptionEnum, func: Function);
    unsubscribe(type: SubscriptionEnum, func: Function);
    notify(type: SubscriptionEnum, ...prms: any[]);

    ProjectCollection: Models.IProject[];
    CurrentProject: Models.IProject;
    CurrentArtifact: Models.IArtifact;

    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    selectArtifact(artifactId: number): Models.IArtifact;
}

export class ProjectManager implements IProjectManager, IProjectNotification {
    private notificationId: string = "projectmanager";
    private _currentProjet: Models.IProject;
    private _currentArtifact: Models.IArtifact;

    static $inject: [string] = ["projectRepository", "notification"];
    constructor(
        private _repository: IProjectRepository,
        private notification: INotificationService) {

        //subscribe to event
        this.subscribe(SubscriptionEnum.ProjectLoad, this.loadProject.bind(this));
        this.subscribe(SubscriptionEnum.ProjectChildrenLoad, this.loadProjectChildren.bind(this));
        this.subscribe(SubscriptionEnum.ProjectClose, this.closeProject.bind(this));
    }

    public subscribe(type: SubscriptionEnum, func: Function) {
        this.notification.attach(this.notificationId, SubscriptionEnum[type], func);
    }

    public unsubscribe(type: SubscriptionEnum, func: Function) {
        this.notification.detach(this.notificationId, SubscriptionEnum[type], func);
    }

    public notify(type: SubscriptionEnum, ...prms: any[]) {
        this.notification.dispatch(this.notificationId, SubscriptionEnum[type], ...prms);
    }


    public set CurrentProject(project: Models.IProject) {
        if (this._currentProjet && project && this._currentProjet.id === project.id) {
            return;
        }
        this._currentProjet = project;
        this.notify(SubscriptionEnum.CurrentProjectChanged, this._currentProjet);
    }

    public get CurrentProject(): Models.IProject {
        return this._currentProjet;
    }

    public set CurrentArtifact(artifact: Models.IArtifact) {
        if (this._currentArtifact && artifact && this._currentArtifact.id === artifact.id) {
            return;
        }
        if (artifact && artifact.projectId !== this._currentProjet.id) {
            let project = this.getProject(this._currentArtifact.projectId);
            if (project) {
                this.CurrentProject = project;
            }
        }
        this._currentArtifact = artifact;
        this.notify(SubscriptionEnum.CurrentArtifactChanged, this._currentArtifact);
    }

    public get CurrentArtifact(): Models.IArtifact {
        return this._currentArtifact;
    }

    public ProjectCollection: Models.IProject[] = [];

    private loadProject = (projectId: number, projectName: string) => {
        let self = this;
        let project = this.getProject(projectId);

        if (project) {
            this.CurrentProject = project;
        } else {
            this._repository.getProject(projectId)
                .then((result: Models.IArtifact[]) => {
                    project = new Models.Project(result);
                    project.id = projectId;
                    project.name = projectName;
                    self.ProjectCollection.unshift(project);
                    self.notify(SubscriptionEnum.ProjectLoaded, project);
                    self.CurrentProject = project;
                }).catch((error: any) => {
                    //TODO: show error
                    console.log(error.message);
                });
        }
    }

    private loadProjectChildren = (projectId: number, artifactId: number) => {
        let self = this;

        this._repository.getProject(projectId, artifactId)
            .then((result: Models.IArtifact[]) => {
                let artifact= self.CurrentProject.getArtifact(artifactId);
                if (artifact) {
                    artifact.artifacts = result;
                    self.notify(SubscriptionEnum.ProjectChildrenLoaded, artifact);
                }
            }).catch(() => {

            });
    }

    private closeProject(allFlag: boolean) {
        let self = this;
        let projectsToRemove: Models.IProject[] = [];
        this.ProjectCollection = this.ProjectCollection.filter(function (it: Models.IProject) {
            let result = true;
            if (allFlag || it.id === self.CurrentProject.id) {
                projectsToRemove.push(it);
                result = false;
            }
            return result;
        });
        self.notify(SubscriptionEnum.ProjectClosed, projectsToRemove);
        this.CurrentProject = this.ProjectCollection[0] || null;
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
        for (let i = 0, project: Models.IProject; project = this.ProjectCollection[i++];) {
            artifact = project.getArtifact(artifactId);
            if (artifact) {
                break;
            }
        }

        return this.CurrentArtifact = artifact;
    }
}