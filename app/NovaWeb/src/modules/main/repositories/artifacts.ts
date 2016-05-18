﻿import "angular";

export interface IItem {
    id: number;
    name: string;
    typeId: number;
    parentId: number;
    predefinedType: number;
}

export interface IProjectItem extends IItem {
    projectId: number;
    version: number;
    artifacts: IProjectItem[];
    hasChildren: boolean;  ///needs to be changed camelCase 
    //flags:
}

export interface IProject {
    id: number;
    name: string;
    artifacts: IProjectItem[];
    getArtifact(id: number, node?: IProjectItem[]): IProjectItem;
}

export class Project implements IProject {
    public id: number;
    public name: string;
    public artifacts: IProjectItem[];

    constructor(data?: IProjectItem[]) {
        this.artifacts = data;
    };

    public getArtifact(id: number, nodes?: IProjectItem[]): IProjectItem {
        let item: IProjectItem;
        if (!nodes) {
            return this.getArtifact(id, this.artifacts);
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

export interface IProjectNode {
    type: string;
    id: number;
    parentFolderId: number;
    name: string;
    description?: string;
    children?: IProjectNode[];
}
