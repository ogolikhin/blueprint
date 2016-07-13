import { Models } from "../../../";

export interface IArtifactService {
    getArtifact(id: number): ng.IPromise<Models.IArtifact>;
}

export class ArtifactService implements IArtifactService {

    private promises: { [id: string]: ng.IPromise<any> } = {};

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public getArtifact(artifactId: number): ng.IPromise<Models.IArtifact> {
        var defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}`,
            //url: `/svc/components/nova/artifacts/${artifactId}`,
            method: "GET",
            //params: {
            //    types: true
            //}
        };

        this.$http(request)
            .success((result: Models.IArtifact[]) => {
                //fake response
                let details ={
                    "orderIndex": 1,
                    "version": 1,
                    "permissions": 0,
                    "lockedByUserId": null,
                    "propertyValues": [
                        {
                            "propertyTypeId": 12093,
                            "propertyTypeVersionId": null,
                            "propertyTypePredefined": Models.PropertyTypePredefined.Description,
                            "value": "Description 1"
                        },
                        {
                            "propertyTypeId": 12094,
                            "propertyTypeVersionId": null,
                            "propertyTypePredefined": Models.PropertyTypePredefined.CreatedBy,
                            "value": ""
                        },
                        {
                            "propertyTypeId": 12095,
                            "propertyTypeVersionId": null,
                            "propertyTypePredefined": Models.PropertyTypePredefined.CreatedOn,
                            "value": "2016/7/7"
                        },
                        {
                            "propertyTypeId": 12094,
                            "propertyTypeVersionId": null,
                            "propertyTypePredefined": Models.PropertyTypePredefined.LastEditedBy,
                            "value": ""
                        },
                        {
                            "propertyTypeId": 12095,
                            "propertyTypeVersionId": null,
                            "propertyTypePredefined": Models.PropertyTypePredefined.LastEditedOn,
                            "value": "2016/7/7"
                        },
                    ],
                    "subArtifacts": [
                        {
                            "isDeleted": null,
                            "id": 62345,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62346,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62347,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62348,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62349,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },

                    ],
                    "traces": []
                }


                defer.resolve(details);
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