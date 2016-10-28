import * as angular from "angular";
import {Models, AdminStoreModels, SearchServiceModels} from "../../models";
import {Helper} from "../../../shared/";
import {ITreeViewNodeVM} from "../../../shared/widgets/bp-tree-view/";
import {IArtifactManager} from "../../../managers";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {IArtifactPickerOptions} from "./bp-artifact-picker";

export interface IViewModel<T> {
    model: T;
}

export abstract class ArtifactPickerNodeVM<T> implements IViewModel<T>, ITreeViewNodeVM {
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
        if (!this.isSelectable()) {
            result.push("not-selectable");
        }
        return result;
    }

    public getIcon(): string {
        return "<i></i>";
    }

    public isSelectable(): boolean {
        return true;
    }

    protected static processChildArtifacts(children: Models.IArtifact[], parent: Models.IArtifact): Models.IArtifact[] {
        children = children.filter(child => child.predefinedType !== Models.ItemTypePredefined.CollectionFolder);
        children.forEach((value: Models.IArtifact) => {
            value.parent = parent;
        });
        return children;
    }
}

export class InstanceItemNodeVM extends ArtifactPickerNodeVM<AdminStoreModels.IInstanceItem> {
    constructor(private artifactManager: IArtifactManager,
                private projectService: IProjectService,
                private options: IArtifactPickerOptions,
                model: AdminStoreModels.IInstanceItem,
                isExpanded: boolean = false) {
        super(model, model.name, String(model.id), model.hasChildren, [], isExpanded);
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

    public loadChildrenAsync(): ng.IPromise<void> {
        this.loadChildrenAsync = undefined;
        switch (this.model.type) {
            case AdminStoreModels.InstanceItemType.Folder:
                return this.projectService.getFolders(this.model.id).then((children: AdminStoreModels.IInstanceItem[]) => {
                    this.children = children.map(child => new InstanceItemNodeVM(this.artifactManager, this.projectService, this.options, child));
                });
            case AdminStoreModels.InstanceItemType.Project:
                return this.projectService.getArtifacts(this.model.id).then((children: Models.IArtifact[]) => {
                    children = ArtifactPickerNodeVM.processChildArtifacts(children, this.model);
                    this.children = children.map(child => new ArtifactNodeVM(this.artifactManager, this.projectService, this.options, child));
                });
            default:
                return;
        }
    }
}

export class ArtifactNodeVM extends ArtifactPickerNodeVM<Models.IArtifact> {
    constructor(private artifactManager: IArtifactManager,
                private projectService: IProjectService,
                private options: IArtifactPickerOptions,
                model: Models.IArtifact) {
        super(model, `${model.prefix}${model.id} ${model.name}`, String(model.id),
            model.hasChildren || (Boolean(options.showSubArtifacts) && Models.ItemTypePredefined.canContainSubartifacts(model.predefinedType)), [], false);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        const typeName = Models.ItemTypePredefined[this.model.predefinedType];
        if (typeName) {
            result.push("is-" + Helper.toDashCase(typeName));
        }
        return result;
    }

    public getIcon(): string {
        //TODO: for now it display custom icons just for already loaded projects
        let statefulArtifact = this.artifactManager.get(this.model.id);
        if (statefulArtifact) {
            let artifactType = statefulArtifact.metadata.getItemTypeTemp();
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
            this.children = children.map(child => new ArtifactNodeVM(this.artifactManager, this.projectService, this.options, child));
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
        const result = super.getCellClass();
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
            model.children ? model.children.map(child => new SubArtifactNodeVM(options, child)) : [], false);
    }

    public getCellClass(): string[] {
        const result = super.getCellClass();
        result.push("is-subartifact");
        return result;
    }

    public isSelectable(): boolean {
        return !(this.options &&
        this.options.selectableItemTypes &&
        this.options.selectableItemTypes.indexOf(this.model.predefinedType) === -1);
    }
}

export class SearchResultVM implements IViewModel<SearchServiceModels.ISearchResult> {
    constructor(
        public model: SearchServiceModels.ISearchResult,
        private onSelect: (vm: SearchResultVM, value?: boolean) => boolean) {
    }

    public selected(value?: boolean): boolean {
        return this.onSelect(this, value);
    }
}
