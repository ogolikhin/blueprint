import "angular";
import { ILocalizationService, IMessageService } from "../../core";
import { IProjectRepository, Models } from "./project-repository";
import { ISelectionManager, SelectionSource } from "./selection-manager";

export {Models}

export interface IProjectManager {
    // eventManager
    initialize();
    dispose();

    projectCollection: Rx.BehaviorSubject<Models.IProject[]>;

    loadProject(project: Models.IProject): void;
    loadArtifact(project: Models.IArtifact): void;

    loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    closeProject(all?: boolean): void;

    getProject(id: number);

    getArtifact(artifactId: number, project?: Models.IArtifact): Models.IArtifact;

    getSubArtifact(artifact: number | Models.IArtifact, subArtifactId: number): Models.ISubArtifact;

    getArtifactType(artifact: number | Models.IArtifact, project?: number | Models.IProject): Models.IItemType;    

    getArtifactPropertyTypes(artifact: number | Models.IArtifact, subArtifact: Models.ISubArtifact): Models.IPropertyType[];

    getPropertyTypes(project: number, propertyTypeId: number): Models.IPropertyType;

    updateArtifactName(artifact: Models.IArtifact);
}


export class ProjectManager implements IProjectManager {

    private _projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    static $inject: [string] = ["localization", "messageService", "projectRepository", "selectionManager"];
    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private _repository: IProjectRepository,
        private selectionManager: ISelectionManager
    ) {
    }

    public dispose() {
        //clear all Project Manager event subscription
        if (this._projectCollection) {
            this._projectCollection.dispose();
        }
        if (this._currentArtifact) {
            this._currentArtifact.dispose();
        }
    }

    public initialize = () => {
        //subscribe to event
        this.dispose();
        this._projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]);
    }

    public get projectCollection(): Rx.BehaviorSubject<Models.IProject[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]));
    }

    public loadProject = (project: Models.IProject) => {
        try {
            if (!project) {
                throw new Error("Project_NotFound");
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
                this.selectionManager.selection = { source: SelectionSource.Explorer, artifact: _project};
            } else {
                this._repository.getProjectMeta(project.id)
                    .then((result: Models.IProjectMeta) => {
                        if (angular.isArray(result.artifactTypes)) {
                            //add specific types 
                            result.artifactTypes.unshift(
                                <Models.IItemType>{
                                    id: -1,
                                    name: this.localization.get("Label_Project"),
                                    predefinedType: Models.ItemTypePredefined.Project,
                                    customPropertyTypeIds: []
                                },
                                <Models.IItemType>{
                                    id: -2,
                                    name: this.localization.get("Label_Collections"),
                                    predefinedType: Models.ItemTypePredefined.CollectionFolder,
                                    customPropertyTypeIds: []
                                }
                            );
                        }

                        _project = angular.extend({}, project, {meta: result});

                        this._repository.getArtifacts(_project.id)
                            .then((data: Models.IArtifact[]) => {
                                _project = new Models.Project(_project, {
                                    artifacts: data,
                                    loaded: true,
                                    open: true
                                });
                                _projectCollection.unshift(_project);
                                self.projectCollection.onNext(_projectCollection);
                                this.selectionManager.selection = { source: SelectionSource.Explorer, artifact: _project };

                            }).catch((error: any) => {
                                
                                this.messageService.addError(error);
                            });

                    }).catch((error: any) => {
                        this.messageService.addError(error);
                    });


            } 
        } catch (ex) {
            this.messageService.addError(ex["message"] || "Project_NotFound");
        }
    }

    public loadArtifact = (artifact: Models.IArtifact) => {
        try {
            let self = this;
            if (artifact === null) {
                return;
            }
            if (!artifact) {
                throw new Error("Artifact_NotFound");
            }

            artifact = this.getArtifact(artifact.id);
            if (!artifact) {
                throw new Error("Artifact_NotFound");
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
                    this.selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact };

                }).catch((error: any) => {
                    //ignore authentication errors here
                    if (error) {
                        this.messageService.addError(error["message"] || "Artifact_NotFound");
                    } else {
                        angular.extend(artifact, {
                            artifacts: null,
                            hasChildren: true,
                            loaded: false,
                            open: false
                        });
                        self.projectCollection.onNext(self.projectCollection.getValue());
                    }
                });

        } catch (ex) {
            this.messageService.addError(ex["message"] || "Artifact_NotFound");
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public updateArtifactName(artifact: Models.IArtifact) {
        let project = this.projectCollection.getValue().filter(function(it) {
            return it.id === artifact.projectId;
        })[0];
        if (project) {
            let art = project.artifacts.filter(function(it) {
                return it.id === artifact.id;
            })[0];
            if (art) {
                art.name = artifact.name;
            }
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public closeProject = (all: boolean = false) => {
        try {
            var artifact = this.selectionManager.getExplorerSelectedArtifact();
            if (!artifact) {
                throw new Error("Artifact_NotFound");
            }
            let projectsToRemove: Models.IProject[] = [];
            let _projectCollection = this.projectCollection.getValue().filter((it: Models.IProject) => {
                let result = true;
                if (all || it.id === artifact.projectId) {
                    projectsToRemove.push(it);
                    result = false;
                }
                return result;
            });
            if (!projectsToRemove.length) {
                throw new Error("Project_NotFound");
            }

            this.projectCollection.onNext(_projectCollection);
            this.selectionManager.selection = { source: SelectionSource.Explorer, artifact: this.projectCollection.getValue()[0] || null };
        } catch (ex) {
            this.messageService.addError(ex["message"] || "Project_NotFound");
        }

    }

    public loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]> {
        return this._repository.getFolders(id);
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

    public getSubArtifactSystemPropertyTypes(subArtifact: Models.ISubArtifact): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];

        if (!subArtifact) {
            return properties;
        }

        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_Name"),
            propertyTypePredefined: Models.PropertyTypePredefined.Name,
            primitiveType: Models.PrimitiveType.Text,
            isRequired: true
        });

        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_Description"),
            propertyTypePredefined: Models.PropertyTypePredefined.Description,
            primitiveType: Models.PrimitiveType.Text,
            isRichText: true
        });

        if (subArtifact.predefinedType === Models.ItemTypePredefined.Step) {
           properties.push(<Models.IPropertyType>{
               name: "Step Of",
               propertyTypePredefined: Models.PropertyTypePredefined.StepOf,
               primitiveType: Models.PrimitiveType.Choice,               
           });
        }

        if (subArtifact.predefinedType === Models.ItemTypePredefined.GDShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.DDShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.SBShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.UIShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.UCDShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.PROShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.BPShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.GDConnector ||
            subArtifact.predefinedType === Models.ItemTypePredefined.DDConnector ||
            subArtifact.predefinedType === Models.ItemTypePredefined.SBConnector ||
            subArtifact.predefinedType === Models.ItemTypePredefined.UIConnector ||
            subArtifact.predefinedType === Models.ItemTypePredefined.BPConnector ||
            subArtifact.predefinedType === Models.ItemTypePredefined.UCDConnector) {

            properties.push(<Models.IPropertyType>{
                name: "Label",
                propertyTypePredefined: Models.PropertyTypePredefined.Label,
                primitiveType: Models.PrimitiveType.Text,
                isRichText: true
            });
        }

        if (subArtifact.predefinedType === Models.ItemTypePredefined.GDShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.DDShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.SBShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.UIShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.UCDShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.PROShape ||
            subArtifact.predefinedType === Models.ItemTypePredefined.BPShape) {

            properties.push(<Models.IPropertyType>{
                name: "X",
                propertyTypePredefined: Models.PropertyTypePredefined.X,
                primitiveType: Models.PrimitiveType.Number
            });

            properties.push(<Models.IPropertyType>{
                name: "Y",
                propertyTypePredefined: Models.PropertyTypePredefined.Y,
                primitiveType: Models.PrimitiveType.Number
            });

            properties.push(<Models.IPropertyType>{
                name: "Width",
                propertyTypePredefined: Models.PropertyTypePredefined.Width,
                primitiveType: Models.PrimitiveType.Number
            });

            properties.push(<Models.IPropertyType>{
                name: "Height",
                propertyTypePredefined: Models.PropertyTypePredefined.Height,
                primitiveType: Models.PrimitiveType.Number
            });
        }
        return properties;
    }



    public getArtifactPropertyTypes(artifact: number | Models.IArtifact, subArtifact: Models.ISubArtifact): Models.IPropertyType[] {
        let _artifact: Models.IArtifact;
        if (typeof artifact === "number") {
            _artifact = this.getArtifact(artifact as number);
        } else if (artifact) {
            _artifact = artifact as Models.IArtifact;
        }
        if (!_artifact) {
            throw new Error("Artifact_NotFound");
        }
        let _project = this.getProject(_artifact.projectId);
        if (!_project) {
            throw new Error("Project_NotFound");
        }
        if (!_project.meta) {
            throw new Error("Project_MetaDataNotFound");
        }

        let properties: Models.IPropertyType[] = [];
        let itemType: Models.IItemType = this.getArtifactType(_artifact, subArtifact, _project);

        if (!itemType) {
            throw new Error("ArtifactType_NotFound");
        }
                
        
        //create list of system properties
        if (subArtifact) {
            properties = this.getSubArtifactSystemPropertyTypes(subArtifact);
        } else {
            properties = this.getArtifactSystemPropertyTypes(artifact, itemType, _project.meta);
        }

        
        //add custom property types
        _project.meta.propertyTypes.forEach((it: Models.IPropertyType) => {
            if (itemType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                properties.push(it);
            }
        });
        return properties;

    }

    private getArtifactSystemPropertyTypes(artifact: number | Models.IArtifact,
        artifactType: Models.IItemType,
        projectMeta: Models.IProjectMeta): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];

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
                    return (artifactType && (artifactType.predefinedType === it.predefinedType));
                });
            } (projectMeta).map(function (it) {
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
            stringDefaultValue: "Never published", 
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
            dateDefaultValue: "",
            disabled: true
        });

        properties.push(<Models.IPropertyType>{
            name: this.localization.get("Label_Description"),
            propertyTypePredefined: Models.PropertyTypePredefined.Description,
            primitiveType: Models.PrimitiveType.Text,
            isRichText: true
        });
        return properties;
    }


    public getArtifactType(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, project?: Models.IProject): Models.IItemType {
        if (!artifact) {
            throw new Error("Artifact_NotFound");
        }
        if (!project) {
            project = this.getProject(artifact.projectId);
        }
        if (!project) {
            throw new Error("Project_NotFound");
        }
        if (!project.meta) {
            throw new Error("Project_MetaDataNotFound");
        }
        if (subArtifact) {
            let _subArtifactType: Models.IItemType = project.meta.subArtifactTypes.filter((it: Models.IItemType) => {
                return it.id === subArtifact.itemTypeId;
            })[0];

            return _subArtifactType;
        }

        let _artifactType: Models.IItemType = project.meta.artifactTypes.filter((it: Models.IItemType) => {
            return it.id === artifact.itemTypeId;
        })[0];

        return _artifactType;

    }    

    public getPropertyTypes(project: number | Models.IProject, propertyTypeId: number): Models.IPropertyType {
        let _project: Models.IProject;
        if (typeof project === "number") {
            _project = this.getProject(project as number);
        } else if (project) {
            _project = project as Models.IProject;
        }
        if (!_project) {
            throw new Error("Project_NotFound");
        }
        if (!_project.meta) {
            throw new Error("Project_MetaDataNotLoaded");
        }

        let propertyType: Models.IPropertyType = _project.meta.propertyTypes.filter((it: Models.IPropertyType) => {
            return it.id === propertyTypeId;
        })[0];

        return propertyType;

    }
}