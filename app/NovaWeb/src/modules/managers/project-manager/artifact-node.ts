import * as angular from "angular";
import {Models, Enums, TreeViewModels} from "../../main/models";
import {IProjectService} from "./project-service";
import {IArtifactManager} from "../../managers";
import {IStatefulArtifactFactory, IStatefulArtifact} from "../artifact-manager";
import {IArtifactNode} from "../project-manager";

export class ArtifactNode implements IArtifactNode {
    private _artifact: IStatefulArtifact;
    public key: string;
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
        this.key = String(artifact.id);
        this.parentNode = parentNode;
        if (parentNode) {
            this.group = artifact.hasChildren;
        } else {
            // for projects
            this.group = true;
            this.expanded = true;
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

    public get model(): IStatefulArtifact {
        return this._artifact;
    }

    public group: boolean;
    public loaded: boolean;
    public expanded: boolean;

    public loadChildrenAsync(): ng.IPromise<any> {
        return this.projectService.getArtifacts(this.model.projectId, this.model.id).then((data: Models.IArtifact[]) => {
            this.children = data.map((it: Models.IArtifact) => {
                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                this.artifactManager.add(statefulArtifact);
                return new ArtifactNode(this.projectService, this.statefulArtifactFactory, this.artifactManager, statefulArtifact, this);
            });
           this.loaded = true;
           this.expanded = true;
        });
    }

    public getNode(id: number, item?: IArtifactNode): IArtifactNode {
        let found: IArtifactNode;
        if (!item) {
            item = this;
        }
        if (item.model.id === id) {
            found = item;
        } else if (item.children) {
            for (let i = 0, it: IArtifactNode; !found && (it = item.children[i++]); ) {
                found = this.getNode(id, it);
            }
        }
        return found;
    };
}
