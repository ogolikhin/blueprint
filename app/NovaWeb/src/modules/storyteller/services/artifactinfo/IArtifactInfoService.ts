module Storyteller {
    export interface IArtifactInfoService {
        getArtifactInfo(artifactId: string, versionId?: number, revisionId?: number, baselineId?: number): ng.IPromise<IArtifactReference>;
    }
}
