module Storyteller {
    export class FileUploadService implements IFileUploadService {
        public static $inject = ["$http", "$q", "$rootScope", "messageService", "$log"];

        constructor(private $http: ng.IHttpService,
            private $q: ng.IQService,
            private $rootScope,
            private messageService: Shell.IMessageService,
            private $log: ng.ILogService) {
        }

        private fileStorePath = "/svc/components/filestore/files/";

        public uploadToFileStore(file: any, expirationDate: Date = null): ng.IPromise<IFileResult> {

            this.messageService.clearMessages();

            var restPath = this.fileStorePath + file.name +  "/";
            if (!!expirationDate) {
                restPath += "?expired="+expirationDate.toISOString();
            }

            var deferred = this.$q.defer<IFileResult>();

            // By setting "ignoreInterceptor: true" we do not log the error caused by the logger server error, so we prevent a infinite loop situation caused by this.
            this.$http.post(restPath, file, <Shell.IHttpInterceptorConfig>{ ignoreInterceptor: true }).success((result: IFileResult) => {
                
                deferred.resolve(result);

            }).error((err: Shell.IHttpError, status: number) => {

                if (this.$log && err.message) {
                    this.$log.error(err.message);
                }

                // Use generic message if FileStore is not available or returns error.
                var errorMessage = this.$rootScope.config.labels["ST_File_Store_Error"];
                this.messageService.addError(errorMessage);

                err.statusCode = status;
                deferred.reject(err);
            });

            return deferred.promise;
        }
    }

    var app = angular.module("Storyteller");
    app.service("fileUploadService", FileUploadService);
}
