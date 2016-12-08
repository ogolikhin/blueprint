import {IFileUploadService, IFileResult} from "./fileUploadService";
import {ICopyImageResult} from "./models/models";

export class FileUploadServiceMock implements IFileUploadService {
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

        const deferred = this.$q.defer<IFileResult>();
        const result: IFileResult = {
            guid: "guid" + file.name,
            uriToFile: "uri-" + file.name
        };

        deferred.resolve(result);

        return deferred.promise;
    }
    public copyArtifactImagesToFilestore(artifactIds: number[],
                                         expirationDate?: Date): ng.IPromise<ICopyImageResult[]> {
        return this.$q.when(null);
    }
}
