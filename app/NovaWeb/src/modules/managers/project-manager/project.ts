import { Models, Enums } from "../../main/models";
import { IArtifactManager, IStatefulArtifact, IProjectArtifact } from "../models";

export class ProjectArtifact implements IProjectArtifact {
    private _artifact: IStatefulArtifact;
    private _parent: IProjectArtifact;
    public children: IProjectArtifact[];

    constructor(artifact: IStatefulArtifact, parent?: IProjectArtifact ) { //
        if (!artifact) {
            throw new Error("Artifact_Not_Found");
        }
        this._artifact = artifact;
        this._parent = parent;
        this.hasChildren = artifact.hasChildren;
    };

    public get artifact(): IStatefulArtifact {
        return this._artifact;
    }


    public get parent(): IProjectArtifact {
        return this._parent;
    }

    public get id(): number {
        return this._artifact.id;
    }

    public get name(): string {
        return this._artifact.name;
    }

    public get projectId() {
        return this._artifact.projectId;
    }

    public get parentId(): number {
        return this._artifact.parentId;
    }

    public get permissions(): Enums.RolePermissions {
        return this._artifact.permissions;
    }

    public get predefinedType(): Models.ItemTypePredefined {
        return this._artifact.predefinedType;
    }

    public hasChildren: boolean;
    public loaded: boolean;
    public open: boolean;
    
} 

export class Project extends ProjectArtifact { 

    public meta: Models.IProjectMeta;

    public get description(): string {
        return this.artifact.description;
    }

    public get projectId() {
        return this.artifact.id;
    }

    public get prefix(): string {
        return "PR";
    }
    public get parentId(): number {
        return -1;
    }

    public get permissions(): Enums.RolePermissions {
        return 4095;
    }

    public get predefinedType(): Models.ItemTypePredefined {
        return Enums.ItemTypePredefined.Project;
    }


    // public getArtifact(id: number, project?: Models.IArtifact): Models.IArtifact {
    //     let foundArtifact: Models.IArtifact;
    //     if (project) {
    //         if (project.id === id) {
    //             foundArtifact = project;
    //         }
    //         for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = project.artifacts[i++]); ) {
    //             if (it.id === id) {
    //                 foundArtifact = it;
    //             } else if (it.artifacts) {
    //                 foundArtifact = this.getArtifact(id, it);
    //             }
    //         }
    //     } else {
    //         for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = this.projectCollection.getValue()[i++]); ) {
    //             foundArtifact = this.getArtifact(id, it);
    //         }
    //     }
    //     return foundArtifact;
    // };


    public getArtifact(id: number, item?: IProjectArtifact): IProjectArtifact {
        let foundArtifact: IProjectArtifact;
        if (!item) {
            item = this;
        }
        if (item.id === id) {
            foundArtifact = item;
        } else if (item.children) {
            for (let i = 0, it: IProjectArtifact; !foundArtifact && (it = item.children[i++]); ) {
                foundArtifact = this.getArtifact(id, it);
            }
        } 
        return foundArtifact;
    };


    public getArtifactTypes(id?: number): Models.IItemType[] {

        let itemTypes: Models.IItemType[] = [];

        if (this.meta && this.meta.artifactTypes) {
            itemTypes = this.meta.artifactTypes.filter((it) => {
                return !angular.isNumber(id) || it.id === id;
            });
        }

        return itemTypes;
    }


    public getPropertyTypes(id?: number): Models.IPropertyType[] {

        let propertyTypes: Models.IPropertyType[] = [];

        if (this.meta && this.meta.propertyTypes) {
            propertyTypes = this.meta.propertyTypes.filter((it) => {
                return !angular.isNumber(id) || it.id === id;
            });
        }
        return propertyTypes;
    }
}
