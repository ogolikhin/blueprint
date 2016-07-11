module Shell {
    export interface IArtifactUtilityService {
        getFiles(artifactId: number): ng.IPromise<IFilesInfo>;
        getFileContentUrl(artifactId: number, fileId: number): string;
        getRelationships(artifactId: number): ng.IPromise<IRelationshipsInfo>;
        getHistory(artifactId: number): ng.IPromise<IHistoryInfo>;
        getProperties(itemId: number, revisionId?: number, includeEmptyProperties?: boolean): ng.IPromise<IArtifactWithProperties>;
        updateTextProperty(itemId: number, propertyValues: IArtifactProperty[]): ng.IPromise<any>;
    }
}
