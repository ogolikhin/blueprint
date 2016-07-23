import "angular";
import * as moment from "moment";
import {ILocalizationService } from "../../core";
import {IMessageService, Message, MessageType} from "../../shell";
import {IProjectRepository, Models} from "./project-repository";

export {Models}

export interface IProjectManager {
    // eventManager
    initialize();
    dispose();

    projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    currentProject: Rx.BehaviorSubject<Models.IProject>;
    currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;
    isProjectSelected: boolean;
    isArtifactSelected: boolean;

    setCurrentProject(project: Models.IProject): void;
    setCurrentArtifact(artifact: Models.IArtifact): void;

    loadProject(project: Models.IProject): void;
    loadArtifact(project: Models.IArtifact): void;

    loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    closeProject(all?: boolean): void;

    getArtifact(artifactId: number, project?: Models.IArtifact): Models.IArtifact;

    getSubArtifact(artifact: number | Models.IArtifact, subArtifactId: number): Models.ISubArtifact;

    getArtifactType(artifact: number | Models.IArtifact, project?: number | Models.IProject): Models.IItemType;

    getArtifactPropertyTypes(artifact: number | Models.IArtifact): Models.IPropertyType[];

    getSubArtifactPropertyTypes(subArtifact: number | Models.IArtifact): Models.IPropertyType[];

    getPropertyTypes(project: number, propertyTypeId: number): Models.IPropertyType;

}


export class ProjectManager implements IProjectManager {

    private _projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    private _currentProject: Rx.BehaviorSubject<Models.IProject>;
    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    static $inject: [string] = ["localization", "messageService", "projectRepository"];
    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private _repository: IProjectRepository
//        private artifactService: IArtifactService
    ) {
    }

    public dispose() {
        //clear all Project Manager event subscription
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
        
//        this.currentArtifact.subscribeOnNext(this.loadArtifactDetails, this);
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
            var _project = this.getProject(project.id);

            if (_project) {
                _projectCollection = _projectCollection.filter(function (it) {
                    return it !== _project;
                });
                _projectCollection.unshift(_project);
                self.projectCollection.onNext(_projectCollection);
                self.setCurrentArtifact(_project);

            } else {
                this._repository.getArtifacts(project.id)
                    .then((result: Models.IArtifact[]) => {
                        _project = new Models.Project(project, {
                            artifacts: result,
                            loaded: true,
                            open: true
                        });
                        _projectCollection.unshift(_project);
                        self.projectCollection.onNext(_projectCollection);
                        self.loadProjectMeta(_project);
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
            let self = this;
            if (artifact === null) {
                return;
            }
            if (!artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }

            artifact = this.getArtifact(artifact.id);
            if (!artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }

            this._repository.getArtifacts(artifact.projectId, artifact.id)
                .then((result: Models.IArtifact[]) => {
                    angular.extend(artifact, {
                        artifacts: result,
                        hasChildren: true,
                        loaded: true,
                        open: true
                    });
                    self.projectCollection.onNext(self.projectCollection.getValue());
                    self.setCurrentArtifact(artifact);
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                });

        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Artifact_NotFound"));
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }
    
    private loadProjectMeta = (project: Models.IProject) => {
        try {
            if (!project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }

            let self = this;
            this._repository.getProjectMeta(project.id)
                .then((result: Models.IProjectMeta) => {
                    if (angular.isArray(result.artifactTypes)) {
                        //add specific types 
                        result.artifactTypes.unshift(
                            <Models.IItemType>{
                                id: -1,
                                name: Models.ItemTypePredefined[Models.ItemTypePredefined.Project],
                                predefinedType: Models.ItemTypePredefined.Project,
                                customPropertyTypeIds: []
                            },
                            <Models.IItemType>{
                                id: -2,
                                name: Models.ItemTypePredefined[Models.ItemTypePredefined.CollectionFolder],
                                predefinedType: Models.ItemTypePredefined.CollectionFolder,
                                customPropertyTypeIds: []
                            }
                        );
                    }

                    project.meta = result;


                    self.setCurrentProject(project);
                    self.setCurrentArtifact(project);
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Project_NotFound"));
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }
    }

    public closeProject = (all: boolean = false) => {
        try {
            if (!this.currentProject.getValue()) {
                this.messageService.addMessage(new Message(MessageType.Warning, "Not selected projects"));
                return;
            }
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

    public loadFolders(id?: number) {
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

    public getSubArtifact(artifact: number | Models.IArtifact, subArtifactId: number): Models.ISubArtifact {
        let foundArtifact: Models.ISubArtifact;
        //TODO: Needs to be implemented


        return foundArtifact;
    };

    public get isProjectSelected(): boolean {
        //NOTE: current Project must have a refference if project collection has any items
        return !!this.currentProject.getValue();
    }

    public get isArtifactSelected(): boolean {
        return !!this.currentArtifact.getValue();
    }

    public getArtifactPropertyTypes(artifact: number | Models.IArtifact): Models.IPropertyType[] {
        let _artifact: Models.IArtifact;
        if (typeof artifact === "number") {
            _artifact = this.getArtifact(artifact as number);
        } else if (artifact) {
            _artifact = artifact as Models.IArtifact;
        }
        if (!_artifact) {
            throw new Error(this.localization.get("Artifact_NotFound"));
        }
        let _project = this.getProject(_artifact.projectId);
        if (!_project) {
            throw new Error(this.localization.get("Project_NotFound"));
        }
        if (!_project.meta) {
            throw new Error(this.localization.get("Project_MetaDataNotFound"));
        }

        let properties: Models.IPropertyType[] = [];
        let _artifactType: Models.IItemType = this.getArtifactType(_artifact, _project);

        if (!_artifactType) {
            throw new Error(this.localization.get("ArtifactType_NotFound"));
        }
        
        //create list of system properties
        
        
        //add system properties  
        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_Name"),
            propertyTypePredefined: Models.PropertyTypePredefined.Name,
            primitiveType: Models.PrimitiveType.Text,
            isRequired: true
        });

        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_Type"),
            propertyTypePredefined: Models.PropertyTypePredefined.ItemTypeId,
            primitiveType: Models.PrimitiveType.Choice,
            validValues: function (meta: Models.IProjectMeta) {
                return meta.artifactTypes.filter((it: Models.IItemType) => {
                    return (_artifactType && (_artifactType.predefinedType === it.predefinedType));
                });
            } (_project.meta).map(function (it) {
                return <Models.IOption>{
                    id: it.id,
                    value: it.name
                };
            }),
            isRequired: true
        });
        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_CreatedBy"),
            propertyTypePredefined: Models.PropertyTypePredefined.CreatedBy,
            primitiveType: Models.PrimitiveType.User, 
            disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_CreatedOn"),
            propertyTypePredefined: Models.PropertyTypePredefined.CreatedOn,
            primitiveType: Models.PrimitiveType.Date,
            disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_LastEditBy"),
            propertyTypePredefined: Models.PropertyTypePredefined.LastEditedBy,
            primitiveType: Models.PrimitiveType.User,
            disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_LastEditOn"),
            propertyTypePredefined: Models.PropertyTypePredefined.LastEditedOn,
            primitiveType: Models.PrimitiveType.Date,
            disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_Description"),
            propertyTypePredefined: Models.PropertyTypePredefined.Description,
            primitiveType: Models.PrimitiveType.Text,
            isRichText: true
        });
        
        //add custom property types
        _project.meta.propertyTypes.forEach((it: Models.IPropertyType) => {
            if (_artifactType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                properties.push(it);
            }
        });
        return properties;

    }

    public getArtifactType(artifact: Models.IArtifact, project?: Models.IProject): Models.IItemType {
        if (!artifact) {
            throw new Error(this.localization.get("Artifact_NotFound"));
        }
        if (!project) {
            project = this.getProject(artifact.projectId);
        }
        if (!project) {
            throw new Error(this.localization.get("Project_NotFound"));
        }
        if (!project.meta) {
            throw new Error(this.localization.get("Project_MetaDataNotFound"));
        }
        let _artifactType: Models.IItemType = project.meta.artifactTypes.filter((it: Models.IItemType) => {
            return it.id === artifact.itemTypeId || (it.id < 0 && it.predefinedType === artifact.predefinedType);
        })[0];
        
        return _artifactType;
    }

    public getSubArtifactPropertyTypes(subArtifact: number | Models.IArtifact): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];
        //TODO: Needs to be implemented
        return properties;
    }

    public getPropertyTypes(project: number | Models.IProject, propertyTypeId: number): Models.IPropertyType {
        let _project: Models.IProject;
        if (typeof project === "number") {
            _project = this.getProject(project as number);
        } else if (project) {
            _project = project as Models.IProject;
        }
        if (!_project) {
            throw new Error(this.localization.get("Project_NotFound"));
        }
        if (!_project.meta) {
            throw new Error(this.localization.get("Project_MetaDataNotLoaded"));
        }

        let propertyType: Models.IPropertyType = _project.meta.propertyTypes.filter((it: Models.IPropertyType) => {
            return it.id === propertyTypeId;
        })[0];

        return propertyType;

    }
}