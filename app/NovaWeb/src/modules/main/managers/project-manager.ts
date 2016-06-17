import "angular"
import {Helper, ILocalizationService } from "../../core";
import {IMessageService} from "../../shell";
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

    static $inject: [string] = ["localization", "messageService", "projectRepository"];
    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private _repository: IProjectRepository) {

        this.initialize();
    }

    public $onDestroy() {
        //clear all Project Manager event subscription
        this.dispose();
    }

    private dispose() {
        if (this.projectCollection)
            this.projectCollection.dispose();
        if (this.currentProject)
            this.currentProject.dispose();
        if (this.currentArtifact)
            this.currentArtifact.dispose();
    }

    public initialize = () => {
        //subscribe to event
        this.dispose();

        this.projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]);

        this.currentProject = new Rx.BehaviorSubject<Models.IProject>(null);

        this.currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>(null);
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
        if (artifact) {
            if (_currentartifact && _currentartifact.id === artifact.id) {
                return;
            } 

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
                        this.messageService.addError(error["message"] || "error");
                    });
            } 
        } catch (ex) {
            this.messageService.addError(ex["message"] || "error");
        }
    }

    public loadArtifact = (artifact: Models.IArtifact) => {
        try {
            let self = this;
            let _artifact = this.getArtifact(artifact.id);
            if (_artifact) {
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
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || "error");
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || "error");
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public closeProject = (all: boolean = false) => {
        try {

            let projectsToRemove: Models.IProject[] = [];
            let _projectCollection = this.projectCollection.getValue().filter(function (it: Models.IProject) {
                let result = true;
                if (all || it.id === this.currentProject.getValue().id) {
                    projectsToRemove.push(it);
                    result = false;
                }
                return result;
            }.bind(this));

            this.projectCollection.onNext(_projectCollection);
            this.setCurrentArtifact(this.projectCollection.getValue()[0] || null);
        } catch (ex) {
            this.messageService.addError(ex["message"] || "error");
        }

    }

    public getFolders(id?: number) {
        try {
            return this._repository.getFolders(id);
        } catch (ex) {
            this.messageService.addError(ex["message"] || "error");
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
            this.messageService.addError(ex["message"] || "error");
        }
        return artifact;
    }

}