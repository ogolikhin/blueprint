import {ILocalizationService} from "../../../core";
import {Models} from "../../../main";

export interface IArtifactHistory {
    artifactHistory: ng.IPromise<IArtifactHistoryVersion[]>;
    getArtifactHistory(artifactId: number,
                       limit?: number,
                       offset?: number,
                       userId?: string,
                       asc?: boolean,
                       timeout?: ng.IPromise<void>): ng.IPromise<IArtifactHistoryVersion[]>;
}

export interface IArtifactHistoryVersion {
    displayName: string;
    hasUserIcon: boolean;
    timestamp: string;
    userId: number;
    versionId: number;
    artifactState: Models.ArtifactStateEnum;
}

export interface IArtifactHistoryResultSet {
    artifactHistoryVersions: IArtifactHistoryVersion[];
    artifactId: number;
}

export class ArtifactHistory implements IArtifactHistory {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    public artifactHistory: ng.IPromise<IArtifactHistoryVersion[]>;

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService,
                private localization: ILocalizationService) {
    }

    public getArtifactHistory(artifactId: number,
                              limit: number = 10,
                              offset: number = 0,
                              userId: string = null,
                              asc: boolean = false,
                              timeout: ng.IPromise<void>): ng.IPromise<IArtifactHistoryVersion[]> {

        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/version`,
            method: "GET",
            params: {
                limit: limit,
                offset: offset,
                userId: userId,
                asc: asc
            },
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<IArtifactHistoryResultSet>) => {
                defer.resolve(result.data.artifactHistoryVersions);

            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
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
