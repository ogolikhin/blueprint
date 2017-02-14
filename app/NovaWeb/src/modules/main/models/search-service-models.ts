import {ItemTypePredefined} from "./itemTypePredefined.enum";
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
    itemTypeIcon?: number;
}

export interface IProjectSearchResultSet extends ISearchResultSet<IProjectSearchResult> {
}

export interface IProjectSearchResult extends ISearchResult {
    path?: string;
    description?: string;
}

export interface IItemNameSearchCriteria extends ISearchCriteria {
    projectIds: number[];
    predefinedTypeIds?: ItemTypePredefined[];
    itemTypeIds?: number[];
    includeArtifactPath?: boolean;
}

export interface IItemNameSearchResultSet extends ISearchResultSet<IItemNameSearchResult> {
    pageItemCount: number;
}

export interface IItemNameSearchResult extends ISearchResult, IArtifact {
    path?: string[];
}
