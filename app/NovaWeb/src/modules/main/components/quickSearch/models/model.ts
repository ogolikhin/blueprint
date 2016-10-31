export interface ISearchResult {
    items: ISearchItem[];
    page: number;
    pageItemCount: number;
    pageSize: number;
}

export interface ISearchItem {
    artifactId: number;
    createdBy: string;
    createdDateTime: string;
    createdUser: number;
    isSubartifact: boolean;
    itemId: number;
    itemTypeId: number;
    lastModifiedBy: string;
    lastModifiedDateTime: string;
    lastModifiedUser: number;
    name: string;
    projectId: number;
    propertyName: string;
    proeprtyTypeId: number;
    searchableValue: string;
    subartifactId: number;
    typeName: string;
}