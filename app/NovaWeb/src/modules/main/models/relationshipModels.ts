export interface Relationship {
    artifactId: number;
    artifactTypePrefix: string;
    artifactName: string;
    itemId: number;
    itemTypePrefix: string;
    itemName: string;
    projectId: number;
    projectName: string;
    rraceDirection: TraceDirection;
    traceType: LinkType;
    suspect: boolean;
    hasAccess: boolean;
}

export enum TraceDirection {
    To = 0,
    From = 1,
    TwoWay = 2
}

export enum LinkType {
    None = 0,
    ParentChild = 1,
    Manual = 2,
    Subartifact = 4,
    Association = 8, // other
    ActorInheritsFrom = 16, //other
    DocumentReference = 32, //other
    GlossaryReference = 64, //other
    ShapeConnector = 128,  //other
    BaselineReference = 256, //other
    ReviewPackageReference = 512, //other
    Reuse = 1024
}

export interface RelationshipExtendedInfo {

    artifactId: number;
    description: string;
    pathToProject: ItemIdItemNameParentId[];
}

export interface ItemIdItemNameParentId {
    itemId: number;
    parentId: number;
    itemName: string;
}

export enum ITraceType {
    Manual = 0,
    Other = 1
}