export interface ICopyImageResult {
    originalId: number;
    newImageId: string;
    newImageUrl: string;
}


export interface IFileUploadService {
    uploadToFileStore(file: any,
                      expirationDate?: Date,
                      progress?: (ev: ProgressEvent) => any,
                      cancelPromise?: ng.IPromise<any>): ng.IPromise<IFileResult>;
    uploadImageToFileStore(file: any,
                      progress?: (ev: ProgressEvent) => any,
                      cancelPromise?: ng.IPromise<any>): ng.IPromise<IFileResult>;
    copyArtifactImagesToFilestore(
                      artifactIds: number[],
                      expirationDate?: Date): ng.IPromise<ICopyImageResult[]>;
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

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService) {
    }

    public uploadToFileStore(file: File,
                             expirationDate?: Date,
                             progress?: (ev: ProgressEvent) => any,
                             cancelPromise?: ng.IPromise<any>): ng.IPromise<IFileResult> {

        const filename: string = encodeURIComponent(file.name);
        const deferred = this.$q.defer<IFileResult>();
        const request: ng.IRequestConfig | any = {
            headers: {
                FileName: filename
            },
            method: "POST",
            url: `/svc/bpfilestore/files/`,
            params: expirationDate ? {expired: expirationDate.toISOString()} : undefined,
            data: file,
            uploadEventHandlers: progress ? {progress: progress} : undefined,
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

    public uploadImageToFileStore(file: File,
                             progress?: (ev: ProgressEvent) => any,
                             cancelPromise?: ng.IPromise<any>): ng.IPromise<IFileResult> {

        const filename: string = encodeURIComponent(file.name);
        const request: ng.IRequestConfig | any = {
            headers: {
                FileName: filename
            },
            method: "POST",
            url: "/svc/bpartifactstore/images/",
            data: file,
            uploadEventHandlers: progress ? {progress: progress} : undefined,
            timeout: cancelPromise
        };

        return this.$http(request)
            .then((result: ng.IHttpPromiseCallbackArg<IFileResult>) => result.data)
            .catch((errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    return this.$q.reject();

                } else {
                    const error = {
                        statusCode: errResult.status,
                        message: errResult.data ? errResult.data.message : "",
                        errorCode: errResult.data.errorCode
                    };
                    return this.$q.reject(error);
                }
            });
    }

    public copyArtifactImagesToFilestore(artifactIds: number[], expirationDate?: Date): ng.IPromise<ICopyImageResult[]> {
        const deferred = this.$q.defer<any>();
        const request: ng.IRequestConfig | any = {
            method: "POST",
            url: `/svc/components/filestore/imagecopy/tofilestore`,
            params: expirationDate ? {expired: expirationDate.toISOString()} : undefined,
            data: artifactIds
        };
        this.$http(request).then((result) => {
            // success
            deferred.resolve(result.data);

        }).catch((err) => {
            deferred.reject(err.data);
        });

        return deferred.promise;
    }
}
