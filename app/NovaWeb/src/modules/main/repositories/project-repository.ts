﻿import * as Models from "../models/models";
import {IProjectService} from "../services/project.svc";
import {IProjectNotification, SubscriptionEnum } from "../services/project-notification";

export {SubscriptionEnum, Models}

export interface IProjectRepository {
    //service: IProjectService;
    Notificator: IProjectNotification;
    ProjectCollection: Models.IProject[];
    CurrentProject: Models.IProject;

    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>
}

export class ProjectRepository implements IProjectRepository {
    private _currentProjet: Models.IProject;

    static $inject: [string] = ["projectService", "projectNotification"];
    constructor(
        private _service: IProjectService,
        private notificator: IProjectNotification) {

        //subscribe to event
        this.notificator.subscribe(SubscriptionEnum.ProjectLoad, this.loadProject.bind(this));
        this.notificator.subscribe(SubscriptionEnum.ProjectChildrenLoad, this.loadProjectChildren.bind(this));
        this.notificator.subscribe(SubscriptionEnum.ProjectClose, this.closeProject.bind(this));
    }

    public ProjectCollection: Models.IProject[] = [];

    public get Notificator(): IProjectNotification {
        return this.notificator;
    }

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
            this._service.getProject(projectId)
                .then((result: Models.IProjectItem[]) => {
                    project = new Models.Project(result);
                    project.id = projectId;
                    project.name = projectName;
                    self.ProjectCollection.push(project);
                    self.notificator.notify(SubscriptionEnum.ProjectLoaded, project, false);
                    self.CurrentProject = project;
                }).catch(() => {

                });
        }
    }

    private loadProjectChildren = (projectId: number, artifactId: number) => {
        let self = this;

        this._service.getProject(projectId, artifactId)
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
        return this._service.getFolders(id);
    }
}