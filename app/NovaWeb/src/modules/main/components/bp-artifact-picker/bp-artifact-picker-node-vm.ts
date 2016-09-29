import * as angular from "angular";
import { Models } from "../../models";
import { Helper } from "../../../shared/";
import { ITreeViewNodeVM } from "../../../shared/widgets/bp-tree-view/";
import { IProjectManager } from "../../../managers";
import { IProjectService } from "../../../managers/project-manager/project-service";
import { IArtifactPickerOptions } from "./bp-artifact-picker";

export abstract class ArtifactPickerNodeVM<T> implements ITreeViewNodeVM {
    constructor(public model: T,
        public name: string,
        public key: string,
        public isExpandable: boolean,
        public children: ArtifactPickerNodeVM<any>[],
        public isExpanded: boolean) {
    }

    public getCellClass(): string[] {
        const result = [] as string[];
        if (this.isExpandable) {
            result.push("has-children");
        }
        return result;
    }

    public getIcon(): string {
        return "<i></i>";
    }

    protected static processChildArtifacts(children: Models.IArtifact[], parent: Models.IArtifact): Models.IArtifact[] {
        children = children.filter(child => child.predefinedType !== Models.ItemTypePredefined.CollectionFolder);
        children.forEach((value: Models.IArtifact) => {
            value.parent = parent;
        });
        return children;
    }
}

export class InstanceItemNodeVM extends ArtifactPickerNodeVM<Models.IProjectNode> {
    constructor(private projectManager: IProjectManager,
        private projectService: IProjectService,
        private options: IArtifactPickerOptions,
        model: Models.IProjectNode,
        isExpanded: boolean = false) {
        super(model, model.name, String(model.id), model.hasChildren, [], isExpanded);
    }

    public getCellClass(): string[] {
        var result = super.getCellClass();
        switch (this.model.type) {
            case Models.ProjectNodeType.Folder:
                result.push("is-folder");
                break;
            case Models.ProjectNodeType.Project:
                result.push("is-project");
                break;
            default:
                break;
        }
        return result;
    }

    public loadChildrenAsync(): ng.IPromise<void> {
        this.loadChildrenAsync = undefined;
        switch (this.model.type) {
            case Models.ProjectNodeType.Folder:
                return this.projectService.getFolders(this.model.id).then((children: Models.IProjectNode[]) => {
                    this.children = children.map(child => new InstanceItemNodeVM(this.projectManager, this.projectService, this.options, child));
                });
            case Models.ProjectNodeType.Project:
                return this.projectService.getArtifacts(this.model.id).then((children: Models.IArtifact[]) => {
                    children = ArtifactPickerNodeVM.processChildArtifacts(children, this.model);
                    this.children = children.map(child => new ArtifactNodeVM(this.projectManager, this.projectService, this.options, child));
                });
            default:
                return;
        }
    }
}

export class ArtifactNodeVM extends ArtifactPickerNodeVM<Models.IArtifact> {
    constructor(private projectManager: IProjectManager,
        private projectService: IProjectService,
        private options: IArtifactPickerOptions,
        model: Models.IArtifact) {
        super(model, `${model.prefix}${model.id} ${model.name}`, String(model.id),
            model.hasChildren || (Boolean(options.showSubArtifacts) && Models.ItemTypePredefined.canContainSubartifacts(model.predefinedType)), [], false);
    }

    public getCellClass(): string[] {
        var result = super.getCellClass();
        switch (this.model.predefinedType) {
            case Models.ItemTypePredefined.PrimitiveFolder:
                result.push("is-folder");
                break;
            case Models.ItemTypePredefined.Project:
                result.push("is-project");
                break;
            default:
                var typeName = Models.ItemTypePredefined[this.model.predefinedType];
                if (typeName) {
                    result.push("is-" + Helper.toDashCase(typeName));
                }
                break;
        }
        return result;
    }

    public getIcon(): string {
        //TODO: for now it display custom icons just for already loaded projects
        let statefulArtifact = this.projectManager.getArtifact(this.model.id);
        if (statefulArtifact) {
            let artifactType = statefulArtifact.metadata.getItemType();
            if (artifactType && artifactType.iconImageId && angular.isNumber(artifactType.iconImageId)) {
                return `<bp-item-type-icon item-type-id="${artifactType.id}" item-type-icon="${artifactType.iconImageId}"></bp-item-type-icon>`;
            }
        }
        return super.getIcon();
    }

    public isSelectable(): boolean {
        return !(this.options &&
            this.options.selectableItemTypes &&
            this.options.selectableItemTypes.indexOf(this.model.predefinedType) === -1);
    }
 
    public loadChildrenAsync(): ng.IPromise<void> {
        this.loadChildrenAsync = undefined;
        return this.projectService.getArtifacts(this.model.projectId, this.model.id).then((children: Models.IArtifact[]) => {
            children = ArtifactPickerNodeVM.processChildArtifacts(children, this.model);
            this.children = children.map(child => new ArtifactNodeVM(this.projectManager, this.projectService, this.options, child));
            if (this.options.showSubArtifacts && Models.ItemTypePredefined.canContainSubartifacts(this.model.predefinedType)) {
                const name = Models.ItemTypePredefined.getSubArtifactsContainerNodeTitle(this.model.predefinedType);
                this.children.unshift(new SubArtifactContainerNodeVM(this.projectService, this.options, this.model, name)); //TODO localize
            }
        });
    }
}

export class SubArtifactContainerNodeVM extends ArtifactPickerNodeVM<Models.IArtifact> {
    constructor(private projectService: IProjectService, private options: IArtifactPickerOptions, model: Models.IArtifact, name: string) {
        super(model, name, `${model.id} ${name}`, true, [], false);
    }

    public getCellClass(): string[] {
        var result = super.getCellClass();
        result.push("is-subartifact");
        return result;
    }

    public isSelectable(): boolean {
        return false;
    }

    public loadChildrenAsync(): ng.IPromise<void> {
        this.loadChildrenAsync = undefined;
        return this.projectService.getSubArtifactTree(this.model.id).then((children: Models.ISubArtifactNode[]) => {
            this.children = children.map(child => new SubArtifactNodeVM(this.options, child));
        });
    }
}

export class SubArtifactNodeVM extends ArtifactPickerNodeVM<Models.ISubArtifactNode> {
    constructor(private options: IArtifactPickerOptions, model: Models.ISubArtifactNode) {
        super(model, `${model.prefix}${model.id} ${model.displayName}`, String(model.id), model.hasChildren,
            model.children ? model.children.map(child => new SubArtifactNodeVM(this.options, child)) : [], false);
    }

    public getCellClass(): string[] {
        var result = super.getCellClass();
        result.push("is-subartifact");
        return result;
    }

    public isSelectable(): boolean {
        return !(this.options &&
            this.options.selectableItemTypes &&
            this.options.selectableItemTypes.indexOf(this.model.predefinedType) === -1);
    }
}
