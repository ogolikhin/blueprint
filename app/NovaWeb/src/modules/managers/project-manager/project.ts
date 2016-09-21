import { Models, Enums } from "../../main/models";
import { IStatefulArtifact, IArtifactNode } from "../models";

export class ArtifactNode implements IArtifactNode {
    private _artifact: IStatefulArtifact;
    public children: IArtifactNode[];

    constructor(artifact: IStatefulArtifact ) { //
        if (!artifact) {
            throw new Error("Artifact_Not_Found");
        }
        this._artifact = artifact;
        this.hasChildren = artifact.hasChildren;
    };

    public get artifact(): IStatefulArtifact {
        return this._artifact;
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


export class Project extends ArtifactNode { 

    public meta: Models.IProjectMeta;
    public constructor(artifact: IStatefulArtifact) {
        super(artifact);
        this.open = true;
        this.hasChildren = true;

    }

    public get description(): string {
        return this.artifact.description;
    }

    public getNode(id: number, item?: IArtifactNode): IArtifactNode {
        let found: IArtifactNode;
        if (!item) {
            item = this;
        }
        if (item.id === id) {
            found = item;
        } else if (item.children) {
            for (let i = 0, it: IArtifactNode; !found && (it = item.children[i++]); ) {
                found = this.getNode(id, it);
            }
        } 
        return found;
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

    public getArtifactType(id: number): Models.IItemType {
        if (!id) {
            throw new Error("Artifact_NotFound");
        }
        if (!this.meta) {
            throw new Error("Project_MetaDataNotFound");
        }
        let node = this.getNode(id);
        
        let artifactType: Models.IItemType = this.meta.artifactTypes.filter((it: Models.IItemType) => {
            return it.id === node.artifact.itemTypeId;
        })[0];

        return artifactType;

    }    
    
}
