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
    propertyTypeId: number;
    searchableValue: string;
    subartifactId: number;
    typeName: string;

    typePrefix: string;
    iconImageId: number;
    predefinedType: number;
    artifactClass: string;
}

export interface ISearchMetadata {
    totalCount: number;
    totalPages: number;
    pageSize: number;
    items: any[];
}