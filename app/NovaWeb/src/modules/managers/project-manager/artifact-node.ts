import {Models, Enums, TreeViewModels} from "../../main/models";
import {IProjectService} from "./project-service";
import {IArtifactManager} from "../../managers";
import {IStatefulArtifactFactory, IStatefulArtifact} from "../artifact-manager";
import {IArtifactNode} from "../project-manager";

export class ArtifactNode implements IArtifactNode {
    private _model: IStatefulArtifact;
    public group: boolean;
    public children: IArtifactNode[];
    public key: string;
    public selectable: boolean = true;

    constructor(private projectService: IProjectService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private artifactManager: IArtifactManager,
                artifact: IStatefulArtifact,
                public expanded: boolean = false) {
        if (!artifact) {
            throw new Error("Artifact_Not_Found");
        }
        this._model = artifact;
        this.key = String(artifact.id);
        this.group = artifact.hasChildren;
    };

    public unloadChildren() {
        if (_.isArray(this.children)) {
            this.children.forEach((it: IArtifactNode) => it.unloadChildren);
        }
        this.children = undefined;
    }

    public get model(): IStatefulArtifact {
        return this._model;
    }

    public loadChildrenAsync(): ng.IPromise<IArtifactNode[]> {
        return this.projectService.getArtifacts(this.model.projectId, this.model.id).then((children: Models.IArtifact[]) => {
            return children.map((it: Models.IArtifact) => {
                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                this.artifactManager.add(statefulArtifact);
                return new ArtifactNode(this.projectService, this.statefulArtifactFactory, this.artifactManager, statefulArtifact);
            });
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
