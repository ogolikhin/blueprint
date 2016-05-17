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

    private addProject = (project: Data.IProject) : Data.IProject => {
        this.ProjectCollection.push(project);
        this.CurrentProject = project;
        return project;
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
                    self.addProject(project);
                    self.notificator.notify(SubscriptionEnum.ProjectLoaded, self.CurrentProject, false);
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


}