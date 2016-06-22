import "angular";
import {ILocalizationService } from "../../core";
import {IMessageService} from "../../shell";
import {IProjectRepository, Models} from "./project-repository";

export {Models}

export interface IProjectManager {
    // eventManager
    initialize();

    projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    currentProject: Rx.BehaviorSubject<Models.IProject>;
    currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    setCurrentProject(project: Models.IProject): void;
    setCurrentArtifact(artifact: Models.IArtifact): void;

    loadProject(project: Models.IProject): void;
    loadArtifact(project: Models.IArtifact): void;
    loadArtifactDetails(artifact: Models.IArtifact): void;

    closeProject(all?: boolean): void;
    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    getArtifact(artifactId: number, project?: Models.IArtifact): Models.IArtifact;
    updateArtifact(artifact: Models.IArtifact, data: any): Models.IArtifact;
}


export class ProjectManager implements IProjectManager {

    private _projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    private _currentProject: Rx.BehaviorSubject<Models.IProject>;
    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    static $inject: [string] = ["localization", "messageService", "projectRepository"];
    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private _repository: IProjectRepository) {
    }

    public $onInit() {
        this.initialize();
    }

    public $onDestroy() {
        //clear all Project Manager event subscription
        this.dispose();
    }

    private dispose() {
        if (this.projectCollection) {
            this.projectCollection.dispose();
        }
        if (this.currentProject) {
            this.currentProject.dispose();
        }
        if (this.currentArtifact) {
            this.currentArtifact.dispose();
        }
    }

    public initialize = () => {
        //subscribe to event
        this.dispose();
        this._projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]);
        this._currentProject = new Rx.BehaviorSubject<Models.IProject>(null);
        this._currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>(null);
        
        this.currentProject.subscribeOnNext(this.loadProjectMeta, this);
    }

    public get projectCollection(): Rx.BehaviorSubject<Models.IProject[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]));
    }
    public get currentProject(): Rx.BehaviorSubject<Models.IProject> {
        return this._currentProject || (this._currentProject = new Rx.BehaviorSubject<Models.IProject>(null));
    }

    public get currentArtifact(): Rx.BehaviorSubject<Models.IArtifact> {
        return this._currentArtifact || (this._currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>(null));
    }

    public setCurrentProject(project: Models.IProject) {
        if (project) {
            let _currentproject = this.currentProject.getValue();
            if (_currentproject && _currentproject.id === project.id) {
                return;
            }
        }
        this.currentProject.onNext(project);
    }

    public setCurrentArtifact(artifact: Models.IArtifact) {
        if (artifact) {
            let _currentartifact = this.currentArtifact.getValue();
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
            if (!project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }
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
                        this.messageService.addError(error["message"] || this.localization.get("Project_NotFound"));
                    });
            } 
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }
    }

    public loadArtifact = (artifact: Models.IArtifact) => {
        try {
            if (!artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            let _project = this.getProject(artifact.projectId);
            if (!_project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }
            let self = this;
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
                    self.setCurrentArtifact(_artifact);
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Artifact_NotFound"));
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public loadArtifactDetails = (artifact: Models.IArtifact) => {
        try {
        
            if (!artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            let _project = this.getProject(artifact.projectId);
            if (!_project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }
            let self = this;
            let _artifact = this.getArtifact(artifact.id);
            if (!_artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            this._repository.getArtifactDetails(artifact.id)
                .then((result: Models.IArtifactDetails) => {
                    
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Artifact_NotFound"));
        }
    }

    public loadProjectMeta = (project: Models.IProject) => {
        try {
            if (project === null) {
                return;
            }
            if (!project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }
            let self = this;
            this._repository.getProjectMeta(project.id)
                .then((result: Models.IProjectMeta) => {
                    project = self.currentProject.getValue();
                    project.meta = result;
                    self.setCurrentProject(project);
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Artifact_NotFound"));
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
            this.setCurrentProject(this.projectCollection.getValue()[0] || null);
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }

    }

    public getFolders(id?: number) {
        try {
            return this._repository.getFolders(id);
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
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
            for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = project.artifacts[i++]); ) {
                if (it.id === id) {
                    foundArtifact = it;
                } else if (it.artifacts) {
                    foundArtifact = this.getArtifact(id, it);
                }
            }
        } else {
            for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = this.projectCollection.getValue()[i++]); ) {
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
            this.messageService.addError(ex["message"]);
        }
        return artifact;
    }

}