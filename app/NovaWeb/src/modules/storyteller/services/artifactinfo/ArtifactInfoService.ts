module Storyteller {
    
    /**
     * Artifact Info Service:
     * 
     */
    export class ArtifactInfoService implements IArtifactInfoService {
        public static $inject = ["$http", "$q"];

        constructor(private $http: ng.IHttpService,
            private $q: ng.IQService) { }

        public getArtifactInfo(artifactId: string, versionId: number = null, revisionId: number = null, baselineId: number = null): ng.IPromise<IArtifactReference> {

            const restPath = this.getRestPath(artifactId);
            const deferred = this.$q.defer<IArtifactReference>();

            let queryParamData = {
                versionId: isNaN(versionId) ? null : versionId,
                revisionId: isNaN(revisionId) ? null : revisionId,
                baselineId: isNaN(baselineId) ? null : baselineId
            };

            //Create parameters
            var requestConfig = {
                params: queryParamData
            };

            this.$http.get<IArtifactReference>(restPath, requestConfig).success((result: IArtifactReference) => {
                deferred.resolve(result);

            }).error((err: Shell.IHttpError, status: number) => {

                err.statusCode = status;
                deferred.reject(err);
            });

            return deferred.promise;
        }

        private getRestPath(artifactId: string): string {
            return  "/svc/components/storyteller/artifactInfo/" + artifactId;
        }
    }

    angular.module("Storyteller").service("artifactInfoService", ArtifactInfoService);
}
