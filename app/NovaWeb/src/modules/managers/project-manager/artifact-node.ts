import * as angular from "angular";
import {Models, Enums} from "../../main/models";
import {IStatefulArtifact} from "../artifact-manager";
import {IArtifactNode} from "../project-manager";

export class ArtifactNode implements IArtifactNode {
    private _artifact: IStatefulArtifact;
    public name: string;
    public children: IArtifactNode[];
    public parentNode: IArtifactNode;

    constructor(artifact: IStatefulArtifact, parentNode?: IArtifactNode) {
        if (!artifact) {
            throw new Error("Artifact_Not_Found");
        }
        this._artifact = artifact;
        this.name = artifact ? artifact.name : null;
        this.parentNode = parentNode;
        if (parentNode) {
            this.hasChildren = artifact.hasChildren;
        } else {
            // for projects
            this.hasChildren = true;
            this.open = true;
        }
    };

    public dispose() {
        if (angular.isArray(this.children)) {
            this.children.forEach((it: IArtifactNode) => it.dispose);
        }
        delete this.children;
        delete this.parentNode;
        delete this._artifact;
    }

    public get artifact(): IStatefulArtifact {
        return this._artifact;
    }

    public get id(): number {
        return !this._artifact ? null : this._artifact.id;
    }

    public get parentId(): number {
        return !this._artifact ? null : this._artifact.parentId;
    }

    public hasChildren: boolean;
    public loaded: boolean;
    public open: boolean;

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
}
