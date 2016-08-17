export interface IFileUploadService {
    uploadToFileStore(file: any, expirationDate?: Date): ng.IPromise<IFileResult>;
}

export interface IFileResult {
    guid: string;
    uriToFile: string;
}

export class FileUploadService implements IFileUploadService {
    public static $inject = [
        "$q",
        "$http",
        "$log"
    ];
    
    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService) {
    }

    public uploadToFileStore(file: File, expirationDate?: Date): ng.IPromise<IFileResult> {
        var deferred = this.$q.defer<IFileResult>();
        const request: ng.IRequestConfig = {
            url: `/svc/components/filestore/files/${file.name}`,
            method: "POST",
            params: expirationDate ? { expired: expirationDate.toISOString() } : undefined,
            data: file
        };

        this.$http(request).then((result: ng.IHttpPromiseCallbackArg<IFileResult>) => {
            deferred.resolve(result.data);
        }, (result: ng.IHttpPromiseCallbackArg<any>) => {
            const error = {
                statusCode: result.status,
                message: result.data ? result.data.message : ""
            };
            this.$log.error(error);
            deferred.reject(error);
        });

        return deferred.promise;
    }
}
