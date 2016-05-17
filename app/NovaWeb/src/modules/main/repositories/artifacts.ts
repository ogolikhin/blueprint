import "angular";

export interface IItem {
    id: number;
    name: string;
    typeId: number;
    parentId: number;
    predefinedType: number,
}

export interface IProjectItem extends IItem {
    projectId: number;
    version: number,
    artifacts: IProjectItem[]
    HasChildren: boolean;  ///needs to be changed camelCase 

    //flags:
}

export interface IProject {
    id: number;
    name: string;
    artifacts: IProjectItem[]
    getArtifact(id: number, node?: IProjectItem[]): IProjectItem;
}

export class Project implements IProject { 
    public id: number;
    public name: string;
    public artifacts: IProjectItem[]

    constructor(data?: IProjectItem[]) {
        this.artifacts = data;
    }

    public getArtifact(id: number, nodes?: IProjectItem[]): IProjectItem {
        let item: IProjectItem;
        if (!nodes) {
            return this.getArtifact(id, this.artifacts);
        } else {
            for (let i = 0, node: IProjectItem; !item && (node = nodes[i++]);) {
                if (node[`Id`] === id) {  ///needs to be changed camelCase 
                    item = node;
                } else if (node.artifacts) {
                    item = this.getArtifact(id, node.artifacts);
                }
            }
        }
        return item;
    }

}

export interface IProjectNode {
    Type: string;
    Id: number;
    ParentFolderId: number;
    Name: string;
    Description?: string;
    Children?: IProjectNode[];
}
