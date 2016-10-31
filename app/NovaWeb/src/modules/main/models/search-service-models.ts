import {ItemTypePredefined} from "./enums";
import {IArtifact} from "./models";

export interface ISearchCriteria {
    query: string;
}

export interface ISearchResultSet<T extends ISearchResult> {
    items: T[];
}

export interface ISearchResult {
    itemId: number;
    name?: string;
    path?: string;
}

export interface IProjectSearchResultSet extends ISearchResultSet<ISearchResult> {
}

export interface IItemNameSearchCriteria extends ISearchCriteria {
    projectIds: number[];
    predefinedTypeIds?: ItemTypePredefined[];
    includeArtifactPath?: boolean;
}

export interface IItemNameSearchResultSet extends ISearchResultSet<IItemNameSearchResult> {
    pageItemCount: number;
}

export interface IItemNameSearchResult extends ISearchResult, IArtifact {
}
