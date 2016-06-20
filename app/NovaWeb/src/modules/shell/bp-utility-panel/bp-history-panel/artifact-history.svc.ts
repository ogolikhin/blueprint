﻿import { ILocalizationService } from "../../../core";
// import { BehaviorSubject } from "rx-angular";
// import * as Models from "../../../main/models/models";
// export {Models}

export interface IArtifactHistory {
    artifactHistory;
    getArtifactHistory(artifactId: number, limit?: number, offset?: number, userId?: string, asc?: boolean): ng.IPromise<IArtifactHistoryVersion[]>;
}

export interface IArtifactHistoryVersion {
    displayName: string;
    hasUserIcon: boolean;
    timestamp: string;
    userId: number;
    versionId: number;
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

    private _artifactHistory;
    public artifactHistory;

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
            this._artifactHistory = new Rx.BehaviorSubject([]);
            this.artifactHistory = this._artifactHistory.asObservable();
    }

    public getArtifactHistory(
        artifactId: number, 
        limit: number = 10, 
        offset: number = 0, 
        userId: string = null, 
        asc: boolean = false): ng.IPromise<IArtifactHistoryVersion[]> {

        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/ArtifactStore/artifacts/${artifactId}/version`, 
            method: "GET",
            params: {
                limit: limit,
                offset: offset,
                userId: userId,
                asc: asc
            }
        };

        this.$http(requestObj)
            .success((result: IArtifactHistoryResultSet) => {
                defer.resolve(result.artifactHistoryVersions);
                this._artifactHistory.onNext(result.artifactHistoryVersions);

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
