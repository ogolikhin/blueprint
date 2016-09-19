import { Models } from "../../../models";
import { Helper } from "../../../../shared/";
import { ITreeViewNodeVM } from "../../../../shared/widgets/bp-tree-view/";
import { IProjectService } from "../../../../managers/project-manager/project-service";

export abstract class ArtifactPickerNodeVM<T> implements ITreeViewNodeVM {
    constructor(public model: T,
        public name: string,
        public key: string,
        public isExpandable: boolean,
        public children: ArtifactPickerNodeVM<any>[],
        public isExpanded: boolean) {
    }

    public abstract getTypeClass(): string;

    protected static processChildArtifacts(children: Models.IArtifact[], parent: Models.IArtifact): Models.IArtifact[] {
        children = children.filter(child => child.predefinedType !== Models.ItemTypePredefined.CollectionFolder);
        children.forEach((value: Models.IArtifact) => {
            value.parent = parent;
        });
        return children;
    }
}

export class InstanceItemNodeVM extends ArtifactPickerNodeVM<Models.IProjectNode> {
    constructor(private projectService: IProjectService, model: Models.IProjectNode, isExpanded: boolean = false) {
        super(model, model.name, model.id.toString(), model.hasChildren, [], isExpanded);
    }

    public getTypeClass(): string {
        switch (this.model.type) {
            case Models.ProjectNodeType.Folder:
                return "is-folder";
            case Models.ProjectNodeType.Project:
                return "is-project";
            default:
                return undefined;
        }
    }

    public loadChildrenAsync(): ng.IPromise<void> {
        this.loadChildrenAsync = undefined;
        switch (this.model.type) {
            case Models.ProjectNodeType.Folder:
                return this.projectService.getFolders(this.model.id).then((children: Models.IProjectNode[]) => {
                    this.children = children.map(child => new InstanceItemNodeVM(this.projectService, child));
                });
            case Models.ProjectNodeType.Project:
                return this.projectService.getArtifacts(this.model.id).then((children: Models.IArtifact[]) => {
                    children = ArtifactPickerNodeVM.processChildArtifacts(children, this.model);
                    this.children = children.map(child => new ArtifactNodeVM(this.projectService, child));
                });
            default:
                return;
        }
    }
}

export class ArtifactNodeVM extends ArtifactPickerNodeVM<Models.IArtifact> {
    constructor(private projectService: IProjectService, model: Models.IArtifact) {
        super(model, `${model.prefix}${model.id} ${model.name}`, model.id.toString(),
            model.hasChildren || Models.ItemTypePredefined.canContainSubartifacts(model.predefinedType), [], false);
    }

    public getTypeClass(): string {
        switch (this.model.predefinedType) {
            case Models.ItemTypePredefined.PrimitiveFolder:
                return "is-folder";
            case Models.ItemTypePredefined.Project:
                return "is-project";
            default:
                var typeName = Models.ItemTypePredefined[this.model.predefinedType];
                return typeName ? "is-" + Helper.toDashCase(typeName) : undefined;
        }
    }

    public loadChildrenAsync(): ng.IPromise<void> {
        this.loadChildrenAsync = undefined;
        return this.projectService.getArtifacts(this.model.projectId, this.model.id).then((children: Models.IArtifact[]) => {
            children = ArtifactPickerNodeVM.processChildArtifacts(children, this.model);
            this.children = children.map(child => new ArtifactNodeVM(this.projectService, child));
            if (Models.ItemTypePredefined.canContainSubartifacts(this.model.predefinedType)) {
                const name = Models.ItemTypePredefined.getSubArtifactsContainerNodeTitle(this.model.predefinedType);
                this.children.unshift(new SubArtifactContainerNodeVM(this.projectService, this.model, name)); //TODO localize
            }
        });
    }
}

export class SubArtifactContainerNodeVM extends ArtifactPickerNodeVM<Models.IArtifact> {
    constructor(private projectService: IProjectService, model: Models.IArtifact, name: string) {
        super(model, name, `${model.id} ${name}`, true, [], false);
    }

    public getTypeClass(): string {
        return "is-subartifact";
    }

    public loadChildrenAsync(): ng.IPromise<void> {
        this.loadChildrenAsync = undefined;
        return this.projectService.getSubArtifactTree(this.model.id).then((children: Models.ISubArtifactNode[]) => {
            this.children = children.map(child => new SubArtifactNodeVM(child));
        });
    }
}

export class SubArtifactNodeVM extends ArtifactPickerNodeVM<Models.ISubArtifactNode> {
    constructor(model: Models.ISubArtifactNode) {
        const children = model.children ? model.children.map(child => new SubArtifactNodeVM(child)) : [];
        super(model, `${model.prefix}${model.id} ${model.displayName}`, model.id.toString(), model.hasChildren, children, false);
    }

    public getTypeClass(): string {
        return "is-subartifact";
    }
}
