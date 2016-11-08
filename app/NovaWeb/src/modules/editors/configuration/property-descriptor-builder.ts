import * as angular from "angular";
import { PropertyTypePredefined, PropertyLookupEnum } from "../../main/models/enums";
import { IPropertyType, IPropertyValue, IOption, PrimitiveType } from "../../main/models/models";
import { ItemTypePredefined } from "../../main/models/enums";
import { IStatefulArtifact, IStatefulSubArtifact } from "../../managers/artifact-manager";
import { ILocalizationService } from "../../core";

export interface IPropertyDescriptorBuilder {
    createArtifactPropertyDescriptors(artifact: IStatefulArtifact): ng.IPromise<IPropertyDescriptor[]>;
    createSubArtifactPropertyDescriptors(subArtifact: IStatefulSubArtifact): ng.IPromise<IPropertyDescriptor[]>;
}

export interface IPropertyDescriptor {
    id: number;
    versionId: number;
    name: string;
    primitiveType: PrimitiveType;
    instancePropertyTypeId: number;
    isRichText: boolean;
    decimalDefaultValue: number;
    userGroupDefaultValue: any[];
    stringDefaultValue: string;
    decimalPlaces: number;
    maxNumber: number;
    minNumber: number;
    dateDefaultValue: string;
    maxDate: string;
    minDate: string;
    isMultipleAllowed: boolean;
    isRequired: boolean;
    isValidated: boolean;
    validValues: IOption[];
    defaultValidValueId: number;
    propertyTypePredefined: PropertyTypePredefined;
    disabled: boolean;
    //extension
    fieldPropertyName: string;
    modelPropertyName: string | number;
    lookup: PropertyLookupEnum;
    isFresh?: boolean;
}

export class PropertyDescriptor implements IPropertyDescriptor {
    public id: number;
    public versionId: number;
    public name: string;
    public primitiveType: PrimitiveType;
    public instancePropertyTypeId: number;
    public isRichText: boolean;
    public decimalDefaultValue: number;
    public userGroupDefaultValue: any[];
    public stringDefaultValue: string;
    public decimalPlaces: number;
    public maxNumber: number;
    public minNumber: number;
    public dateDefaultValue: string;
    public maxDate: string;
    public minDate: string;
    public isMultipleAllowed: boolean;
    public isRequired: boolean;
    public isValidated: boolean;
    public validValues: IOption[];
    public defaultValidValueId: number;
    public propertyTypePredefined: PropertyTypePredefined;
    public disabled: boolean;
    //extension
    public fieldPropertyName: string;
    public modelPropertyName: string | number;
    public lookup: PropertyLookupEnum;
    public isFresh?: boolean;

    public static createFromPropertyType(type: IPropertyType) {
        const propertyContext = new PropertyDescriptor();
        angular.extend(propertyContext, type);
        propertyContext.init();
        return propertyContext;
    }

    public static createFromPropertyValue(propertyValue: IPropertyValue) {
        const propertyContext = new PropertyDescriptor();
        propertyContext.id = propertyValue.propertyTypeId;
        propertyContext.propertyTypePredefined = propertyValue.propertyTypePredefined;
        propertyContext.name = propertyValue["name"];
        propertyContext.primitiveType = propertyValue["primitiveType"];
        propertyContext.isMultipleAllowed = propertyValue.isMultipleAllowed;
        if (propertyContext.primitiveType === PrimitiveType.Text) {
            propertyContext.isRichText = propertyValue.isRichText;
        }
        else if (propertyContext.primitiveType === PrimitiveType.Choice && propertyValue.value) {
            propertyContext.isMultipleAllowed = propertyValue.value["isMultipleAllowed"];
            if (propertyValue.value["validValues"]) {
                propertyContext.validValues = propertyValue.value["validValues"];
            }
        }
        propertyContext.init();
        return propertyContext;
    }

    private init() {
        let propertyTypeName: string = _.camelCase(String(PropertyTypePredefined[this.propertyTypePredefined]));
        if (this.isSystem(this.propertyTypePredefined)) {
            this.lookup = PropertyLookupEnum.System;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = propertyTypeName;
        } else if (this.propertyTypePredefined === PropertyTypePredefined.CustomGroup) {
            this.lookup = PropertyLookupEnum.Custom;
            this.fieldPropertyName = `${PropertyLookupEnum[this.lookup]}_${this.id.toString()}`;
            this.modelPropertyName = this.id;
        } else {
            this.lookup = PropertyLookupEnum.Special;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = this.propertyTypePredefined;
        }
    }

    private isSystem(type: PropertyTypePredefined): boolean {
        return [PropertyTypePredefined.Name,
                PropertyTypePredefined.ItemTypeId,
                PropertyTypePredefined.CreatedBy,
                PropertyTypePredefined.CreatedOn,
                PropertyTypePredefined.LastEditedBy,
                PropertyTypePredefined.LastEditedOn,
                PropertyTypePredefined.Description].indexOf(type) >= 0;
    }
}

export class PropertyDescriptorBuilder implements IPropertyDescriptorBuilder {

    public static $inject = ["$q", "localization"];

    constructor(private $q: ng.IQService, private localization: ILocalizationService) {
    }
    
    public createArtifactPropertyDescriptors(artifact: IStatefulArtifact): ng.IPromise<IPropertyDescriptor[]> {
        if (artifact.artifactState.historical) {
            const defered = this.$q.defer<IPropertyDescriptor[]>();
            const propertyContexts = [];
            propertyContexts.push(...this.createArtifactSystemPropertyDescriptors(artifact));
            artifact.customProperties.list().forEach((propertyValue: IPropertyValue) => {
                propertyContexts.push(PropertyDescriptor.createFromPropertyValue(propertyValue));
            });
            defered.resolve(propertyContexts);
            return defered.promise;
        } else {
            return artifact.metadata.getArtifactPropertyTypes().then((propertyTypes) => {
                const propertyContexts = [];
                propertyTypes.forEach(propertyType => {
                    propertyContexts.push(PropertyDescriptor.createFromPropertyType(propertyType));
                });
                return propertyContexts;
            });
        }
    }

    public createSubArtifactPropertyDescriptors(subArtifact: IStatefulSubArtifact): ng.IPromise<IPropertyDescriptor[]> {
        if (subArtifact.artifactState.historical) {
            const defered = this.$q.defer<IPropertyDescriptor[]>();
            const propertyContexts = [];
            propertyContexts.push(...this.createSubArtifactSystemPropertyDescriptors(subArtifact));
            subArtifact.customProperties.list().forEach((propertyValue: IPropertyValue) => {
                propertyContexts.push(PropertyDescriptor.createFromPropertyValue(propertyValue));
            });
            defered.resolve(propertyContexts);
            return defered.promise;
        } else {
            return subArtifact.metadata.getSubArtifactPropertyTypes().then((propertyTypes) => {
                const propertyContexts = [];
                propertyTypes.forEach(propertyType => {
                    propertyContexts.push(PropertyDescriptor.createFromPropertyType(propertyType));
                });
                return propertyContexts;
            });
        }
    }

    private createArtifactSystemPropertyDescriptors(artifact: IStatefulArtifact) {
        const properties: IPropertyType[] = [];

        //add system properties
        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Name"),
            propertyTypePredefined: PropertyTypePredefined.Name,
            primitiveType: PrimitiveType.Text,
            isRequired: true
        });

        const artifactTypes = [{ id: artifact.itemTypeId, value: artifact.itemTypeName}];

        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Type"),
            propertyTypePredefined: PropertyTypePredefined.ItemTypeId,
            primitiveType: PrimitiveType.Choice,
            validValues: artifactTypes,
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
            dateDefaultValue: "",
            disabled: true
        });

        properties.push(<IPropertyType>{
            name: this.localization.get("Label_Description"),
            propertyTypePredefined: PropertyTypePredefined.Description,
            primitiveType: PrimitiveType.Text,
            isRichText: true
        });

        switch (artifact.predefinedType) {
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
        const propertyContexts = [];
        properties.forEach(propertyType => {
            propertyContexts.push(PropertyDescriptor.createFromPropertyType(propertyType));
        });

        return propertyContexts;
    }

    private createSubArtifactSystemPropertyDescriptors(subArtifact: IStatefulSubArtifact) {
        const properties: IPropertyType[] = [];

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
            isRichText: true
        });

        if (subArtifact.predefinedType === ItemTypePredefined.Step) {
            properties.push(<IPropertyType>{
                name: "Step Of",
                propertyTypePredefined: PropertyTypePredefined.StepOf,
                primitiveType: PrimitiveType.Choice
            });
        }

        if (subArtifact.predefinedType === ItemTypePredefined.GDShape ||
            subArtifact.predefinedType === ItemTypePredefined.DDShape ||
            subArtifact.predefinedType === ItemTypePredefined.SBShape ||
            subArtifact.predefinedType === ItemTypePredefined.UIShape ||
            subArtifact.predefinedType === ItemTypePredefined.UCDShape ||
            subArtifact.predefinedType === ItemTypePredefined.BPShape ||
            subArtifact.predefinedType === ItemTypePredefined.GDConnector ||
            subArtifact.predefinedType === ItemTypePredefined.DDConnector ||
            subArtifact.predefinedType === ItemTypePredefined.SBConnector ||
            subArtifact.predefinedType === ItemTypePredefined.UIConnector ||
            subArtifact.predefinedType === ItemTypePredefined.BPConnector ||
            subArtifact.predefinedType === ItemTypePredefined.UCDConnector ||
            subArtifact.predefinedType === ItemTypePredefined.PROShape) {

            properties.push(<IPropertyType>{
                name: this.localization.get("Label_Label"),
                propertyTypePredefined: PropertyTypePredefined.Label,
                primitiveType: PrimitiveType.Text,
                isRichText: true
            });
        }

        if (subArtifact.predefinedType === ItemTypePredefined.GDShape ||
            subArtifact.predefinedType === ItemTypePredefined.DDShape ||
            subArtifact.predefinedType === ItemTypePredefined.SBShape ||
            subArtifact.predefinedType === ItemTypePredefined.UIShape ||
            subArtifact.predefinedType === ItemTypePredefined.UCDShape ||
            subArtifact.predefinedType === ItemTypePredefined.BPShape ||
            subArtifact.predefinedType === ItemTypePredefined.PROShape) {

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
}
