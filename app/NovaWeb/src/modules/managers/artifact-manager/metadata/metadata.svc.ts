import {BPLocale, ILocalizationService} from "../../../commonModule/localization/localization.service";
import {PrimitiveType, PropertyTypePredefined} from "../../../main/models/enums";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {IItemType, IOption, IProjectMeta, IPropertyType} from "../../../main/models/models";

export interface IMetaDataService {
    get(projectId: number): ng.IPromise<ProjectMetaData>;
    remove(projectId: number);
    refresh(projectId: number): ng.IPromise<ProjectMetaData>;
    getArtifactItemType(projectId: number, itemTypeId: number): ng.IPromise<IItemType>;
    getArtifactItemTypeTemp(projectId: number, itemTypeId: number): IItemType;
    getSubArtifactItemType(projectId: number, itemTypeId: number): ng.IPromise<IItemType>;
    getArtifactPropertyTypes(projectId: number, itemTypeId: number): ng.IPromise<IPropertyType[]>;
    getSubArtifactPropertyTypes(projectId: number, filter: (itemType: IItemType) => boolean): ng.IPromise<IPropertyType[]>;
}


export class ProjectMetaData {
    constructor(public id: number, public data: IProjectMeta, locale: BPLocale) {
        if (data) {
            _.each(data.propertyTypes, (propertyType: IPropertyType) => {
                if (propertyType.maxDate) {
                    propertyType.maxDate = locale.toDate(propertyType.maxDate, true);
                }
                if (propertyType.minDate) {
                    propertyType.minDate = locale.toDate(propertyType.minDate, true);
                }
                if (propertyType.dateDefaultValue) {
                    propertyType.dateDefaultValue = locale.toDate(propertyType.dateDefaultValue, true);
                }
            });
        }
    }
}

interface IProjectsMeta {
    [key: string]: ProjectMetaData;
}

export class MetaDataService implements IMetaDataService {

    private projectsMeta: IProjectsMeta = {};

    private promises: { [id: string]: ng.IPromise<any> } = {};

    public static $inject = [
        "$q",
        "$http",
        "localization"
    ];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private localization: ILocalizationService) {
    }

    public get(projectId: number): ng.IPromise<ProjectMetaData> {
        let promise: ng.IPromise<ProjectMetaData> = this.promises[String(projectId)];
        if (!promise) {
            const deferred: ng.IDeferred<ProjectMetaData> = this.$q.defer<ProjectMetaData>();
            const loadedMetaData = this.projectsMeta[String(projectId)];
            if (loadedMetaData) {
                deferred.resolve(loadedMetaData);
            } else {
                this.load(projectId, deferred);
            }
            return deferred.promise;
        }
        return promise;
    }

    private load(projectId: number, defer: ng.IDeferred<ProjectMetaData>) {
        this.promises[String(projectId)] = defer.promise;
        const url = `svc/artifactstore/projects/${projectId}/meta/customtypes`;
        this.$http.get<IProjectMeta>(url).then(
            (result: ng.IHttpPromiseCallbackArg<IProjectMeta>) => {
                if (angular.isArray(result.data.artifactTypes)) {
                    //add specific types
                    result.data.artifactTypes.unshift(
                        <IItemType>{
                            id: ItemTypePredefined.Project,
                            name: this.localization.get("Label_Project"),
                            predefinedType: ItemTypePredefined.Project,
                            customPropertyTypeIds: []
                        },
                        <IItemType>{
                            id: ItemTypePredefined.Collections,
                            name: this.localization.get("Label_Collections"),
                            predefinedType: ItemTypePredefined.CollectionFolder,
                            customPropertyTypeIds: []
                        },
                        <IItemType>{
                            id: ItemTypePredefined.BaselinesAndReviews,
                            name: this.localization.get("Label_BaselinesAndReviews"),
                            predefinedType: ItemTypePredefined.BaselineFolder,
                            customPropertyTypeIds: []
                        }
                    );
                }
                const metadata = new ProjectMetaData(projectId, result.data, this.localization.current);
                this.projectsMeta[String(projectId)] = metadata;
                defer.resolve(metadata);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        ).finally(() => {
            delete this.promises[String(projectId)];
        });
    }

    public remove(projectId: number) {
        if (_.has(this.projectsMeta, projectId)) {
            delete this.projectsMeta[projectId];
        }
    }

    public refresh(projectId: number) {
        const deferred: ng.IDeferred<ProjectMetaData> = this.$q.defer<ProjectMetaData>();
        this.load(projectId, deferred);
        return deferred.promise;
    }

    public getArtifactItemType(projectId: number, itemTypeId: number): ng.IPromise<IItemType> {
        return this.get(projectId).then((metaData) => {
            return _.find(metaData.data.artifactTypes, (it: IItemType) => {
                return it.id === itemTypeId;
            });
        });
    }

    // TODO Temporary method to get artifact type
    public getArtifactItemTypeTemp(projectId: number, itemTypeId: number): IItemType {
        const loadedMeta = this.projectsMeta[String(projectId)];
        if (loadedMeta) {
            return _.find(loadedMeta.data.artifactTypes, (it: IItemType) => {
                return it.id === itemTypeId;
            });
        }
        return undefined;
    }

    public getSubArtifactItemType(projectId: number, itemTypeId: number): ng.IPromise<IItemType> {
        return this.get(projectId).then((metaData) => {
            return _.find(metaData.data.subArtifactTypes, (it: IItemType) => {
                return it.id === itemTypeId;
            });
        });
    }

    public getArtifactPropertyTypes(projectId: number, itemTypeId: number): ng.IPromise<IPropertyType[]> {

        return this.get(projectId).then((projectMeta) => {
            const itemType = _.find(projectMeta.data.artifactTypes, (it: IItemType) => {
                return it.id === itemTypeId;
            });
            const properties: IPropertyType[] = [];
            if (itemType) {
                properties.push(...this.getArtifactSystemPropertyTypes(projectMeta, itemType));
                //add custom property types
                properties.push(...this.getCustomPropertyTypes(projectMeta, itemType));
            }
            return properties;
        });

    }

    public getSubArtifactPropertyTypes(projectId: number, filter: (itemType: IItemType) => boolean): ng.IPromise<IPropertyType[]> {

        return this.get(projectId).then((projectMeta) => {
            const itemType = _.find(projectMeta.data.subArtifactTypes, filter);
            const properties: IPropertyType[] = [];
            if (itemType) {
                properties.push(...this.getSubArtifactSystemPropertyTypes(itemType));
                //add custom property types
                properties.push(...this.getCustomPropertyTypes(projectMeta, itemType));
            }

            return properties;
        });

    }

    private getArtifactSystemPropertyTypes(projectMeta: ProjectMetaData, itemType: IItemType): IPropertyType[] {
        const properties: IPropertyType[] = [];

        //add system properties
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Name"),
            propertyTypePredefined: PropertyTypePredefined.Name,
            primitiveType: PrimitiveType.Text,
            isRequired: true
        });
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Type"),
            propertyTypePredefined: PropertyTypePredefined.ItemTypeId,
            primitiveType: PrimitiveType.Choice,
            validValues: function (data: IProjectMeta) {
                if (!data) {
                    return [];
                }
                return _.filter(data.artifactTypes, (it: IItemType) => {
                    return (itemType && (itemType.predefinedType === it.predefinedType));
                });
            }(projectMeta ? projectMeta.data : null).map(function (it) {
                return <IOption>{
                    id: it.id,
                    value: it.name
                };
            }),
            disabled: true, // makes the artifact type read-only
            isRequired: true
        });
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_CreatedBy"),
            propertyTypePredefined: PropertyTypePredefined.CreatedBy,
            primitiveType: PrimitiveType.User,
            disabled: true
        });
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_CreatedOn"),
            propertyTypePredefined: PropertyTypePredefined.CreatedOn,
            primitiveType: PrimitiveType.Date,
            stringDefaultValue: "Never published",
            disabled: true
        });
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_LastEditBy"),
            propertyTypePredefined: PropertyTypePredefined.LastEditedBy,
            primitiveType: PrimitiveType.User,
            disabled: true
        });
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_LastEditOn"),
            propertyTypePredefined: PropertyTypePredefined.LastEditedOn,
            primitiveType: PrimitiveType.Date,
            disabled: true
        });
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Description"),
            propertyTypePredefined: PropertyTypePredefined.Description,
            primitiveType: PrimitiveType.Text,
            isRichText: true,
            isMultipleAllowed: true
        });

        switch (itemType.predefinedType) {
            case ItemTypePredefined.Actor:
                properties.push({
                    name: this.localization.get("Label_ActorImage", "Image"), //TODO localize
                    propertyTypePredefined: PropertyTypePredefined.Image,
                    primitiveType: PrimitiveType.Image
                }, {
                    name: this.localization.get("Label_ActorInheritFrom", "Inherit from"), //TODO localize
                    propertyTypePredefined: PropertyTypePredefined.ActorInheritance,
                    primitiveType: PrimitiveType.ActorInheritance
                });
                break;
            case ItemTypePredefined.Document:
                properties.push({
                    name: this.localization.get("Label_DocumentFile", "DocumentFile"), //TODO localize
                    propertyTypePredefined: PropertyTypePredefined.DocumentFile,
                    primitiveType: PrimitiveType.DocumentFile
                });
                break;
            default:
                break;
        }

        return properties;
    }

    private getSubArtifactSystemPropertyTypes(itemType: IItemType): IPropertyType[] {
        let properties: IPropertyType[] = [];

        if (!itemType) {
            return properties;
        }

        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Name"),
            propertyTypePredefined: PropertyTypePredefined.Name,
            primitiveType: PrimitiveType.Text,
            isRequired: true
        });

        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Description"),
            propertyTypePredefined: PropertyTypePredefined.Description,
            primitiveType: PrimitiveType.Text,
            isRichText: true,
            isMultipleAllowed: true
        });

        if (itemType.predefinedType === ItemTypePredefined.Step) {
            properties.push(<IPropertyType>{
                name: "Step Of",
                propertyTypePredefined: PropertyTypePredefined.StepOf,
                primitiveType: PrimitiveType.Choice
            });
        }

        if (itemType.predefinedType === ItemTypePredefined.GDShape ||
            itemType.predefinedType === ItemTypePredefined.DDShape ||
            itemType.predefinedType === ItemTypePredefined.SBShape ||
            itemType.predefinedType === ItemTypePredefined.UIShape ||
            itemType.predefinedType === ItemTypePredefined.UCDShape ||
            itemType.predefinedType === ItemTypePredefined.BPShape ||
            itemType.predefinedType === ItemTypePredefined.GDConnector ||
            itemType.predefinedType === ItemTypePredefined.DDConnector ||
            itemType.predefinedType === ItemTypePredefined.SBConnector ||
            itemType.predefinedType === ItemTypePredefined.UIConnector ||
            itemType.predefinedType === ItemTypePredefined.BPConnector ||
            itemType.predefinedType === ItemTypePredefined.UCDConnector ||
            itemType.predefinedType === ItemTypePredefined.PROShape) {

            properties.push(<IPropertyType>{
                name: this.localization.get("Label_Label"),
                propertyTypePredefined: PropertyTypePredefined.Label,
                primitiveType: PrimitiveType.Text,
                isRichText: true
            });
        }

        if (itemType.predefinedType === ItemTypePredefined.GDShape ||
            itemType.predefinedType === ItemTypePredefined.DDShape ||
            itemType.predefinedType === ItemTypePredefined.SBShape ||
            itemType.predefinedType === ItemTypePredefined.UIShape ||
            itemType.predefinedType === ItemTypePredefined.UCDShape ||
            itemType.predefinedType === ItemTypePredefined.BPShape ||
            itemType.predefinedType === ItemTypePredefined.PROShape) {

            properties.push(<IPropertyType>{
                name: this.localization.get("Label_X"),
                propertyTypePredefined: PropertyTypePredefined.X,
                primitiveType: PrimitiveType.Number
            });

            properties.push(<IPropertyType>{
                name: this.localization.get("Label_Y"),
                propertyTypePredefined: PropertyTypePredefined.Y,
                primitiveType: PrimitiveType.Number
            });

            properties.push(<IPropertyType>{
                name: this.localization.get("Label_Width"),
                propertyTypePredefined: PropertyTypePredefined.Width,
                primitiveType: PrimitiveType.Number
            });

            properties.push(<IPropertyType>{
                name: this.localization.get("Label_Height"),
                propertyTypePredefined: PropertyTypePredefined.Height,
                primitiveType: PrimitiveType.Number
            });
        }
        return properties;
    }

    private getCustomPropertyTypes(projectMeta: ProjectMetaData, itemType: IItemType): IPropertyType[] {
        let properties: IPropertyType[] = [];
        if (projectMeta && projectMeta.data) {
            _.each(itemType.customPropertyTypeIds, id => {
                let propertyType = _.find(projectMeta.data.propertyTypes, pt => pt.id === id);
                if (propertyType) {
                    propertyType.propertyTypePredefined = PropertyTypePredefined.CustomGroup;
                    properties.push(propertyType);
                }
            });
        }
        return properties;
    }

}
