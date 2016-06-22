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


export interface IArtifact  {
    id: number;
    name: string;
    projectId: number;
    typeId: number;
    parentId: number;
    predefinedType: ArtifactTypeEnum;
    prefix?: string;
    version?: number;
    hasChildren?: boolean;  
    artifacts?: IArtifact[];
    //flags:
}
export interface IProperty {
    id: number;
    name: string;
}

export interface IArtifactDetails extends IArtifact {
    systemProperties: IProperty[];
    customProperties: IProperty[];
    //flags:
}
export interface IProjectMeta {
    artifactTypes: IProperty[];
    propertyTypes: IProperty[];
    subArtifactTypes: IProperty[];
    //flags:
}


export interface IProject extends IArtifact {
    description: string;
    meta?: IProjectMeta;
}


export class Artifact implements IArtifactDetails {
    private _systemProperties: IProperty[];
    private _customProperties: IProperty[];
    constructor(...data: any[]) { //
        angular.extend(this, ...data);
    };
    public id: number;

    public name: string;

    public projectId: number;
    public parentId: number;
    public predefinedType: ArtifactTypeEnum;
    public typeId: number;

    public get systemProperties() {
        return this._systemProperties || (this._systemProperties = []);
    }
    public get customProperties() {
        return this._customProperties || (this._customProperties = []);
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

    public get predefinedType(): ArtifactTypeEnum {
        return ArtifactTypeEnum.Project;
    }

    public get hasChildren() {
        return this.artifacts && this.artifacts.length > 0;
    }


    
}




