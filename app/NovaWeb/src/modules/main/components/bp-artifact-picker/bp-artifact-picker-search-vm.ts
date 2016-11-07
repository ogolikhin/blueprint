import * as angular from "angular";
import {Models, SearchServiceModels, TreeViewModels} from "../../models";
import {Helper} from "../../../shared/";

export abstract class SearchResultVM<T extends SearchServiceModels.ISearchResult> implements TreeViewModels.IViewModel<T> {
    public abstract readonly id: string;
    public abstract readonly iconClass: string;

    constructor(
        public model: T,
        private onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean) {
    }

    public selected(value?: boolean): boolean {
        return this.onSelect(this, value);
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
        onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean) {
        super(model, onSelect);
        this.id = `${this.model.prefix}${this.model.id}`;
        this.iconClass = `icon-${_.kebabCase(Models.ItemTypePredefined[this.model.predefinedType])}`;
    }
}
