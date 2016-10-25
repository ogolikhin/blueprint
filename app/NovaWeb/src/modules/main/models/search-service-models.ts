export interface ISearchCriteria {
    query: string;
}

export interface ISearchResultSet<T extends ISearchResult> {
    items: T[];
}

export interface ISearchResult {
    itemId: number;
    name: string;
    path?: string;
}

export interface IProjectSearchResultSet extends ISearchResultSet<ISearchResult> {
}

export interface IItemSearchCriteria extends ISearchCriteria {
    projectIds: number[];
    ItemTypeIds?: number[];
    includeArtifactPath?: boolean;
}

export interface IItemNameSearchResultSet extends ISearchResultSet<IItemSearchResult> {
    pageItemCount: number;
}

export interface IItemSearchResult extends ISearchResult {
    projectId: number;
    artifactId: number;
    itemTypeId: number;
    typeName: string;
    typePrefix: string;
}
