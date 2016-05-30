
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

export interface IProject extends IArtifact {
    getArtifact(artifactId: number, artifacts?: IArtifact[]): IArtifact;
}


export class Project implements IProject {
    public id: number;
    public name: string;
    public get projectId() {
        return this.id;
    }
    public typeId: number;
    public get parentId() {
        return -1;
    }

    public get predefinedType(): ArtifactTypeEnum {
        return ArtifactTypeEnum.Project;
    }

    public get hasChildren() {
        return this.artifacts.length > 0;
    }

    public artifacts: IArtifact[];


    constructor(id: number, name: string, data?: IArtifact[]) {
        this.id = id;
        this.name = name;
        if (angular.isArray(data)) {
            this.artifacts = data;
        }
    };

    public getArtifact(artifactId: number, artifacts?: IArtifact[]): IArtifact {
        let artifact: IArtifact;
        if (angular.isArray(artifacts)) {
            for (let i = 0, it: IArtifact; !artifact && (it = artifacts[i++]); ) {
                if (it.id === artifactId) {
                    artifact = it;
                } else if (it.artifacts) {
                    artifact = this.getArtifact(artifactId, it.artifacts);
                }
            }
        } else {
            artifact = this.getArtifact(artifactId, this.artifacts);
        }
        return artifact;
    };

}


class BaseItem {

    public setProperty(name: string, value: any) {
        if (this[name]) {
            this[name] = value;
        }
    }
}


