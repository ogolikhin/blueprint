import {ItemTypePredefined, PropertyTypePredefined} from "../../../main/models/enums";
import {IHashMap} from "../../../main/models/models";
import {PropertyType, PropertyValueFormat, ArtifactUpdateType} from "./enums";

export {
    ItemTypePredefined, 
    PropertyTypePredefined
};

export interface IArtifactInfo {
    id: number;
    typePrefix: string;
    name: string;
    typeId: number;
    parentId: number;
    orderIndex: number;
    shortDescription: string;
    notInCollection: boolean;
    isDiagram: boolean;
    predefined: ItemTypePredefined;
    hasComments: boolean;
}

export interface IArtifactProperty {
    name: string;
    value: any;
    format: PropertyValueFormat;
    propertyTypeId: number;
}

export interface IArtifactWithProperties {
    artifactId: number;
    properties: IArtifactProperty[];
    authorHistory: IArtifactProperty[];
    description: IArtifactProperty;
}

export interface IVersionInfo {
    artifactId: number;
    utcLockedDateTime: Date;
    lockOwnerLogin: string;
    projectId: number;
    versionId: number;
    revisionId: number;
    baselineId: number;
    isVersionInformationProvided: boolean;
    isHeadOrSavedDraftVersion: boolean;
}

export interface IHashMapOfPropertyValues {
    [name: string]: IPropertyValueInformation;
}

export interface IProcess {
    id: number;
    name: string;
    typePrefix: string;
    projectId: number;
    itemTypeId: number;
    baseItemTypePredefined: ItemTypePredefined;
    shapes: IProcessShape[];
    links: IProcessLink[];
    decisionBranchDestinationLinks: IProcessLink[];
    propertyValues: IHashMapOfPropertyValues;
    requestedVersionInfo: IVersionInfo;
}

export interface IProcessShape {
    id: number;
    name: string;
    projectId: number;
    typePrefix: string;
    parentId: number;
    baseItemTypePredefined: ItemTypePredefined;
    propertyValues: IHashMapOfPropertyValues;
    associatedArtifact: IArtifactReference;
}

export interface IPropertyValueInformation {
    propertyName: string;
    typePredefined: PropertyTypePredefined;
    typeId: number;
    value: any;
}

export interface IArtifactReference {
    id: number;
    projectId: number;
    name: string;
    typePrefix: string;
    projectName: string;
    baseItemTypePredefined: ItemTypePredefined;
    version: number;
    link: string;
}

export class ArtifactReference implements IArtifactReference {
    id: number;
    projectId: number;
    name: string;
    typePrefix: string;
    projectName: string;
    baseItemTypePredefined: ItemTypePredefined;
    version: number;
    link: string;
}

export interface IProcessLink {
    sourceId: number;
    destinationId: number;
    orderindex: number;
    label: string;
}

export interface IProcessLinkModel extends IProcessLink {
    parentId: number;
    sourceNode: any;
    destinationNode: any;
}

export interface IUserTaskShape extends ITaskShape {
    flags: ITaskFlags;
}

export interface ITaskShape extends IProcessShape {
}

export interface ITaskFlags {
    hasComments: boolean;
    hasTraces: boolean;
}

export interface ISystemTaskShape extends ITaskShape {
    flags: ITaskFlags;
}

export interface IArtifactReferenceLink {
    sourceId: number;
    destinationId: number;
    orderindex: number;
    associatedReferenceArtifactId: number;
}

export interface IUserStory extends IArtifact {
    processTaskId: number;
    isNew: boolean;
}

export interface IArtifact {
    id: number;
    name: string;
    projectId: number;
    typeId: number;
    typePrefix: string;
    typePredefined: ItemTypePredefined;
    systemProperties: IProperty[];
    customProperties: IProperty[];
}

export interface IProperty {
    name: string;
    value: any;
    propertyTypeId: number;
    propertyType: PropertyType;
}

export interface IProcessFlow {
    parentFlow: IProcessFlow;
    orderIndex: number;
    startShapeId: number;
    endShapeId: number;
    shapes: IHashMap<IProcessShape>;
}

export class ProcessFlowModel implements IProcessFlow {
    constructor(public parentFlow: IProcessFlow = null,
                public orderIndex: number = 0,
                public startShapeId: number = null,
                public endShapeId: number = null,
                public shapes: IHashMap<IProcessShape> = {}) {
    }
}

export class TreeShapeRef {
    public index: number;
    public flow: IProcessFlow;
    public prevShapeIds: number[] = [];
    public nextShapeIds: number[] = [];
}

export class ProcessLinkModel implements IProcessLinkModel {
    constructor(public parentId: number = 0,
                public sourceId: number = 0,
                public destinationId: number = 0,
                public orderindex: number = 0,
                public label: string = "",
                public sourceNode: any = null,
                public destinationNode: any = null) {
    }
}
export class ProcessShapeModel implements IProcessShape {
    constructor(public id: number = 0,
                public name: string = "",
                public projectId: number = 0,
                public typePrefix: string = "",
                public parentId: number = 0,
                public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
                public associatedArtifact: IArtifactReference = null,
                public propertyValues: IHashMapOfPropertyValues = {},
                public branchDestinationId: number = undefined,
                public flags: ITaskFlags = <ITaskFlags>{},
                public decisionSourceIds: number[] = []) {
    }
}
export class ProcessModel implements IProcess {
    constructor(public id: number = 0,
                public name: string = "",
                public typePrefix: string = "",
                public projectId: number = 0,
                public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.Process,
                public shapes: IProcessShape[] = [],
                public links: IProcessLinkModel[] = [],
                public propertyValues: IHashMapOfPropertyValues = {},
                public decisionBranchDestinationLinks: IProcessLink[] = [],
                public itemTypeId: number = 0,
                public requestedVersionInfo: IVersionInfo = null) {
    }
}

export class TaskShapeModel extends ProcessShapeModel implements ITaskShape {
    constructor(public id: number = 0,
                public name: string = "",
                public projectId: number = 0,
                public typePrefix: string = "",
                public parentId: number = 0,
                public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
                public associatedArtifact: IArtifactReference = null,
                public propertyValues: IHashMapOfPropertyValues = {}) {
        super(id, name, projectId, typePrefix, parentId, baseItemTypePredefined, associatedArtifact, propertyValues);
    }
}

export class UserTaskShapeModel extends TaskShapeModel implements IUserTaskShape {
    constructor(public id: number = 0,
                public name: string = "",
                public projectId: number = 0,
                public typePrefix: string = "",
                public parentId: number = 0,
                public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
                public associatedArtifact: IArtifactReference = null,
                public propertyValues: IHashMapOfPropertyValues = {}) {
        super(id, name, projectId, typePrefix, parentId, baseItemTypePredefined, associatedArtifact, propertyValues);
    }
}

export class SystemTaskShapeModel extends TaskShapeModel implements ISystemTaskShape {
    constructor(public id: number = 0,
                public name: string = "",
                public projectId: number = 0,
                public typePrefix: string = "",
                public parentId: number = 0,
                public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
                public associatedArtifact: IArtifactReference = null,
                public propertyValues: IHashMapOfPropertyValues = {}) {
        super(id, name, projectId, typePrefix, parentId, baseItemTypePredefined, associatedArtifact, propertyValues);
    }
}

export class NewUserTaskInfo {
    public userTaskId: number;
    public systemTaskId: number;
}

export class SourcesAndDestinations {
    public sourceIds: number[];
    public destinationIds: number[];
}

export class EdgeGeo {
    edge: MxCell;
    state: MxCellState;
}
