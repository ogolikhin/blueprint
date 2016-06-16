import "angular"
import {
    Helper,
    ILocalizationService,
    IEventManager,
    EventSubscriber } from "../../core";
import {IProjectRepository, Models} from "../services/project-repository";

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

    projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    currentProject: Rx.BehaviorSubject<Models.IProject>;
    currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    loadProject(project: Models.IProject): void;
    loadArtifact(project: Models.IArtifact): void;

    closeProject(all?: boolean): void;
    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    getArtifact(artifactId: number, project?: Models.IArtifact): Models.IArtifact;
    updateArtifact(artifact: Models.IArtifact, data: any): Models.IArtifact;
}


export class ProjectManager implements IProjectManager {

    public projectCollection: Rx.BehaviorSubject<Models.IProject[]>;

    public currentProject: Rx.BehaviorSubject<Models.IProject>;

    public currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    static $inject: [string] = ["localization", "eventManager", "projectRepository"];
    constructor(
        private localization: ILocalizationService,
        private eventManager: IEventManager,
        private _repository: IProjectRepository) {

        this.initialize();
    }

    public $onDestroy() {
        //clear all Project Manager event subscription
        this.dispose();
    }

    private dispose() {
        this.projectCollection && this.projectCollection.dispose();
        this.currentProject && this.currentProject.dispose();
        this.currentArtifact && this.currentArtifact.dispose();
    }
    public initialize = () => {
        //subscribe to event
        this.dispose();

        this.projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]);

        this.currentProject = new Rx.BehaviorSubject<Models.IProject>({} as Models.IProject);

        this.currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>({} as Models.IArtifact);
    }

    private setCurrentProject(project: Models.IProject) {
        let _currentproject = this.currentProject.getValue();
        if (_currentproject && project && _currentproject.id === project.id) {
            return;
        }
        this.currentProject.onNext(project);
    }

    private setCurrentArtifact(artifact: Models.IArtifact) {
        let _currentartifact = this.currentArtifact.getValue();
        if (_currentartifact && artifact && _currentartifact.id === artifact.id) {
            return;
        }
        if (artifact && _currentartifact && artifact.projectId !== _currentartifact.id) {
            let project = this.getProject(artifact.projectId);
            if (project) {
                this.setCurrentProject(project);
            }
        }

        this.currentArtifact.onNext(artifact);
    }

    public loadProject = (project: Models.IProject) => {
        try {
            let self = this;
            var _projectCollection: Models.IProject[] = this.projectCollection.getValue();
            let _project = this.getProject(project.id);

            if (_project) {
                _projectCollection = _projectCollection.filter(function (it) {
                    return it !== _project;
                });
                _projectCollection.unshift(_project);
                this.projectCollection.onNext(_projectCollection);
                this.setCurrentArtifact(project);

            } else {
                this._repository.getArtifacts(project.id)
                    .then((result: Models.IArtifact[]) => {
                        _project = new Models.Project(project, { artifacts: result });
                        _project = angular.extend(_project, {
                            loaded: true,
                            open: true
                        });
                        _projectCollection.unshift(_project);
                        self.projectCollection.onNext(_projectCollection);
                        self.setCurrentArtifact(_project);
                    }).catch((error: any) => {
                        this.eventManager.dispatch(EventSubscriber.Main, "exception", error);
                    });
            } 
        } catch (ex) {
            this.eventManager.dispatch(EventSubscriber.Main, "exception", ex);
        }
    }

    public loadArtifact = (artifact: Models.IArtifact) => {
        try {
            let self = this;
            //let project = this.getProject(projectId);
            //if (!project) {
            //    throw new Error(this.localization.get("Project_NotFound"));
            //} 
            let _artifact = this.getArtifact(artifact.id);
            if (!_artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            this._repository.getArtifacts(artifact.projectId, artifact.id)
                .then((result: Models.IArtifact[]) => {
                    angular.extend(_artifact, {
                        artifacts: result,
                        hasChildren: true,
                        loaded: true,
                        open: true
                    });
                    self.projectCollection.onNext(self.projectCollection.getValue());
//                    self.notify(SubscriptionEnum.ProjectChildrenLoaded, artifact);
                }).catch((error: any) => {
                    this.eventManager.dispatch(EventSubscriber.Main, "exception", error);
                });
        } catch (ex) {
            this.eventManager.dispatch(EventSubscriber.Main, "exception", ex);
        }
    }

    public closeProject = (all: boolean = false) => {
        try {
            let self = this;
            let projectsToRemove: Models.IProject[] = [];
            let _projectCollection = this.projectCollection.getValue().filter(function (it: Models.IProject) {
                let result = true;
                if (all || it.id === self.currentProject.getValue().id) {
                    projectsToRemove.push(it);
                    result = false;
                }
                return result;
            });

            this.projectCollection.onNext(_projectCollection);
            this.setCurrentArtifact(this.projectCollection.getValue()[0] || null);
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
        let project = this.projectCollection.getValue().filter(function (it) {
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
            for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = this.projectCollection.getValue()[i++]);) {
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