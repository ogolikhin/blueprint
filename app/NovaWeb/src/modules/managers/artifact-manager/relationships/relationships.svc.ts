import {Relationships} from "../../../main";
import {ILocalizationService} from "../../../core/localization/localizationService";

export interface IArtifactRelationshipsService {
    getRelationships(artifactId: number,
                     subArtifactId?: number,
                     versionId?: number,
                     timeout?: ng.IPromise<void>): ng.IPromise<Relationships.IArtifactRelationshipsResultSet>;
}

export class ArtifactRelationshipsService implements IArtifactRelationshipsService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService,
                private localization: ILocalizationService) {
    }

    public getRelationships(artifactId: number,
                            subArtifactId?: number,
                            versionId?: number,
                            timeout?: ng.IPromise<void>): ng.IPromise<Relationships.IArtifactRelationshipsResultSet> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationships`,
            method: "GET",
            params: {
                subartifactId: subArtifactId,
                versionId: versionId
            }
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Relationships.IArtifactRelationshipsResultSet>) => {
                defer.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }
}
