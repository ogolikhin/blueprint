module Shell {
    export class ArtifactSearchService implements IArtifactSearchService {
        public static $inject = ["$http", "$q"];

        constructor(private $http: ng.IHttpService, private $q: ng.IQService) { }

        public search(artifactName: string, projectId?: string): ng.IPromise<IArtifactSearchResultItem[]> {
            const deferred = this.$q.defer<IArtifactSearchResultItem[]>();

            this.$http.get<IArtifactSearchResultItem[]>("/svc/shared/artifacts/search/", {
                params: {
                    name: artifactName,
                    projectId: projectId,
                    showBusyIndicator: false
                }
            }).success((result) => {
                deferred.resolve(result);
            }).error((data: IHttpError, status: number) => {
                data.statusCode = status;
                deferred.reject(data);
            });

            return deferred.promise;
        }
    }

    angular.module("Shell").service("artifactSearchService", ArtifactSearchService);
}
