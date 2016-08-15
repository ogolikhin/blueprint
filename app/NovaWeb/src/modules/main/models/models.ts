﻿import {ItemTypePredefined, PropertyTypePredefined, PrimitiveType, RolePermissions, TraceType, TraceDirection } from "./enums";


export enum ArtifactStateEnum {
    Published = 0,
    Draft = 1,
    Deleted = 2
}

export enum ProjectNodeType {
    Folder = 0,
    Project = 1
}

export interface IProjectNode {
    id: number;
    type: ProjectNodeType;
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
    description?: string;
    prefix?: string;
    parentId?: number;
    itemTypeId?: number;
    itemTypeVersionId?: number;
    version?: number;
    customPropertyValues?: IPropertyValue[];
    specificPropertyValues?: IPropertyValue[];
    traces?: ITrace[];

    predefinedType?: ItemTypePredefined;
}

export interface IUserGroup {
    id?: number;
    displayName?: string;
    isGroup?: boolean;
}

export interface ISubArtifact extends IItem {
    isDeleted?: boolean;
}

export interface IArtifact extends IItem {
    projectId?: number;
    orderIndex?: number;
    version?: number;

    createdOn?: Date; 
    lastEditedOn?: Date;
    createdBy?: IUserGroup;
    lastEditedBy?: IUserGroup;

    lockedByUser?: IUserGroup;
    lockedDateTime?: Date;

    permissions?: RolePermissions;

    hasChildren?: boolean;
    subArtifacts?: ISubArtifact[];

    //for client use
    artifacts?: IArtifact[];
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
    dateDefaultValue?: string;
    userGroupDefaultValue?: any[];
    stringDefaultValue?: string;
    decimalPlaces?: number;
    maxNumber?: number;
    minNumber?: number;
    maxDate?: string;
    minDate?: string;
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

    public get parentId(): number {
        return -1;
    }
    public lockedByUserId: number;

    public get permissions(): RolePermissions {
        return 4095;
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

export interface IHashMap<T> {
    [key: string]: T;
}


export interface IEditorContext {
    artifact?: IArtifact;
    type?: IItemType;
}
  
export {ItemTypePredefined, PropertyTypePredefined, PrimitiveType, TraceType, TraceDirection };
