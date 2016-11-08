import * as angular from "angular";
import {Models, SearchServiceModels, TreeViewModels} from "../../models";
import {Helper} from "../../../shared/";
import {IProjectService} from "../../../managers/project-manager/project-service";

export class SearchResultVMFactory extends TreeViewModels.TreeNodeVMFactory {
    constructor(projectService: IProjectService,
                private onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean,
                isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean,
                selectableItemTypes?: Models.ItemTypePredefined[],
                showSubArtifacts?: boolean) {
        super(projectService, isItemSelectable, selectableItemTypes, showSubArtifacts);
    }

    public createProjectSearchResultVM(model: SearchServiceModels.ISearchResult): ProjectSearchResultVM {
        return new ProjectSearchResultVM(model, this.onSelect);
    }

    public createArtifactSearchResultVM(model: SearchServiceModels.IItemNameSearchResult): ArtifactSearchResultVM {
        return new ArtifactSearchResultVM(model, this.onSelect, this.isSelectable(model));
    }
}

export abstract class SearchResultVM<T extends SearchServiceModels.ISearchResult> implements TreeViewModels.IViewModel<T> {
    public abstract readonly id: string;
    public abstract readonly iconClass: string;

    constructor(
        public model: T,
        private onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean,
        public readonly isSelectable: boolean = true) {
    }

    public selected(value?: boolean): boolean {
        return this.isSelectable && this.onSelect(this, value);
    }
}

export class ProjectSearchResultVM extends SearchResultVM<SearchServiceModels.ISearchResult> {
    public readonly id = "";
    public readonly iconClass = "icon-project";

    constructor(
        model: SearchServiceModels.ISearchResult,
        onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean) {
        super(model, onSelect);
    }
}

export class ArtifactSearchResultVM extends SearchResultVM<SearchServiceModels.IItemNameSearchResult> {
    public readonly id: string;
    public readonly iconClass: string;

    constructor(
        model: SearchServiceModels.IItemNameSearchResult,
        onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean,
        isSelectable: boolean) {
        super(model, onSelect, isSelectable);
        this.id = `${this.model.prefix}${this.model.id}`;
        this.iconClass = `icon-${_.kebabCase(Models.ItemTypePredefined[this.model.predefinedType])}`;
    }
}
