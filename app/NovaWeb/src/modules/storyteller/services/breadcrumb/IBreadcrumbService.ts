module Shell {
    export interface IBreadcrumbService {
        getNavigationPath(processIds: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<any>;
        artifactPathLinks: IArtifactReference[];
    }
}
