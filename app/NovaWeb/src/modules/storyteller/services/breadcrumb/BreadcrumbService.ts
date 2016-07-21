module Shell {
    
    /**
     * Breadcrumb Service:
     * 
     * get breadcrumb navigation path from the Web API  
     * 
     */
    export class BreadcrumbService implements IBreadcrumbService {
        public static $inject = [
            "$http",
            "$q",
            "$rootScope",
            "messageService",
            "busyIndicatorService"
        ];

        constructor(private $http: ng.IHttpService,
            private $q: ng.IQService,
            private $rootScope: ng.IRootScopeService,
            private messageService: Shell.IMessageService,
            private busyIndicatorService: Shell.IBusyIndicatorService) {

        }
        public artifactPathLinks: IArtifactReference[];

        public getNavigationPath(processIds: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<IArtifactReference[]> {
            var restPath = this.navigationRestPath(processIds);
            var deferred = this.$q.defer<IArtifactReference[]>();

            let queryParamData = {
                versionId: isNaN(versionId) ? null : versionId,
                revisionId: isNaN(revisionId) ? null : revisionId,
                baselineId: isNaN(baselineId) ? null : baselineId,
                readOnly: !readOnly ? null : true //Do not send ?readOnly=false query parameter for normal calls
            };

            //Create parameters
            var requestConfig = {
                params: queryParamData
            };

            this.$http.get<IArtifactReference[]>(restPath, requestConfig).success((result: IArtifactReference[]) => {
                this.artifactPathLinks = result;
                deferred.resolve(result);

            }).error((err: Shell.IHttpError, status: number) => {
                err.statusCode = status;
                this.artifactPathLinks = [];
                deferred.reject(err);
            });

            return deferred.promise;
        }
       
        private navigationRestPath(processId: string): string {
            var restPath = "/svc/shared/navigation/" + processId;
            return restPath;
        }

    }

    var app = angular.module("Shell");
    app.service("breadcrumbService", BreadcrumbService);
}
