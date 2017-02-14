import {IItemChangeSet} from "../../managers/artifact-manager/changeset";
import {IProjectService} from "../../managers/project-manager/project-service";
import {ITreeNode} from "../../shared/widgets/bp-tree-view";
import {AdminStoreModels, Models} from "./";
import {ItemTypePredefined} from "./itemTypePredefined.enum";

export interface ITreeNodeVM<T> extends Models.IViewModel<T>, ITreeNode {
    getCellClass(): string[];
    getIcon(): string;
    getLabel(): string;
}

abstract class TreeNodeVM<T> implements ITreeNodeVM<T>, ITreeNode {
    public children?: ITreeNode[];

    constructor(public model: T,
                public key: string,
                public group: boolean,
                public expanded: boolean,
                public selectable: boolean) {
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
                public timeout?: ng.IPromise<void>,
                public isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean,
                public selectableItemTypes?: ItemTypePredefined[],
                public showArtifacts: boolean = true,
                public showBaselinesAndReviews: boolean = false,
                public showCollections: boolean = false,
                public showSubArtifacts: boolean = false) {
    }

    public createExplorerNodeVM(model: Models.IArtifact, expanded: boolean = false): ExplorerNodeVM {
        return new ExplorerNodeVM(this, model, expanded);
    }

    public createInstanceItemNodeVM(model: AdminStoreModels.IInstanceItem, expanded: boolean = false): InstanceItemNodeVM {
        return new InstanceItemNodeVM(this, model, expanded);
    }

    public createArtifactNodeVM(project: AdminStoreModels.IInstanceItem, model: Models.IArtifact, expanded: boolean = false): ArtifactNodeVM {
        return new ArtifactNodeVM(this, project, model, this.isSelectable(model), expanded);
    }

    public createSubArtifactContainerNodeVM(project: AdminStoreModels.IInstanceItem, model: Models.IArtifact, name: string): SubArtifactContainerNodeVM {
        return new SubArtifactContainerNodeVM(this, project, model, name);
    }

    public createSubArtifactNodeVM(project: AdminStoreModels.IInstanceItem, model: Models.ISubArtifactNode,
                                   parentArtifact: Models.IArtifact): SubArtifactNodeVM {
        return new SubArtifactNodeVM(this, project, model, this.isSelectable(model), parentArtifact);
    }

    public static processChildArtifacts(children: Models.IArtifact[], artifactPath: string[],
                                        idPath: number[], parentPredefinedType: ItemTypePredefined): Models.IArtifact[] {
        children.forEach((value: Models.IArtifact) => {
            value.artifactPath = artifactPath;
            value.idPath = idPath;
            value.parentPredefinedType = parentPredefinedType;
        });
        return children;
    }

    private isSelectable(item: Models.IArtifact | Models.ISubArtifact) {
        return (!this.isItemSelectable || this.isItemSelectable({item: item})) &&
            (!this.selectableItemTypes || this.selectableItemTypes.indexOf(item.predefinedType) !== -1);
    }
}

abstract class HomogeneousTreeNodeVM<T> extends TreeNodeVM<T> {
    public children: this[];

    constructor(model: T,
                key: string,
                group: boolean,
                expanded: boolean,
                selectable: boolean) {
        super(model, key, group, expanded, selectable);
    }

    public unloadChildren() {
        if (_.isArray(this.children)) {
            this.children.forEach((child: this) => {
                child.unloadChildren();
            });
        }
        this.children = null;
    }

    public getNode(comparator: T | ((model: T) => boolean), item?: this): this {
        let found: this;
        if (!item) {
            item = this;
        }
        if (_.isFunction(comparator) ? comparator(item.model) : item.model === comparator) {
            found = item;
        } else if (item.children) {

            //todo: we should find a better way to write this as its not very clear. prob lodash has support to do better
            for (let i = 0, it: this;
                 !found &&
                 (it = item.children[i++]);
            ) {
                found = this.getNode(comparator, it);
            }
        }
        return found;
    };
}

export class ExplorerNodeVM extends HomogeneousTreeNodeVM<Models.IArtifact> {
    constructor(private factory: TreeNodeVMFactory,
                model: Models.IArtifact,
                expanded: boolean = false) {
        super(model, String(model.id), model.hasChildren, expanded, true);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        let typeName: string;
        if (this.model.predefinedType === ItemTypePredefined.BaselineFolder &&
            this.model.itemTypeId === ItemTypePredefined.BaselinesAndReviews) {
            typeName = ItemTypePredefined[ItemTypePredefined.BaselinesAndReviews];
        } else if (this.model.predefinedType === ItemTypePredefined.CollectionFolder &&
            this.model.itemTypeId === ItemTypePredefined.Collections) {
            typeName = ItemTypePredefined[ItemTypePredefined.Collections];
        } else {
            typeName = ItemTypePredefined[this.model.predefinedType];
        }
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
        return this.model.name;
    }

    public loadChildrenAsync(): ng.IPromise<ITreeNode[]> {
        return this.factory.projectService.getArtifacts(this.model.projectId, this.model.id, this.factory.timeout).then((children: Models.IArtifact[]) => {
            return children.map((it: Models.IArtifact) => {
                return this.factory.createExplorerNodeVM(it);
            });
        });
    }

    private static minimalModel: Models.IArtifact = {
        id: undefined,
        name: undefined,
        itemTypeId: undefined,
        predefinedType: undefined,
        projectId: undefined,
        itemTypeIconId: undefined
    };

    /**
     * Updates the model with changes. Only properties that exist in the model, or in minimalModel are updated.
     *
     * @param {IItemChangeSet} changes
     */
    public updateModel(changes: IItemChangeSet) {
        if (changes.change) {
            if (changes.change.key in this.model || changes.change.key in ExplorerNodeVM.minimalModel) {
                this.model[changes.change.key] = changes.change.value;
            }
        } else {
            for (let key in _.pickBy(changes.item, (key, value) => !_.isFunction(value))) {
                if (key in this.model || key in ExplorerNodeVM.minimalModel) {
                    this.model[key] = changes.item[key];
                }
            }
        }
    }
}

export class InstanceItemNodeVM extends TreeNodeVM<AdminStoreModels.IInstanceItem> {
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

    public loadChildrenAsync(): ng.IPromise<ITreeNode[]> {
        switch (this.model.type) {
            case AdminStoreModels.InstanceItemType.Folder:
                return this.factory.projectService.getFolders(this.model.id, this.factory.timeout).then((children: AdminStoreModels.IInstanceItem[]) => {
                    return children.map(child => this.factory.createInstanceItemNodeVM(child));
                });
            case AdminStoreModels.InstanceItemType.Project:
                return this.factory.projectService.getArtifacts(this.model.id, undefined, this.factory.timeout).then((children: Models.IArtifact[]) => {
                    if (!this.factory.showArtifacts) {
                        children = children.filter(child => child.predefinedType === ItemTypePredefined.CollectionFolder ||
                            child.predefinedType === ItemTypePredefined.BaselineFolder);
                    }
                    if (!this.factory.showBaselinesAndReviews) {
                        children = children.filter(child => child.predefinedType !== ItemTypePredefined.BaselineFolder);
                    }
                    if (!this.factory.showCollections) {
                        children = children.filter(child => child.predefinedType !== ItemTypePredefined.CollectionFolder);
                    }
                    return TreeNodeVMFactory.processChildArtifacts(children, [this.model.name], [this.model.id], null)
                        .map(child => this.factory.createArtifactNodeVM(this.model, child, !this.factory.showArtifacts));
                });
            default:
                return;
        }
    }
}

export class ArtifactNodeVM extends TreeNodeVM<Models.IArtifact> {
    constructor(private factory: TreeNodeVMFactory,
                public project: AdminStoreModels.IInstanceItem,
                model: Models.IArtifact,
                isSelectable: boolean,
                expanded: boolean = false) {
        super(model, String(model.id), model.hasChildren ||
            (factory.showSubArtifacts && ItemTypePredefined.canContainSubartifacts(model.predefinedType)), expanded, isSelectable);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        let typeName: string;
        if (this.model.predefinedType === ItemTypePredefined.BaselineFolder &&
            this.model.itemTypeId === ItemTypePredefined.BaselinesAndReviews) {
            typeName = ItemTypePredefined[ItemTypePredefined.BaselinesAndReviews];
        } else if (this.model.predefinedType === ItemTypePredefined.CollectionFolder &&
            this.model.itemTypeId === ItemTypePredefined.Collections) {
            typeName = ItemTypePredefined[ItemTypePredefined.Collections];
        } else {
            typeName = ItemTypePredefined[this.model.predefinedType];
        }
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

    public loadChildrenAsync(): ng.IPromise<ITreeNode[]> {
        return this.factory.projectService.getArtifacts(this.model.projectId, this.model.id, this.factory.timeout).then((children: Models.IArtifact[]) => {
            const result: ITreeNode[] = TreeNodeVMFactory.processChildArtifacts(children, _.concat(this.model.artifactPath, this.model.name),
                _.concat(this.model.idPath, this.model.id), this.model.predefinedType)
                .map(child => this.factory.createArtifactNodeVM(this.project, child));
            if (this.factory.showSubArtifacts && ItemTypePredefined.canContainSubartifacts(this.model.predefinedType)) {
                const name = ItemTypePredefined.getSubArtifactsContainerNodeTitle(this.model.predefinedType);
                result.unshift(this.factory.createSubArtifactContainerNodeVM(this.project, this.model, name)); //TODO localize
            }
            return result;
        });
    }
}

export class SubArtifactContainerNodeVM extends TreeNodeVM<Models.IArtifact> {
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

    public loadChildrenAsync(): ng.IPromise<ITreeNode[]> {
        return this.factory.projectService.getSubArtifactTree(this.model.id, this.factory.timeout).then((children: Models.ISubArtifactNode[]) => {
            children.forEach(child => {
                child.artifactName = this.model.name;
                child.artifactTypePrefix = this.model.prefix;
            });
            return children.map(child => this.factory.createSubArtifactNodeVM(this.project, child, this.model));
        });
    }
}

export class SubArtifactNodeVM extends TreeNodeVM<Models.ISubArtifactNode> {
    constructor(private factory: TreeNodeVMFactory,
                public project: AdminStoreModels.IInstanceItem,
                model: Models.ISubArtifactNode,
                isSelectable: boolean,
                public parentArtifact: Models.IArtifact) {
        super(model, String(model.id), model.hasChildren, false, isSelectable);
        this.children = model.children ? model.children.map(child => factory.createSubArtifactNodeVM(project, child, parentArtifact)) : [];
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
