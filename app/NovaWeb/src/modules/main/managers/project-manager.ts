import * as Models from "../models/models";
import {IProjectRepository} from "../services/project-repository";
import {IProjectNotification, SubscriptionEnum } from "../services/project-notification";

export {SubscriptionEnum, Models}

export interface IProjectManager {
    // Notification
    subscribe(type: SubscriptionEnum, func: Function);
    notify(type: SubscriptionEnum, ...prms: any[]);

    ProjectCollection: Models.IProject[];
    CurrentProject: Models.IProject;

    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;
}

export class ProjectManager implements IProjectManager {
    private _currentProjet: Models.IProject;

    static $inject: [string] = ["projectRepository", "projectNotification"];
    constructor(
        private _repository: IProjectRepository,
        private notificator: IProjectNotification) {

        //subscribe to event
        this.notificator.subscribe(SubscriptionEnum.ProjectLoad, this.loadProject.bind(this));
        this.notificator.subscribe(SubscriptionEnum.ProjectChildrenLoad, this.loadProjectChildren.bind(this));
        this.notificator.subscribe(SubscriptionEnum.ProjectClose, this.closeProject.bind(this));
    }

    public subscribe(type: SubscriptionEnum, func: Function) {
        this.notificator.subscribe(type, func);
    };
    public notify(type: SubscriptionEnum, ...prms: any[]) {
        this.notificator.notify(type, ...prms);
    };

    public ProjectCollection: Models.IProject[] = [];

    public set CurrentProject(project: Models.IProject) {
        this._currentProjet = project;
        this.notificator.notify(SubscriptionEnum.CurrentProjectChanged, this._currentProjet);
    }

    public get CurrentProject(): Models.IProject {
        return this._currentProjet;
    }

    private loadProject = (projectId: number, projectName: string) => {
        let self = this;
        let project = this.ProjectCollection.filter(function (it) {
            return it.id === projectId;
        })[0];

        if (project) {
            this.CurrentProject = project;
            self.notificator.notify(SubscriptionEnum.ProjectLoaded, self.CurrentProject, true);
        } else {
            this._repository.getProject(projectId)
                .then((result: Models.IProjectItem[]) => {
                    project = new Models.Project(result);
                    project.id = projectId;
                    project.name = projectName;
                    self.ProjectCollection.push(project);
                    self.notificator.notify(SubscriptionEnum.ProjectLoaded, project, false);
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
            .then((result: Models.IProjectItem[]) => {
                let node = self.CurrentProject.getArtifact(artifactId);
                if (node) {
                    node.artifacts = result;
                    self.notificator.notify(SubscriptionEnum.ProjectChildrenLoaded, this.CurrentProject, artifactId);
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
        self.notificator.notify(SubscriptionEnum.ProjectClosed, projectsToRemove);
        this.CurrentProject = this.ProjectCollection[0] || null;
    }

    public getFolders(id?: number) {
        return this._repository.getFolders(id);
    }
}