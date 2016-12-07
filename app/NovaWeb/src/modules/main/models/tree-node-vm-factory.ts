import {Models, AdminStoreModels} from "./";
import {ITreeNode} from "../../shared/widgets/bp-tree-view";
import {IProjectService} from "../../managers/project-manager/project-service";
import {IArtifactManager, IStatefulArtifactFactory, IStatefulArtifact} from "../../managers/artifact-manager";

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
                public artifactManager: IArtifactManager,
                public statefulArtifactFactory: IStatefulArtifactFactory,
                public timeout?: ng.IPromise<void>,
                public isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean,
                public selectableItemTypes?: Models.ItemTypePredefined[],
                public showSubArtifacts?: boolean) {
    }

    public createStatefulArtifactNodeVM(model: IStatefulArtifact, expanded: boolean = false): StatefulArtifactNodeVM {
        return new StatefulArtifactNodeVM(this, model, expanded);
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

    public static processChildArtifacts(children: Models.IArtifact[], artifactPath: string[], 
        idPath: number[], parentPredefinedType: Models.ItemTypePredefined): Models.IArtifact[] {
        children = children.filter(child => child.predefinedType !== Models.ItemTypePredefined.CollectionFolder);
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

    constructor(public model: T,
                public key: string,
                public group: boolean,
                public expanded: boolean,
                public selectable: boolean) {
        super(model, key, group, expanded, selectable);
    }

    public unloadChildren() {
        if (_.isArray(this.children)) {
            this.children.forEach((it: this) => it.unloadChildren);
        }
        this.children = undefined;
    }

    public getNode(comparator: T | ((model: T) => boolean), item?: this): this {
        let found: this;
        if (!item) {
            item = this;
        }
        if (_.isFunction(comparator) ? comparator(item.model) : item.model === comparator) {
            found = item;
        } else if (item.children) {

            //todo: we shoudl find a better way to write this as its not very clear. prob lodash has support to do better
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

export class StatefulArtifactNodeVM extends HomogeneousTreeNodeVM<IStatefulArtifact> {
    constructor(private factory: TreeNodeVMFactory,
                model: IStatefulArtifact,
                expanded: boolean = false) {
        super(model, String(model.id), model.hasChildren, expanded, true);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        let typeName: string;
        if (this.model.predefinedType === Models.ItemTypePredefined.CollectionFolder &&
            this.model.itemTypeId === Models.ItemTypePredefined.Collections) {
            typeName = Models.ItemTypePredefined[Models.ItemTypePredefined.Collections];
        } else {
            typeName = Models.ItemTypePredefined[this.model.predefinedType];
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
                const statefulArtifact = this.factory.statefulArtifactFactory.createStatefulArtifact(it);
                this.factory.artifactManager.add(statefulArtifact);
                return this.factory.createStatefulArtifactNodeVM(statefulArtifact);
            });
        });
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
                    return TreeNodeVMFactory.processChildArtifacts(children, [this.model.name], [this.model.id], null)
                        .map(child => this.factory.createArtifactNodeVM(this.model, child));
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

    public loadChildrenAsync(): ng.IPromise<ITreeNode[]> {
        return this.factory.projectService.getArtifacts(this.model.projectId, this.model.id, this.factory.timeout).then((children: Models.IArtifact[]) => {
            const result: ITreeNode[] = TreeNodeVMFactory.processChildArtifacts(children, _.concat(this.model.artifactPath, this.model.name),
                _.concat(this.model.idPath, this.model.id), this.model.predefinedType)
                .map(child => this.factory.createArtifactNodeVM(this.project, child));
            if (this.showSubArtifacts && Models.ItemTypePredefined.canContainSubartifacts(this.model.predefinedType)) {
                const name = Models.ItemTypePredefined.getSubArtifactsContainerNodeTitle(this.model.predefinedType);
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
            return children.map(child => this.factory.createSubArtifactNodeVM(this.project, child));
        });
    }
}

export class SubArtifactNodeVM extends TreeNodeVM<Models.ISubArtifactNode> {
    constructor(private factory: TreeNodeVMFactory,
                public project: AdminStoreModels.IInstanceItem,
                model: Models.ISubArtifactNode,
                isSelectable: boolean) {
        super(model, String(model.id), model.hasChildren, false, isSelectable);
        this.children = model.children ? model.children.map(child => factory.createSubArtifactNodeVM(project, child)) : [];
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
