import { ILocalizationService } from "../../../core";
// import { Models } from "../../../main";

export interface IArtifactAttachments {
    getArtifactAttachments(artifactId: number, subArtifactId?: number, addDrafts?: boolean): ng.IPromise<IArtifactAttachmentsResultSet>;
}

export interface IArtifactAttachment {
    userId: number;
    userName: string;
    fileName: string;
    attachmentId: number;
    uploadDate: string;
}

export interface IArtifactDocRef {
    artifactName: string;
    artifactId: number;
    userId: number;
    userName: string;
    referencedDate: string;
}

export interface IArtifactAttachmentsResultSet {
    artifactId: number;
    subartifactId: number;
    attachments: IArtifactAttachment[];
    documentReferences: IArtifactDocRef[];
}

export class ArtifactAttachments implements IArtifactAttachments {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    public artifactAttachments: ng.IPromise<IArtifactAttachmentsResultSet>;
    // public artifactDocRefs: ng.IPromise<IArtifactDocRef[]>;

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }

    public getArtifactAttachments(
        artifactId: number, 
        subArtifactId: number = null, 
        addDrafts: boolean = true): ng.IPromise<IArtifactAttachmentsResultSet> {

        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/attachment`, 
            method: "GET",
            params: {
                subartifactId: subArtifactId,
                addDrafts: addDrafts
            }
        };

        this.$http(requestObj)
            .success((result: IArtifactAttachmentsResultSet) => {
                // console.log("retrieved attachments: " + JSON.stringify(result));
                defer.resolve(result);

            }).error((err: any, statusCode: number) => {
                const error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });
            
        return defer.promise;
    }
}
