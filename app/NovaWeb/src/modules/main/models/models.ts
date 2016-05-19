
export enum ArtifactTypeEnum {
    Project = 0,

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
    children?: IArtifact[];
    //flags:
}

export interface IProject {
    id: number;
    name: string;
    children: IArtifact[];
    getArtifact(id: number, node?: IArtifact[]): IArtifact;
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
    public children: IArtifact[];

    constructor(data?: IArtifact[]) {
        this.children = data;
    };

    public getArtifact(id: number, nodes?: IArtifact[]): IArtifact {
        let item: IArtifact;
        if (!nodes) {
            return this.getArtifact(id, this.children);
        } else {
            nodes.map(function (node) {
                if (node.id === id) {  ///needs to be changed camelCase 
                    item = node;
                } else if (node.artifacts) {
                    item = this.getArtifact(id, node.artifacts);
                }
            }.bind(this));
        }
        return item;
    };

}


