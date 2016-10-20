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
