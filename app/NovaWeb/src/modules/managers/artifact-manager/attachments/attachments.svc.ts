import {ILocalizationService} from "../../../core";

export interface IArtifactAttachmentsService {
    getArtifactAttachments(artifactId: number,
                           subArtifactId?: number,
                           addDrafts?: boolean,
                           timeout?: ng.IPromise<void>): ng.IPromise<IArtifactAttachmentsResultSet>;
}

export interface IArtifactAttachment {
    userId: number;
    userName: string;
    fileName: string;
    fileType: string;
    attachmentId: number;
    uploadedDate: string;
    guid?: string;
    changeType?: number;
}

export interface IArtifactDocRef {
    artifactName: string;
    artifactId: number;
    userId: number;
    userName: string;
    itemTypePrefix: string;
    referencedDate: string;
    changeType?: number;
}

export interface IArtifactAttachmentsResultSet {
    artifactId: number;
    subartifactId: number;
    attachments: IArtifactAttachment[];
    documentReferences: IArtifactDocRef[];
}

export class ArtifactAttachmentsService implements IArtifactAttachmentsService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    public artifactAttachments: ng.IPromise<IArtifactAttachmentsResultSet>;
    // public artifactDocRefs: ng.IPromise<IArtifactDocRef[]>;

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService,
                private localization: ILocalizationService) {
    }

    public getArtifactAttachments(artifactId: number,
                                  subArtifactId: number = null,
                                  addDrafts: boolean = true,
                                  timeout?: ng.IPromise<void>): ng.IPromise<IArtifactAttachmentsResultSet> {

        const defer = this.$q.defer<IArtifactAttachmentsResultSet>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/attachment`,
            method: "GET",
            params: {
                subartifactId: subArtifactId,
                addDrafts: addDrafts
            },
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<IArtifactAttachmentsResultSet>) => {
                // console.log("retrieved attachments: " + JSON.stringify(result));
                defer.resolve(result.data);

            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
                defer.reject(error);
            });

        return defer.promise;
    }
}
