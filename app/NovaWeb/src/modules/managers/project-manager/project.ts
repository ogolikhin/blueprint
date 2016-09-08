import { Models, Enums } from "../../main/models";
import { IArtifactManager, IStatefulArtifact, IProjectArtifact } from "../models";

export class Project implements IProjectArtifact { 
    private _artifact: IStatefulArtifact;
    private _parent: IProjectArtifact;
    private _children: IProjectArtifact[];

    constructor(artifact: IStatefulArtifact, parent?: IProjectArtifact ) { //
        if (!artifact) {
            throw new Error("Project_Not_Found");
        }
        this._artifact = artifact;
    };

    public get artifact(): IStatefulArtifact {
        return this._artifact;
    }

    public get children(): IProjectArtifact[] {
        return this._children || (this._children = []);
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

    public get description(): string {
        return this._artifact.description;
    }

    public get itemTypeId(): number {
        return this._artifact.itemTypeId;
    }

    public get itemTypeVersionId(): number {
        return this._artifact.itemTypeVersionId;
    }

    public meta: Models.IProjectMeta;

    public get projectId() {
        return this._artifact.id;
    }

    public get prefix(): string {
        return "PR";
    }
    public get parentId(): number {
        return -1;
    }
    public lockedByUserId: number;

    public get permissions(): Enums.RolePermissions {
        return 4095;
    }

    public get predefinedType(): Models.ItemTypePredefined {
        return Enums.ItemTypePredefined.Project;
    }

    public get hasChildren() {
        return this.children.length > 0;
    }

    public getArtifact(id: number, children?: IProjectArtifact[]): IStatefulArtifact {
        let foundArtifact: IStatefulArtifact;
        if (id === this.id) {
            foundArtifact = this.artifact;
        } else {
            for (let i = 0, it: IProjectArtifact; !foundArtifact && (it = this.children[i++]); ) {
                if (it.artifact.id === id) {
                    foundArtifact = it.artifact;
                } else if (it.children.length) {
                    foundArtifact = this.getArtifact(id, it.children);
                }
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
