import * as angular from "angular";
import * as _ from "lodash";
import {Models, AdminStoreModels} from "./";
import {Helper} from "../../shared/";
import {ITreeViewNode} from "../../shared/widgets/bp-tree-view/";
import {IProjectService} from "../../managers/project-manager/project-service";

export interface IViewModel<T> {
    model: T;
}

export abstract class TreeViewNodeVM<T> implements IViewModel<T>, ITreeViewNode {
    constructor(public model: T,
                public key: string,
                public group: boolean,
                public expanded: boolean,
                public selectable: boolean,
                public children?: ITreeViewNode[]) {
    }

    public getCellClass(): string[] {
        const result = [] as string[];
        if (this.group) {
            result.push("has-children");
        }
        if (!this.selectable) {
            result.push("not-selectable");
        }
        return result;
    }

    public getIcon(): string {
        return "<i></i>";
    }

    public abstract getLabel(): string;
}

export class TreeNodeVMFactory {
    constructor(public projectService: IProjectService,
                public isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean,
                public selectableItemTypes?: Models.ItemTypePredefined[],
                public showSubArtifacts?: boolean) {
    }

    public createInstanceItemNodeVM(model: AdminStoreModels.IInstanceItem, expanded: boolean = false): InstanceItemNodeVM {
        return new InstanceItemNodeVM(this, model, expanded);
    }

    public createArtifactNodeVM(project: AdminStoreModels.IInstanceItem, model: Models.IArtifact): ArtifactNodeVM {
        return new ArtifactNodeVM(this, project, model, this.isSelectable(model), this.showSubArtifacts);
    }

    public createSubArtifactContainerNodeVM(project: AdminStoreModels.IInstanceItem, model: Models.IArtifact, name: string): SubArtifactContainerNodeVM {
        return new SubArtifactContainerNodeVM(this, project, model, name);
    }

    public createSubArtifactNodeVM(project: AdminStoreModels.IInstanceItem, model: Models.ISubArtifactNode): SubArtifactNodeVM {
        return new SubArtifactNodeVM(this, project, model, this.isSelectable(model));
    }

    public static processChildArtifacts(children: Models.IArtifact[], artifactPath: string[], idPath: number[]): Models.IArtifact[] {
        children = children.filter(child => child.predefinedType !== Models.ItemTypePredefined.CollectionFolder);
        children.forEach((value: Models.IArtifact) => {
            value.artifactPath = artifactPath;
            value.idPath = idPath;
        });
        return children;
    }

    protected isSelectable(item: Models.IArtifact | Models.ISubArtifact) {
        return (!this.isItemSelectable || this.isItemSelectable({item: item})) &&
            (!this.selectableItemTypes || this.selectableItemTypes.indexOf(item.predefinedType) !== -1);
    }
}

export class InstanceItemNodeVM extends TreeViewNodeVM<AdminStoreModels.IInstanceItem> {
    constructor(private factory: TreeNodeVMFactory,
                model: AdminStoreModels.IInstanceItem,
                expanded: boolean = false) {
        super(model, String(model.id), model.hasChildren, expanded, true);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        switch (this.model.type) {
            case AdminStoreModels.InstanceItemType.Folder:
                result.push("is-folder");
                break;
            case AdminStoreModels.InstanceItemType.Project:
                result.push("is-project");
                break;
            default:
                break;
        }
        return result;
    }

    public getLabel(): string {
        return this.model.name;
    }

    public loadChildrenAsync(): ng.IPromise<ITreeViewNode[]> {
        switch (this.model.type) {
            case AdminStoreModels.InstanceItemType.Folder:
                return this.factory.projectService.getFolders(this.model.id).then((children: AdminStoreModels.IInstanceItem[]) => {
                    return children.map(child => this.factory.createInstanceItemNodeVM(child));
                });
            case AdminStoreModels.InstanceItemType.Project:
                return this.factory.projectService.getArtifacts(this.model.id).then((children: Models.IArtifact[]) => {
                    return TreeNodeVMFactory.processChildArtifacts(children, [this.model.name], [this.model.id])
                        .map(child => this.factory.createArtifactNodeVM(this.model, child));
                });
            default:
                return;
        }
    }
}

export class ArtifactNodeVM extends TreeViewNodeVM<Models.IArtifact> {
    constructor(private factory: TreeNodeVMFactory,
                public project: AdminStoreModels.IInstanceItem,
                model: Models.IArtifact,
                isSelectable: boolean,
                private showSubArtifacts?: boolean) {
        super(model, String(model.id), model.hasChildren ||
            (Boolean(showSubArtifacts) && Models.ItemTypePredefined.canContainSubartifacts(model.predefinedType)), false, isSelectable);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        const typeName = Models.ItemTypePredefined[this.model.predefinedType];
        if (typeName) {
            result.push("is-" + _.kebabCase(typeName));
        }
        return result;
    }

    public getIcon(): string {
        if (_.isFinite(this.model.itemTypeIconId)) {
            return `<bp-item-type-icon item-type-id="${this.model.itemTypeId}" item-type-icon-id="${this.model.itemTypeIconId}"></bp-item-type-icon>`;
        } else {
            return super.getIcon();
        }
    }

    public getLabel(): string {
        return `${this.model.prefix}${this.model.id} ${this.model.name}`;
    }

    public loadChildrenAsync(): ng.IPromise<ITreeViewNode[]> {
        return this.factory.projectService.getArtifacts(this.model.projectId, this.model.id).then((children: Models.IArtifact[]) => {
            const result: ITreeViewNode[] = TreeNodeVMFactory.processChildArtifacts(children, _.concat(this.model.artifactPath, this.model.name),
                _.concat(this.model.idPath, this.model.id))
                .map(child => this.factory.createArtifactNodeVM(this.project, child));
            if (this.showSubArtifacts && Models.ItemTypePredefined.canContainSubartifacts(this.model.predefinedType)) {
                const name = Models.ItemTypePredefined.getSubArtifactsContainerNodeTitle(this.model.predefinedType);
                result.unshift(this.factory.createSubArtifactContainerNodeVM(this.project, this.model, name)); //TODO localize
            }
            return result;
        });
    }
}

export class SubArtifactContainerNodeVM extends TreeViewNodeVM<Models.IArtifact> {
    constructor(private factory: TreeNodeVMFactory,
                public project: AdminStoreModels.IInstanceItem,
                model: Models.IArtifact,
                private name: string) {
        super(model, `${model.id} ${name}`, true, false, false);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        result.push("is-subartifact");
        return result;
    }

    public getLabel(): string {
        return this.name;
    }

    public loadChildrenAsync(): ng.IPromise<ITreeViewNode[]> {
        return this.factory.projectService.getSubArtifactTree(this.model.id).then((children: Models.ISubArtifactNode[]) => {
            return children.map(child => this.factory.createSubArtifactNodeVM(this.project, child));
        });
    }
}

export class SubArtifactNodeVM extends TreeViewNodeVM<Models.ISubArtifactNode> {
    constructor(private factory: TreeNodeVMFactory,
                public project: AdminStoreModels.IInstanceItem,
                model: Models.ISubArtifactNode,
                isSelectable: boolean) {
        super(model, String(model.id), model.hasChildren, false, isSelectable,
            model.children ? model.children.map(child => factory.createSubArtifactNodeVM(project, child)) : []);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        result.push("is-subartifact");
        return result;
    }

    public getLabel(): string {
        return `${this.model.prefix}${this.model.id} ${this.model.displayName}`;
    }
}
