import { ILocalizationService, IMessageService } from "../../core";
import { IStatefulArtifactFactory } from "../artifact-manager/artifact";
import { Project, ArtifactNode } from "./project";
import { IArtifactNode, IStatefulArtifact } from "../models";
import { StatefulArtifact } from "../artifact-manager/artifact";

import { Models, Enums } from "../../main/models";
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
    
    loadArtifact(id: number): void;
    loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    getProject(id: number);
    getArtifactNode(id: number): IArtifactNode;
    getArtifact(id: number): IStatefulArtifact;

    

    // getSubArtifact(artifact: number | Models.IArtifact, subArtifactId: number): Models.ISubArtifact;

    getArtifactType(artifact: number | IStatefulArtifact): Models.IItemType;    

    getArtifactPropertyTypes(id: number, subArtifact?: Models.ISubArtifact): Models.IPropertyType[];

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
                angular.extend(data, {
                    projectId: data.id,
                    prefix: "PR",
                    permissions: 4095,
                    predefinedType: Enums.ItemTypePredefined.Project,
                    hasChildren: true
                });
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
                this.loadArtifact(project.id);

        }).catch((error: any) => {
            this.messageService.addError(error);
        });

    }

    public loadArtifact = (id: number) => {
        let node: IArtifactNode;

        try {
            node = this.getArtifactNode(id);
            if (!node) {
                throw new Error("Artifact_NotFound");
            }

            this.projectService.getArtifacts(node.projectId, node.artifact.id)
                .then((data: Models.IArtifact[]) => {
                    node.children = data.map((it: Models.IArtifact) => {
                        const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                        this.artifactManager.add(statefulArtifact);
                        return new ArtifactNode(statefulArtifact);
                    });
                    node.loaded = true;

                    node.open = true;

                    this.projectCollection.onNext(this.projectCollection.getValue());
                    this.selectionManager.setArtifact(node.artifact, SelectionSource.Explorer);

                }).catch((error: any) => {
                    //ignore authentication errors here
                    if (error) {
                        this.messageService.addError(error["message"] || "Artifact_NotFound");
                    } else {
                        node.children = [];
                        node.loaded = false;
                        node.open = false;
                        //node.hasChildren = false;                        
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

    public getArtifactNode(id: number): IArtifactNode {
        let found: IArtifactNode;
        let projects  = this.projectCollection.getValue();
        for (let i = 0, it: Project; !found && (it = projects[i++]); ) {
            found = it.getNode(id);
        }
        return found;
    };

    public getArtifact(id: number): IStatefulArtifact {
        let found = this.getArtifactNode(id);
        return found ? found.artifact : null;
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



    public getArtifactPropertyTypes(id: number, subArtifact?: Models.ISubArtifact): Models.IPropertyType[] {

        if (!id) {
            throw new Error("Artifact_NotFound");
        }

        let node = this.getArtifactNode(id);
        if (!node) {
            throw new Error("Artifact_NotFound");
        }
        let project = this.getProject(node.projectId);
        if (!project) {
            throw new Error("Project_NotFound");
        }
        let itemtype = project.getArtifactType(node.id);
        let properties: Models.IPropertyType[] = [];

        
        //create list of system properties
        if (subArtifact) {
            properties = this.getSubArtifactSystemPropertyTypes(subArtifact);
        } else {
            properties = this.getArtifactSystemPropertyTypes(node.artifact, itemtype, project.meta );
        }

        
        //add custom property types
        project.meta.propertyTypes.forEach((it: Models.IPropertyType) => {
            if (itemtype.customPropertyTypeIds.indexOf(it.id) >= 0) {
                properties.push(it);
            }
        });
        return properties;

    }

    private getArtifactSystemPropertyTypes(
        artifact: IStatefulArtifact, 
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


    public getArtifactType(id: number): Models.IItemType {
        if (!id) {
            throw new Error("Artifact_NotFound");
        }

        let node = this.getArtifactNode(id);
        if (!node) {
            throw new Error("Artifact_NotFound");
        }
        let project = this.getProject(node.projectId);
        if (!project) {
            throw new Error("Project_NotFound");
        }
        return project.getArtifactType(node.id);
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