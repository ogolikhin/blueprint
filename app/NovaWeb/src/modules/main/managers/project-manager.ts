import * as Models from "../models/models";
import {ILocalizationService} from "../../core/localization";
import {IEventManager, EventSubscriber} from "../../core/event-manager";
import {IProjectRepository} from "../services/project-repository";

export {Models}

export enum SubscriptionEnum { 
    PropertyChanged,
    ProjectChanged,
    ArtifactChanged,
    ProjectLoad,
    ProjectLoaded,
    ProjectChildrenLoad,
    ProjectChildrenLoaded,
    ProjectClose,
    ProjectClosed
}

export interface IProjectManager {
    // eventManager
    initialize();
    subscribe(type: SubscriptionEnum, func: Function);
    unsubscribe(type: SubscriptionEnum, func: Function);
    notify(type: SubscriptionEnum, ...prms: any[]);

    ProjectCollection: Models.IProject[];
    CurrentProject: Models.IProject;
    CurrentArtifact: Models.IArtifact;

    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    getArtifact(artifactId: number, project?: Models.IArtifact): Models.IArtifact;
    updateArtifact(artifact: Models.IArtifact, data: any): Models.IArtifact;
}


export class ProjectManager implements IProjectManager {
    private _listeners: string[] = [];
    private _currentProject: Models.IProject;
    private _currentArtifact: Models.IArtifact;
    private _projectCollection: Models.IProject[];


    static $inject: [string] = ["localization", "eventManager", "projectRepository"];
    constructor(
        private localization: ILocalizationService,
        private eventManager: IEventManager,
        private _repository: IProjectRepository) {

        //subscribe to event
        this._listeners = [
            this.subscribe(SubscriptionEnum.PropertyChanged, this.onPropertyChanged.bind(this)),
            this.subscribe(SubscriptionEnum.ProjectLoad, this.loadProject.bind(this)),
            this.subscribe(SubscriptionEnum.ProjectChildrenLoad, this.loadProjectChildren.bind(this)),
            this.subscribe(SubscriptionEnum.ProjectClose, this.closeProject.bind(this)),
        ];
    }

    public $onDestroy() {
        //clear all Project Manager event subscription
        this._listeners.map(function (it) {
            this.eventManager.detachById(it);
        }.bind(this));
    }

    public initialize = () => {
        this._projectCollection = [];
        this._currentProject = null;
        this._currentArtifact = null;
    }

    public subscribe(type: SubscriptionEnum, func: Function): string {
        return this.eventManager.attach(EventSubscriber.ProjectManager, SubscriptionEnum[type], func);
    }

    public unsubscribe(type: SubscriptionEnum, func: Function) {
        this.eventManager.detach(EventSubscriber.ProjectManager, SubscriptionEnum[type], func);
    }

    public notify(type: SubscriptionEnum, ...prms: any[]) {
        this.eventManager.dispatch(EventSubscriber.ProjectManager, SubscriptionEnum[type], ...prms);
    }

    private onPropertyChanged(item: any, propertyName: string, value: any, oldValue: any) {

    }

    public set CurrentProject(project: Models.IProject) {
        if (this._currentProject && project && this._currentProject.id === project.id) {
            return;
        }
        this._currentProject = project;
        this.notify(SubscriptionEnum.ProjectChanged, this._currentProject);
    }

    public get CurrentProject(): Models.IProject {
        return this._currentProject;
    }

    public set CurrentArtifact(artifact: Models.IArtifact) {
        if (artifact && this._currentArtifact && this._currentArtifact.id === artifact.id) {
            return;
        }
        if (artifact && artifact.projectId !== this._currentProject.id) {
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
        if (!this._projectCollection) {
            this._projectCollection = [];
        }
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
                this.CurrentProject = project;
                this.CurrentArtifact = project;

                this.notify(SubscriptionEnum.ProjectLoaded, project);
            } else {
                this._repository.getArtifacts(projectId)
                    .then((result: Models.IArtifact[]) => {
                        project = new Models.Project(projectId, projectName, result);
                        self._projectCollection.unshift(project);
                        self.CurrentProject = project;
                        self.CurrentArtifact = project;
                        self.notify(SubscriptionEnum.ProjectLoaded, project);
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
            let artifact = this.getArtifact(artifactId, project);
            if (!artifact) {
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
        try {
            return this._repository.getFolders(id);
        } catch (ex) {
            this.eventManager.dispatch(EventSubscriber.Main, "exception", ex);
        }
    }

    public getProject(id: number) {
        let project = this.ProjectCollection.filter(function (it) {
            return it.id === id;
        })[0];
        return project;
    }

    public getArtifact(id: number, project?: Models.IArtifact): Models.IArtifact {
        let foundArtifact: Models.IArtifact;
        if (project) {
            if (project.id === id) {
                foundArtifact = project;
            }
            for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = project.artifacts[i++]);) {
                if (it.id === id) {
                    foundArtifact = it;
                } else if (it.artifacts) {
                    foundArtifact = this.getArtifact(id, it);
                }
            }
        } else {
            for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = this.ProjectCollection[i++]);) {
                foundArtifact = this.getArtifact(id, it);
            }
        }
        return foundArtifact;
    };

    public updateArtifact(artifact: Models.IArtifact, data?: any): Models.IArtifact {
        try {
            if (artifact && data) {
                angular.extend(artifact, data);
            }
        } catch (ex) {
            this.eventManager.dispatch(EventSubscriber.Main, "exception", ex);
        }
        return artifact;
    }

}