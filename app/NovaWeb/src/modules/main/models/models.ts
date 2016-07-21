import {ItemTypePredefined, PropertyTypePredefined, PrimitiveType, TraceType, TraceDirection } from "./enums";
export {ItemTypePredefined, PropertyTypePredefined, PrimitiveType, TraceType, TraceDirection };

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

export interface ITrace {
    traceType?: TraceType;
    traceId: number;
    Direction: TraceDirection;
    isDeleted?: boolean;
    isSuspect?: boolean;
}

export interface IItem {
    id: number;
    name?: string;
    parentId?: number;
    itemTypeId?: number;
    itemTypeVersionId?: number;
    version?: number;
    propertyValues?: IPropertyValue[];
    traces?: ITrace[];
}

export interface ISubArtifact extends IItem {
    isDeleted?: boolean;
}

export interface IArtifact extends IItem {
    projectId?: number;
    prefix?: string;
    orderIndex?: number;
    version?: number;
    permissions?: number;
    lockedByUserId?: number;
    hasChildren?: boolean;
    subArtifacts?: ISubArtifact[];

    //for client use
    artifacts?: IArtifact[];
    predefinedType?: ItemTypePredefined;
    loaded?: boolean;

}
export interface IOption {
    id: number;
    value: string;
}
export interface IItemType {
    id: number;
    name: string;
    projectId?: number;
    versionId?: number;
    instanceItemTypeId?: number;
    prefix: string;
    predefinedType: number;
    iconImageId: number;
    usedInThisProject: boolean;
    customPropertyTypeIds: number[];
}
export interface IPropertyType {
    id?: number;
    versionId?: number;
    name?: string;
    primitiveType?: PrimitiveType;
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
    validValues?: IOption[];
    defaultValidValueId?: number;
    
    // Extra properties. Maintaned by client
    propertyTypePredefined?: PropertyTypePredefined;
    disabled?: boolean;
}
export interface IPropertyValue {
    propertyTypeId: number;
    propertyTypeVersionId?: number;
    propertyTypePredefined?: PropertyTypePredefined;
    value: any;
}

export interface IProjectMeta {
    artifactTypes: IItemType[];
    propertyTypes: IPropertyType[];
    subArtifactTypes: IItemType[];
    //flags:
}

export interface IProject extends IArtifact {
    description?: string;
    meta?: IProjectMeta;
}


export class Project implements IProject {
    constructor(...data: any[]) { //
        angular.extend(this, ...data);
        this.itemTypeId = <number>ItemTypePredefined.Project;
    };

    public id: number;

    public name: string;

    public description: string;

    public itemTypeId: number;

    public itemTypeVersionId: number;

    public meta: IProjectMeta;

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

    public getArtifactTypes(id?: number): IItemType[] {

        let itemTypes: IItemType[] = [];

        if (this.meta && this.meta.artifactTypes) {
            itemTypes = this.meta.artifactTypes.filter((it) => {
                return !angular.isNumber(id) || it.id === id;
            });
        }

        return itemTypes;
    }


    public getPropertyTypes(id?: number): IPropertyType[] {

        let propertyTypes: IPropertyType[] = [];

        if (this.meta && this.meta.propertyTypes) {
            propertyTypes = this.meta.propertyTypes.filter((it) => {
                return !angular.isNumber(id) || it.id === id;
            });
        }
        return propertyTypes;
    }
}

export class Artifact implements IArtifact {
    public propertyValues: IPropertyValue[];
    public artifacts: IArtifact[];
    public subArtifacts: ISubArtifact[];

    constructor(...data: any[]) { //
        angular.extend(this, ...data);
    };
    public id: number;
    public name: string;
    public projectId: number;
    public parentId: number;
    public predefinedType: ItemTypePredefined;
    public itemTypeId: number;
    public itemTypeVersionId: number;

}

  
