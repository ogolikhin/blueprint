import * as angular from "angular";
import {Models, Enums} from "../../main/models";
import {IProjectService} from "./project-service";
import {IArtifactManager} from "../../managers";
import {IStatefulArtifactFactory, IStatefulArtifact} from "../artifact-manager";
import {IArtifactNode} from "../project-manager";

export class ArtifactNode implements IArtifactNode {
    private _artifact: IStatefulArtifact;
    public name: string;
    public children: IArtifactNode[];
    public parentNode: IArtifactNode;

    constructor(private projectService: IProjectService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private artifactManager: IArtifactManager,
                artifact: IStatefulArtifact,
                parentNode?: IArtifactNode) {
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

    public loadChildrenAsync(): ng.IPromise<any> {
        return this.projectService.getArtifacts(this.artifact.projectId, this.artifact.id).then((data: Models.IArtifact[]) => {
            this.children = data.map((it: Models.IArtifact) => {
                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                this.artifactManager.add(statefulArtifact);
                return new ArtifactNode(this.projectService, this.statefulArtifactFactory, this.artifactManager, statefulArtifact, this);
            });
            this.loaded = true;
            this.open = true;
        });
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
}
