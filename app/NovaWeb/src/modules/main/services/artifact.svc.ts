import * as Models from "../models/models";

import {ArtifactServiceMock} from "./artifact.svc.mock";

export interface IArtifactService {
    getArtifact(id: number): ng.IPromise<Models.IArtifact>;
}

export class ArtifactService implements IArtifactService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public getArtifact(artifactId: number): ng.IPromise<Models.IArtifact> {
        var defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/bpartifactstore/artifacts/${artifactId}`,
            //url: `/svc/components/nova/artifacts/${artifactId}`,
            method: "GET",
            //params: {
            //    types: true
            //}
        };

        this.$http(request)
            .success((result: Models.IArtifact) => {
                //fake response
                //let artifact = ArtifactServiceMock.createArtifact(artifactId, 5);
                //ArtifactServiceMock.createSystemProperty(artifact);


                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "")
                };
                defer.reject(error);
            });
        return defer.promise;
    }
}