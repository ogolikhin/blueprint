module Shell {
    export class ArtifactUtilityService implements IArtifactUtilityService {
        private pendingPromises: { [id: string]: ng.IPromise<any> } = {};

        public static $inject = ["$http", "$q", "messageService"];
        constructor(private $http: ng.IHttpService, private $q: ng.IQService, private messageService: Shell.IMessageService) {
        }

        public getFiles(artifactId: number): ng.IPromise<IFilesInfo> {
            return this.getArtifactResources(artifactId, "files");
        }

        public getRelationships(artifactId: number): ng.IPromise<IRelationshipsInfo> {
            return this.getArtifactResources(artifactId, "relationships");
        }

        public getHistory(artifactId: number): ng.IPromise<IHistoryInfo> {
            return this.getArtifactResources(artifactId, "history");
        }

        private getArtifactResources(artifactId: number, resourceKey: string): ng.IPromise<any> {
            var promiseKey = resourceKey + String(artifactId);
            var promise = this.pendingPromises[promiseKey];
            if (!promise) {
                var deferred = this.$q.defer<any>();

                this.$http.get<any>("/svc/components/RapidReview/artifacts/" + artifactId + "/" + resourceKey)
                    .success((result) => {
                    delete this.pendingPromises[promiseKey];
                    deferred.resolve(result);
                }).error((data: IHttpError, status: number) => {
                    delete this.pendingPromises[promiseKey];
                    data.statusCode = status;
                    deferred.reject(data);
                });

                promise = this.pendingPromises[promiseKey] = deferred.promise;
            }

            return promise;
        }

        public getProperties(itemId: number, revisionId: number = null, includeEmptyProperties = false): ng.IPromise<IArtifactWithProperties> {
            var requestParameters = "";
            if (revisionId) {
                requestParameters = "?revisionId=" + revisionId;
                if (includeEmptyProperties) {
                    requestParameters += "&includeEmptyProperties=true";
                }
            } else if (includeEmptyProperties) {
                requestParameters = "?includeEmptyProperties=true";
            }
            var promiseKey = "properties" + String(itemId);
            var promise = this.pendingPromises[promiseKey];
            if (!promise) {
                var deferred = this.$q.defer<any>();
                this.$http.get<any>("/svc/components/RapidReview/items/" + itemId + "/properties" + requestParameters)
                    .success((result) => {
                    delete this.pendingPromises[promiseKey];
                    deferred.resolve(result);
                }).error((data: IHttpError, status: number) => {
                    delete this.pendingPromises[promiseKey];
                    data.statusCode = status;
                    deferred.reject(data);
                });

                promise = this.pendingPromises[promiseKey] = deferred.promise;
            }

            return promise;
        }

        public updateTextProperty(itemId: number, propertyValues: IArtifactProperty[]): ng.IPromise<any> {
            var promiseKey = "updateProperty" + String(itemId);
            var promise = this.pendingPromises[promiseKey];
            if (!promise) {
                var deferred = this.$q.defer<any>();
                this.$http.patch<any>("/svc/components/RapidReview/items/" + itemId + "/properties", angular.toJson(propertyValues))
                    .success((result, status: number) => {
                        delete this.pendingPromises[promiseKey];
                        for (var r of result.messages) {
                            var message = new Message(MessageType.Warning, r.message);
                            this.messageService.addMessage(message);
                        }
                        deferred.resolve(result);
                    }).error((data: IHttpError, status: number) => {
                        delete this.pendingPromises[promiseKey];
                        data.statusCode = status;
                        deferred.reject(data);
                    });

                promise = this.pendingPromises[promiseKey] = deferred.promise;
            }
            return promise;
        }

        public getFileContentUrl(artifactId: number, fileId: number): string {
            return "/svc/components/RapidReview/artifacts/" + artifactId + "/files/" + fileId;
        }
    }

    angular.module("Shell").service("artifactUtilityService", ArtifactUtilityService);
}
