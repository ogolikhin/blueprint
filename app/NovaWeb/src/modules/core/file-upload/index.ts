export interface IFileUploadService {
    uploadToFileStore(
        file: any, 
        expirationDate?: Date, 
        progress?: (ev: ProgressEvent) => any,
        cancelPromise?: ng.IPromise<any>): ng.IPromise<IFileResult>;
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

    private encodeRFC5987ValueChars (str) {
        return encodeURIComponent(str).
            // Note that although RFC3986 reserves "!", RFC5987 does not,
            // so we do not need to escape it
            replace(/['()]/g, window["escape"]). // i.e., %27 %28 %29
            replace(/\*/g, "%2A").
                // The following are not required for percent-encoding per RFC5987, 
                // so we can allow for a little better readability over the wire: |`^
                replace(/%(?:7C|60|5E)/g, window["unescape"]);
    }

    public uploadToFileStore(
        file: File,
        expirationDate?: Date, 
        progress?: (ev: ProgressEvent) => any, 
        cancelPromise?: ng.IPromise<any>): ng.IPromise<IFileResult> {

        const filename: string = this.encodeRFC5987ValueChars(file.name);
        const deferred = this.$q.defer<IFileResult>();
        const request: ng.IRequestConfig | any = {
            headers: {
                FileName: filename
            },
            method: "POST",
            url: `/svc/bpfilestore/files/`,
            params: expirationDate ? { expired: expirationDate.toISOString() } : undefined,
            data: file,
            uploadEventHandlers: progress ? { progress: progress } : undefined,
            timeout: cancelPromise
        };

        this.$http(request).then((result: ng.IHttpPromiseCallbackArg<IFileResult>) => {
            deferred.resolve(result.data);
        }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
            if (!errResult) {
                deferred.reject();
                return;
            }
            const error = {
                statusCode: errResult.status,
                message: errResult.data ? errResult.data.message : ""
            };
            deferred.reject(error);
        });

        return deferred.promise;
    }
}
