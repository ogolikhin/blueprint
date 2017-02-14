import {AdminStoreModels, Models, SearchServiceModels} from "../../models";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";

export abstract class SearchResultVM<T extends SearchServiceModels.ISearchResult> implements Models.IViewModel<T> {
    public abstract readonly id: string;
    public abstract readonly iconClass: string;

    constructor(
        public model: T,
        private onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean,
        public selectable: boolean = true,
        public project?: AdminStoreModels.IInstanceItem) {
    }

    public selected(value?: boolean): boolean {
        return this.selectable && this.onSelect(this, value);
    }
}

export class ProjectSearchResultVM extends SearchResultVM<SearchServiceModels.IProjectSearchResult> {
    public readonly id = "";
    public readonly iconClass = "icon-project";

    constructor(
        model: SearchServiceModels.IProjectSearchResult,
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
        isItemSelectable?: (params: {item: Models.IArtifact | Models.ISubArtifactNode}) => boolean,
        selectableItemTypes?: ItemTypePredefined[],
        project?: AdminStoreModels.IInstanceItem) {
        super(model, onSelect, (!isItemSelectable || isItemSelectable({item: model})) &&
            (!selectableItemTypes || selectableItemTypes.indexOf(model.predefinedType) !== -1), project);
        this.id = `${this.model.prefix}${this.model.id}`;
        this.iconClass = `icon-${_.kebabCase(ItemTypePredefined[this.model.predefinedType])}`;
    }
}
