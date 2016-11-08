import {
    RolePermissions
} from "./enums";

export enum InstanceItemType {
    Folder = 0,
    Project = 1
}

export interface IInstanceItem {
    id: number;
    parentFolderId: number;
    name: string;
    description?: string;
    type: InstanceItemType;
    hasChildren: boolean;
    permissions?: RolePermissions;
}
