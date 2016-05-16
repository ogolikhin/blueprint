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
    children: IProjectItem[]
    //flags:
}

export interface IProject {
    id: number;
    name: string;
    artifacts: IProjectItem[]
}

export class Project implements IProject {
    public id: number;
    public name: string;
    public artifacts: IProjectItem[]

    constructor(data?: IProjectItem[]) {
        this.artifacts = data;
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
