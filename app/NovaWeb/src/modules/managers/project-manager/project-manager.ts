﻿import { ILocalizationService, IMessageService } from "../../core";
import { Project, ProjectArtifact } from "./project";
import { IProjectArtifact, IStatefulArtifact } from "../models";
import { IStatefulArtifactFactory } from "../artifact-manager/artifact";

import { Models } from "../../main/models";
import { IProjectService } from "./project-service";
import { ISelectionManager, SelectionSource } from "../selection-manager";

import { IArtifactManager } from "../../managers";

export interface IProjectManager {
    // eventManager
    initialize();
    
    dispose();

    projectCollection: Rx.BehaviorSubject<Project[]>;

    add(data: Models.IProject);
    remove(all?: boolean): void;

//    loadProject(project: Models.IProject): void;
    
    loadArtifact(artifact: number | IProjectArtifact): void;

    loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    getProject(id: number);

    getArtifact(id: number, project?: Project): IProjectArtifact;

    // getSubArtifact(artifact: number | Models.IArtifact, subArtifactId: number): Models.ISubArtifact;

    getArtifactType(artifact: number | IStatefulArtifact): Models.IItemType;    

    // getArtifactPropertyTypes(artifact: number | Models.IArtifact, subArtifact: Models.ISubArtifact): Models.IPropertyType[];

    // getPropertyTypes(project: number, propertyTypeId: number): Models.IPropertyType;

    // updateArtifactName(artifact: Models.IArtifact);
}


export class ProjectManager  implements IProjectManager { 

    private _projectCollection: Rx.BehaviorSubject<Project[]>;

    static $inject: [string] = [
        "localization", 
        "messageService", 
        "projectService", 
        "artifactManager", 
        "selectionManager2",
        "statefulArtifactFactory"
    ];

    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private projectService: IProjectService,
        private artifactManager: IArtifactManager,
        private selectionManager: ISelectionManager,
        private statefulArtifactFactory: IStatefulArtifactFactory
    ) {
    }

    public dispose() {
        //clear all Project Manager event subscription
        if (this._projectCollection) {
            this._projectCollection.dispose();
        }
    }

    public initialize = () => {
        //subscribe to event
        this.dispose();
        delete this._projectCollection ;
    }

    public get projectCollection(): Rx.BehaviorSubject<Project[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Project[]>([]));
    }

    public add(data: Models.IProject) {
        let project: Project;
        try {    
            if (!data) {
                throw new Error("Project_NotFound");
            }
            project = this.getProject(data.id);
            if (project) {
                //todo move project to first position

            } else {
                angular.extend(data, {hasChildren: true});
                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(data);
                this.artifactManager.add(statefulArtifact);
                project = new Project(statefulArtifact);
                this.projectCollection.getValue().unshift(project);
                this.loadProject(project);

            }

        } catch (ex) {
            this.messageService.addError(ex["message"] || "Project_NotFound");
        }
    }

    public remove = (all: boolean = false) => {
        try {
            var project = this.projectCollection.getValue()[0] ;
            if (!project) {
                throw new Error("Project_NotFound");
            }
            let projectsToRemove: Project[] = [];
            let _projectCollection = this.projectCollection.getValue().filter((it: Project) => {
                let result = true;
                if (all || it.id === project.projectId) {
                    projectsToRemove.push(it);
                    result = false;
                }
                return result;
            });
            if (!projectsToRemove.length) {
                throw new Error("Project_NotFound");
            }

            this.projectCollection.onNext(_projectCollection);
//            this.selectionManager.selection = { source: SelectionSource.Explorer, artifact: this.projectCollection.getValue()[0] || null };
        } catch (ex) {
            this.messageService.addError(ex["message"] || "Project_NotFound");
        }

    }

    private loadProject = (project: Project) => {
        
        this.projectService.getProjectMeta(project.id)
            .then((metadata: Models.IProjectMeta) => {
                if (angular.isArray(metadata.artifactTypes)) {
                    //add specific types 
                    metadata.artifactTypes.unshift(
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
                project.meta = metadata;
                //load project children
                this.loadArtifact(project);

        }).catch((error: any) => {
            this.messageService.addError(error);
        });

    }

    public loadArtifact = (artifact: number | IProjectArtifact) => {
        let projectArtifact: IProjectArtifact;

        try {
            if (angular.isNumber(artifact)) {
                projectArtifact = this.getArtifact(artifact);
            } else {
                projectArtifact = artifact;
            }
            if (!projectArtifact) {
                throw new Error("Artifact_NotFound");
            }

            this.projectService.getArtifacts(projectArtifact.projectId, projectArtifact.artifact.id)
                .then((data: Models.IArtifact[]) => {
                    projectArtifact.children = data.map((it: Models.IArtifact) => {
                        const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                        this.artifactManager.add(statefulArtifact);
                        
                        return new ProjectArtifact(statefulArtifact, projectArtifact);
                    });
                    projectArtifact.loaded = true;
                    projectArtifact.open = true;

                    this.projectCollection.onNext(this.projectCollection.getValue());
                    // this.selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact };
                    this.selectionManager.setArtifact(projectArtifact.artifact, SelectionSource.Explorer);

                }).catch((error: any) => {
                    //ignore authentication errors here
                    if (error) {
                        this.messageService.addError(error["message"] || "Artifact_NotFound");
                    } else {
                        projectArtifact.children = [];
                        projectArtifact.loaded = false;
                        projectArtifact.open = false;
                        //projectArtifact.hasChildren = false;                        
                        this.projectCollection.onNext(this.projectCollection.getValue());
                    }
                });

        } catch (ex) {
            this.messageService.addError(ex["message"] || "Artifact_NotFound");
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    // public updateArtifactName(artifact: Models.IArtifact) {
    //     let project = this.projectCollection.getValue().filter(function(it) {
    //         return it.id === artifact.projectId;
    //     })[0];
    //     if (project) {
    //         let art = project.artifacts.filter(function(it) {
    //             return it.id === artifact.id;
    //         })[0];
    //         if (art) {
    //             art.name = artifact.name;
    //         }
    //         this.projectCollection.onNext(this.projectCollection.getValue());
    //     }
    // }


    public loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]> {
        return this.projectService.getFolders(id);
    }

    public getProject(id: number): Project {
        let project = this.projectCollection.getValue().filter(function (it) {
            return it.id === id;
        })[0];
        return project;
    }

    public getArtifact(id: number, project?: Project): IProjectArtifact {
        let foundArtifact: IProjectArtifact;
        let projects  = this.projectCollection.getValue();
        for (let i = 0, it: Project; !foundArtifact && (it = projects[i++]); ) {
            foundArtifact = it.getArtifact(id);
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



    // public getArtifactPropertyTypes(artifact: number | IProjectArtifact, subArtifact: Models.ISubArtifact): Models.IPropertyType[] {
    //     let _artifact: Models.IArtifact;
    //     if (typeof artifact === "number") {
    //         _artifact = this.getArtifact(artifact as number);
    //     } else if (artifact) {
    //         _artifact = artifact as Models.IArtifact;
    //     }
    //     if (!_artifact) {
    //         throw new Error("Artifact_NotFound");
    //     }
    //     let _project = this.getProject(_artifact.projectId);
    //     if (!_project) {
    //         throw new Error("Project_NotFound");
    //     }
    //     if (!_project.meta) {
    //         throw new Error("Project_MetaDataNotFound");
    //     }

    //     let properties: Models.IPropertyType[] = [];
    //     let itemType: Models.IItemType = this.getArtifactType(_artifact, subArtifact, _project);

    //     if (!itemType) {
    //         throw new Error("ArtifactType_NotFound");
    //     }
                
        
    //     //create list of system properties
    //     if (subArtifact) {
    //         properties = this.getSubArtifactSystemPropertyTypes(subArtifact);
    //     } else {
    //         properties = this.getArtifactSystemPropertyTypes(artifact, itemType, _project.meta);
    //     }

        
    //     //add custom property types
    //     _project.meta.propertyTypes.forEach((it: Models.IPropertyType) => {
    //         if (itemType.customPropertyTypeIds.indexOf(it.id) >= 0) {
    //             properties.push(it);
    //         }
    //     });
    //     return properties;

    // }

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


    public getArtifactType(it: number | IStatefulArtifact): Models.IItemType {
        if (!it) {
            throw new Error("Artifact_NotFound");
        }
        let artifact: IStatefulArtifact;

        if (angular.isNumber(it)) {
            artifact = this.getArtifact(it);
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