import * as Data from "./artifacts";
import {IProjectService} from "../services/project.svc";
import {IProjectNotification, SubscriptionEnum } from "../services/project-notification";

export {SubscriptionEnum, Data}

export interface IProjectRepository {
    service: IProjectService;
    Notificator: IProjectNotification;
    ProjectCollection: Data.IProject[];
    CurrentProject: Data.IProject;
}

export class ProjectRepository implements IProjectRepository {
    private _currentProjet: Data.IProject;

    static $inject: [string] = ["projectService", "projectNotification"];
    constructor(
        private _service: IProjectService,
        private notificator: IProjectNotification) {

        //subscribe to event
        this.notificator.subscribe(SubscriptionEnum.ProjectLoad, this.loadProject.bind(this));
        this.notificator.subscribe(SubscriptionEnum.ProjectNodeLoad, this.loadChildren.bind(this));
        this.notificator.subscribe(SubscriptionEnum.ProjectClose, this.closeProject.bind(this));
    }

    public ProjectCollection: Data.IProject[] = [];

    public get service(): IProjectService {
        return this._service;
    }

    public get Notificator(): IProjectNotification {
        return this.notificator;
    }

    public set CurrentProject(project: Data.IProject) {
        this._currentProjet = project;
        this.notificator.notify(SubscriptionEnum.CurrentProjectChanged, this._currentProjet);
    }
    public get CurrentProject(): Data.IProject {
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

            this.service.getProject(projectId)
                .then((result: Data.IProjectItem[]) => {
                    project = new Data.Project(result);
                    project.id = projectId;
                    project.name = projectName;
                    self.ProjectCollection.push(project);
                    self.notificator.notify(SubscriptionEnum.ProjectLoaded, project, false);
                    self.CurrentProject = project;
                }).catch(() => {

                });
        }
    }

    private loadChildren = (projectId: number, artifactId: number) => {
        let self = this;

        this.service.getProject(projectId, artifactId)
            .then((result: Data.IProjectItem[]) => {
                let node = self.CurrentProject.getArtifact(artifactId);
                if (node) {
                    node.artifacts = result;
                    self.notificator.notify(SubscriptionEnum.ProjectNodeLoaded, this.CurrentProject, artifactId);
                }
            }).catch(() => {

            });
    }

    private closeProject(allFlag: boolean) {
        let self = this;
        let projectsToRemove: Data.IProject[] = [];
        this.ProjectCollection = this.ProjectCollection.filter(function (it: Data.IProject) {
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

}