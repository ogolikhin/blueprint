
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

export interface IArtifact  {
    id: number;
    name: string;
    projectId: number;
    typeId: ArtifactTypeEnum;
    parentId: number;
    predefinedType: number;
    version?: number;
    hasChildren?: boolean;  
    artifacts?: IArtifact[];
    //flags:
}

export interface IProject {
    id: number;
    name: string;
    artifacts: IArtifact[];
    getArtifact(artifactId: number, artifacts?: IArtifact[]): IArtifact;
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

export class Project implements IProject {
    public id: number;
    public name: string;
    public artifacts: IArtifact[];

    constructor(data?: IArtifact[]) {
        this.artifacts = data;
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


