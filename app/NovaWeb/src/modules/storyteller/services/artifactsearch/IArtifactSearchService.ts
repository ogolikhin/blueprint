module Shell {
    export interface IArtifactSearchService {
        search(artifactName: string, projectId?: string): ng.IPromise<IArtifactSearchResultItem[]>;
    }
}
