import { ILocalizationService } from "../../core";
import * as Models from "../models/models";

export {Models}

export interface IProjectRepository {
    getFolders(id?: number): ng.IPromise<any[]>;
    getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]>;
    getArtifactDetails(artifactId: number): ng.IPromise<Models.IArtifact>;
    getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta>;
}

export class ProjectRepository implements IProjectRepository {
    static $inject: [string] = ["$q", "$http", "$log", "localization"];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }

    public getFolders(id?: number): ng.IPromise<Models.IProjectNode[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/instance/folders/${id || 1}/children`)
            .success((result: Models.IProjectNode[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Folder_NotFound")
                };
                defer.reject(error);
            });
        return defer.promise;
    } 
    public getProject(id?: number): ng.IPromise<Models.IProjectNode[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/instance/projects/${id}`)
            .success((result: Models.IProjectNode[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Project_NotFound")
                };
                defer.reject(error);
            });
        return defer.promise;
    } 

    public getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {
        var defer = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : ``) + `/children`;

        this.$http.get<any>(url)
            .success((result: Models.IArtifact[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                defer.reject(error);
            });
        return defer.promise;
    }

    public getArtifactDetails(artifactId: number): ng.IPromise<Models.IArtifact> {
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
                let details: Models.IArtifact = {
                    "id": 56318,
                    "name": "Use Case Diagram (100004785 Enterprise Order Capture)",
                    "parentId": 56316,
                    "itemTypeId": 11995,
                    "itemTypeVersionId": null,
                    "projectId": 56313,
                    "orderIndex": 18,
                    "version": 1,
                    "permissions": 0,
                    "lockedByUserId": null,
                    "propertyValues": [
                        {
                            "propertyTypeId": 12093,
                            "propertyTypeVersionId": null,
                            "propertyTypePredefined": 4099,
                            "value": ""
                        }
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
                        {
                            "isDeleted": null,
                            "id": 62350,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62351,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62352,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62353,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62354,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62355,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62356,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62357,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62358,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62359,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62360,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62361,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62362,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62363,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62364,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62365,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62366,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62367,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62368,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        },
                        {
                            "isDeleted": null,
                            "id": 62369,
                            "name": "Boundary",
                            "parentId": 56318,
                            "itemTypeId": 11941,
                            "itemTypeVersionId": null,
                            "propertyValues": [],
                            "traces": []
                        }
                    ],
                    "traces": []
                }


                defer.resolve(details);
            }).error((err: any, statusCode: number) => {
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Artifact_NotFound")
                };
                defer.reject(error);
            });
        return defer.promise;
    }

    public getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta> {
        var defer = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}/meta/customtypes`;

        this.$http.get<any>(url)
            .success((result: Models.IProjectMeta) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Project_NotFound")
                };
                defer.reject(error);
            });
        return defer.promise;
    }


}

