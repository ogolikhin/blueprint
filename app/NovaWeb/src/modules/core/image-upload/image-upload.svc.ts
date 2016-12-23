export interface IImageUploadService {
    uploadToFileStore(file: any,
                      progress?: (ev: ProgressEvent) => any,
                      cancelPromise?: ng.IPromise<any>): ng.IPromise<IImageResult>;
}

export interface IImageResult {
    guid: string;
    uriToFile: string;
}

export class ImageUploadService implements IImageUploadService {
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
                             progress?: (ev: ProgressEvent) => any,
                             cancelPromise?: ng.IPromise<any>): ng.IPromise<IImageResult> {

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
            .then((result: ng.IHttpPromiseCallbackArg<IImageResult>) => result.data)
            .catch((errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    return this.$q.reject();

                } else {
                    const error = {
                        statusCode: errResult.status,
                        message: errResult.data ? errResult.data.message : ""
                    };
                    return this.$q.reject(error);
                }
            });
    }
}
