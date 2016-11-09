import * as angular from "angular";
import {
    ItemTypePredefined,
    PropertyTypePredefined,
    PrimitiveType,
    RolePermissions,
    ReuseSettings,
    TraceType,
    TraceDirection,
    LockResultEnum
} from "./enums";
import {IArtifactAttachment, IArtifactDocRef} from "../../managers/artifact-manager";
import {IRelationship} from "./relationshipModels";

export enum ArtifactStateEnum {
    Published = 0,
    Draft = 1,
    Deleted = 2
}

export interface ISubArtifactNode {
    id: number;
    parentId: number;
    itemTypeId: number;
    displayName: string;
    predefinedType: ItemTypePredefined;
    prefix: string;
    hasChildren: boolean;
    children?: ISubArtifactNode[];
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
    systemPropertyValues?: IPropertyValue[];
    traces?: IRelationship[];

    predefinedType?: ItemTypePredefined;

    attachmentValues?: IArtifactAttachment[];
    docRefValues?: IArtifactDocRef[];
}

export interface IUserGroup {
    id?: number;
    displayName?: string;
    isGroup?: boolean;
}

export interface ISubArtifact extends IItem {
    isDeleted?: boolean;
}

export interface IPublishResultSet {
    artifacts?: IArtifact[];
    projects?: IItem[];
}

export interface IArtifact extends IItem {
    projectId?: number;
    orderIndex?: number;
    version?: number;

    createdOn?: Date;
    lastEditedOn?: Date;
    createdBy?: IUserGroup;
    lastEditedBy?: IUserGroup;
    lastSavedOn?: Date;

    lockedByUser?: IUserGroup;
    lockedDateTime?: Date;

    permissions?: RolePermissions;
    readOnlyReuseSettings?: ReuseSettings;

    hasChildren?: boolean;
    subArtifacts?: ISubArtifact[];

    itemTypeIconId?: number;
    itemTypeName?: string;

    //for client use
    children?: IArtifact[];
    loaded?: boolean;
    // for artifact picker use
    parent?: IArtifact;
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
    isReuseReadOnly?: boolean;
    isRichText?: boolean;
    isMultipleAllowed?: boolean;
    value: any;
}

export interface IActorInheritancePropertyValue {
    pathToProject: string[];
    actorName: string;
    actorPrefix: string;
    actorId: number;
    hasAccess: boolean;

    // client side use only
    isProjectPathVisible: boolean;
}

export interface IActorImagePropertyValue {
    url: string;
    guid: string;

    // for client use only
    imageSource: string;
}

export interface IProjectMeta {
    artifactTypes: IItemType[];
    propertyTypes: IPropertyType[];
    subArtifactTypes: IItemType[];
    //flags:
}

export interface IArtifactWithProject extends IArtifact {
    projectName?: string;
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
    public itemTypeName: string;
    public hasCustomIcon: boolean;
}

export interface IKeyValuePair {
    key: any;
    value: any;
}

export interface IHashMap<T> {
    [key: string]: T;
}


export interface IEditorContext {
    artifact?: IArtifact;
    type?: IItemType;
}


export interface ILockResult {
    result: LockResultEnum;
    info: IVersionInfo;
}

export interface IVersionInfo {
    artifactId?: number;
    utcLockedDateTime?: Date;
    lockOwnerId?: number;
    lockOwnerLogin?: string;
    lockOwnerDisplayName?: string;
    projectId?: number;
    versionId?: number;
    revisionId?: number;
    baselineId?: number;
    parentId?: number;
    orderIndex?: number;
    isVersionInformationProvided?: boolean;
    isHeadOrSavedDraftVersion?: boolean;
}


export {ItemTypePredefined, PropertyTypePredefined, PrimitiveType, TraceType, TraceDirection};
