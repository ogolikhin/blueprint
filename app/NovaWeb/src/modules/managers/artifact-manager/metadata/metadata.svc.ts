import * as angular from "angular";
import { ILocalizationService } from "../../../core";
import { Models } from "../../../main/models";

export interface IMetaDataService {
    load(projectId?: number);
    get(projectId?: number);
    remove(projectId?: number);
    getArtifactItemType(itemTypeId: number, projectId: number): Models.IItemType;
    getSubArtifactItemType(projectId: number, itemTypeId: number): Models.IItemType;
    getArtifactPropertyTypes(projectId: number, itemTypeId: number): Models.IPropertyType[];
    getSubArtifactPropertyTypes(projectId: number, itemTypeId: number): Models.IPropertyType[];
}


class ProjectMetaData {
    constructor(public id: number, public data: Models.IProjectMeta) {

    }
}


export class MetaDataService implements IMetaDataService {

    private collection: ProjectMetaData[] = [];
    
    public static $inject = [
        "$q",
        "$http",
        "localization"
    ];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private localization: ILocalizationService) {
    }

    public load(projectId?: number, force: boolean = false): ng.IPromise<ProjectMetaData> {
        let deferred = this.$q.defer<ProjectMetaData>();
        let metadata = this.get(projectId);
        if (!force && metadata ) {
            deferred.resolve(metadata);
        } else {
            let url: string = `svc/artifactstore/projects/${projectId}/meta/customtypes`;

            this.$http.get<Models.IProjectMeta>(url).then(
                (result: ng.IHttpPromiseCallbackArg<Models.IProjectMeta>) => {
                    if (angular.isArray(result.data.artifactTypes)) {
                        //add specific types 
                        result.data.artifactTypes.unshift(
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
                    metadata = new ProjectMetaData(projectId, result.data);
                    this.collection.push(metadata); 
                    deferred.resolve(metadata);
                },
                (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                    if (!errResult) {
                        deferred.reject();
                        return;
                    }
                    var error = {
                        statusCode: errResult.status,
                        message: "Project_NotFound"
                    };
                    deferred.reject(error);
                }
                
            );

        }

        return deferred.promise;
    }

    public get(projectId: number): ProjectMetaData {
        return this.collection.filter((it: ProjectMetaData) => it.id === projectId)[0];
    }    

    public remove(projectId: number) {
        this.collection = this.collection.filter((it: ProjectMetaData) => {
            return it.id !== projectId;
        });
    }
    

    public getArtifactItemType(projectId: number, itemTypeId: number): Models.IItemType {
        let itemType = {} as  Models.IItemType;
        let metadata = this.get(projectId);
        if (metadata) {
            itemType = metadata.data.artifactTypes.filter((it: Models.IItemType) => {
                return it.id === itemTypeId;
            })[0];
        } 
        
        return itemType;
    }

    public getSubArtifactItemType(projectId: number, itemTypeId: number): Models.IItemType {
        let itemType = {} as  Models.IItemType;
        let metadata = this.get(projectId);
        if (metadata) {
            itemType = metadata.data.subArtifactTypes.filter((it: Models.IItemType) => {
                return it.id === itemTypeId;
            })[0];
        } 
        
        return itemType;
    }


    public getArtifactPropertyTypes(projectId: number, itemTypeId: number): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];

        let projectMeta = this.get(projectId);
        let itemType = this.getArtifactItemType(projectId, itemTypeId);        

        properties.push(...this.getArtifactSystemPropertyTypes(projectMeta, itemType));
        //add custom property types
        properties.push(...this.getCustomPropertyTypes(projectMeta, itemType));
        return properties;

    }

    public getSubArtifactPropertyTypes(projectId: number, itemTypeId: number): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];

        let projectMeta = this.get(projectId);
        let itemType = this.getSubArtifactItemType(projectId, itemTypeId);        
        if (itemType) {

            properties.push(...this.getSubArtifactSystemPropertyTypes(itemType));
            //add custom property types
            properties.push(...this.getCustomPropertyTypes(projectMeta, itemType));
        }

        return properties;

    }


    private getArtifactSystemPropertyTypes(projectMeta: ProjectMetaData, itemType: Models.IItemType): Models.IPropertyType[] {
        // artifactType: Models.IItemType,
        // projectMeta: Models.IProjectMeta): Models.IPropertyType[] {
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
            validValues: function (data: Models.IProjectMeta) {
                if (!data) {
                    return [];
                }
                return data.artifactTypes.filter((it: Models.IItemType) => {
                    return (itemType && (itemType.predefinedType === it.predefinedType));
                });
            } (projectMeta ? projectMeta.data : null).map(function (it) {
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

        switch (itemType.predefinedType) {
            case Models.ItemTypePredefined.Actor:
                properties.push({
                    name: this.localization.get("Label_ActorImage", "Image"), //TODO localize
                    propertyTypePredefined: Models.PropertyTypePredefined.Image,
                    primitiveType: Models.PrimitiveType.Image,
                }, {
                    name: this.localization.get("Label_ActorInheritFrom", "Inherit from"), //TODO localize
                    propertyTypePredefined: Models.PropertyTypePredefined.ActorInheritance,
                    primitiveType: Models.PrimitiveType.ActorInheritance,
                });
                break;
            case Models.ItemTypePredefined.Document:
                properties.push({
                    name: this.localization.get("Label_DocumentFile", "DocumentFile"), //TODO localize
                    propertyTypePredefined: Models.PropertyTypePredefined.DocumentFile,
                    primitiveType: Models.PrimitiveType.DocumentFile,
                });
                break;
            default:
                break;
        }

        return properties;
    }

    private getSubArtifactSystemPropertyTypes(itemType: Models.IItemType): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];

        if (!itemType) {
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

        if (itemType.predefinedType === Models.ItemTypePredefined.Step) {
           properties.push(<Models.IPropertyType>{
               name: "Step Of",
               propertyTypePredefined: Models.PropertyTypePredefined.StepOf,
               primitiveType: Models.PrimitiveType.Choice,               
           });
        }

        if (itemType.predefinedType === Models.ItemTypePredefined.GDShape ||
            itemType.predefinedType === Models.ItemTypePredefined.DDShape ||
            itemType.predefinedType === Models.ItemTypePredefined.SBShape ||
            itemType.predefinedType === Models.ItemTypePredefined.UIShape ||
            itemType.predefinedType === Models.ItemTypePredefined.UCDShape ||
            itemType.predefinedType === Models.ItemTypePredefined.PROShape ||
            itemType.predefinedType === Models.ItemTypePredefined.BPShape ||
            itemType.predefinedType === Models.ItemTypePredefined.GDConnector ||
            itemType.predefinedType === Models.ItemTypePredefined.DDConnector ||
            itemType.predefinedType === Models.ItemTypePredefined.SBConnector ||
            itemType.predefinedType === Models.ItemTypePredefined.UIConnector ||
            itemType.predefinedType === Models.ItemTypePredefined.BPConnector ||
            itemType.predefinedType === Models.ItemTypePredefined.UCDConnector) {

            properties.push(<Models.IPropertyType>{
                name: "Label",
                propertyTypePredefined: Models.PropertyTypePredefined.Label,
                primitiveType: Models.PrimitiveType.Text,
                isRichText: true
            });
        }

        if (itemType.predefinedType === Models.ItemTypePredefined.GDShape ||
            itemType.predefinedType === Models.ItemTypePredefined.DDShape ||
            itemType.predefinedType === Models.ItemTypePredefined.SBShape ||
            itemType.predefinedType === Models.ItemTypePredefined.UIShape ||
            itemType.predefinedType === Models.ItemTypePredefined.UCDShape ||
            itemType.predefinedType === Models.ItemTypePredefined.PROShape ||
            itemType.predefinedType === Models.ItemTypePredefined.BPShape) {

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

    private getCustomPropertyTypes(projectMeta: ProjectMetaData, itemType: Models.IItemType): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];
        if (projectMeta && projectMeta.data) {
            projectMeta.data.propertyTypes.forEach((it: Models.IPropertyType) => {
                if (itemType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                    properties.push(it);
                }
            });
        }
        return properties;
    }





}
