import {ItemTypePredefined, PropertyTypePredefined, PrimitiveType } from "./enums"
export {ItemTypePredefined, PropertyTypePredefined, PrimitiveType }

export enum ArtifactTypeEnum {
    Project = -1,

    Unknown = 0,
    // Artifacts
    Folder = 1,
    Actor = 2,
    Document = 3,
    DomainDiagram = 4,
    GenericDiagram = 5,
    Glossary = 6,
    Process = 7,
    Storyboard = 8,
    Requirement = 9,
    UiMockup = 10,
    UseCase = 11,
    UseCaseDiagram = 12,

    //BaseLines and Reviews
    BaselineReviewFolder = 13,
    Baleline = 14,
    Review = 15,

    //Collections
    CollectionFolder = 16,
    Collection = 17
}


export enum ArtifactStateEnum {
    Published = 0,
    Draft = 1,
    Deleted = 2
}

export interface IProjectNode {
    id: number;
    type: number;
    name: string;
    parentFolderId: number;
    description?: string;
    hasChildren: boolean;
    children?: IProjectNode[];
}

export interface IArtifact {
    id: number;
    name: string;
    projectId: number;
    parentId: number;
    typeId: number;
    prefix?: string;
    predefinedType: ItemTypePredefined;
    orderIndex?: number;
    version?: number;
    permissions?: number;
    hasChildren?: boolean;
    propertyValues?: IPropertyValue[];
    subArtifacts?: IArtifact[];

    //for client use
    artifacts?: IArtifact[];
}
export interface IItemType {
    id: number;
    name: string;
    projectId?: number;
    versionId?: number;
    instanceItemTypeId?: number;
    prefix: string;
    baseType: number;
    iconImageId: number;
    usedInThisProject: boolean;
    customPropertyTypeIds: number[];
}
export interface IPropertyType {
    id?: number;
    versionId?: number;
    name?: string;
    primitiveType: PrimitiveType;
    instancePropertyTypeId?: number;
    isRichText?: boolean;
    decimalDefaultValue?: number;
    dateDefaultValue?: Date;
    userGroupDefaultValue?: any[];
    stringDefaultValue?: string;
    decimalPlaces?: number;
    maxNumber?: number;
    minNumber?: number;
    maxDate?: Date;
    minDate?: Date;
    isMultipleAllowed?: boolean;
    isRequired?: boolean;
    isValidated?: boolean;
    validValues?: string[];
    defaultValidValueIndex?: number;
    
    // Extra properties. Maintaned by client
    disabled?: boolean;
}
export interface IPropertyValue {
    propertyTypeId: number;
    propertyTypeVersionId?: number;
    propertyTypePredefined?: PropertyTypePredefined, 
    value: any;
}


export interface IArtifactDetails extends IArtifact {
//    links: ILink[];
    //flags:
}
export interface IProjectMeta {
    artifactTypes: IItemType[];
    propertyTypes: IPropertyType[];
    subArtifactTypes: IItemType[];
    //flags:
}

export interface IProject extends IArtifact {
    description: string;
    meta?: IProjectMeta;
}

export class Artifact {
    private _propertyValues: IPropertyValue[];
    private _subArtifacts: IArtifact[];

    constructor(...data: any[]) { //
        angular.extend(this, ...data);
    };
    public id: number;

    public name: string;

    public projectId: number;
    public parentId: number;
    public predefinedType: ItemTypePredefined;
    public typeId: number;

    public get propertyValues() {
        return this._propertyValues || (this._propertyValues = []);
    }
    public get subArtifacts() {
        return this._subArtifacts || (this._subArtifacts = []);
    }
    public artifacts: IArtifact[];
}

export class Project implements IProject {
    constructor(...data: any[]) { //
        angular.extend(this, ...data);
    };

    public id: number;

    public name: string;

    public description: string;

    public typeId: number;

    public artifacts: IArtifact[];

    public get projectId() {
        return this.id;
    }

    public get parentId() {
        return -1;
    }

    public get predefinedType(): ItemTypePredefined {
        return ItemTypePredefined.Project;
    }

    public get hasChildren() {
        return this.artifacts && this.artifacts.length > 0;
    }

}

export interface IArtifactDetailFields {
    systemFields: AngularFormly.IFieldConfigurationObject[];
    customFields: AngularFormly.IFieldConfigurationObject[];
    noteFields: AngularFormly.IFieldConfigurationObject[];
}




