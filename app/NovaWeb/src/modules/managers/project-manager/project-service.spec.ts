import * as angular from "angular";
import "angular-mocks";
import {Models, AdminStoreModels, SearchServiceModels} from "../../main/models";
import {HttpStatusCode} from "../../core/http";
import {IProjectService, ProjectService} from "./project-service";
import {ProjectServiceMock} from "./project-service.mock";

describe("Project Repository", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectService", ProjectService);
    }));

    describe("getFolders", () => {
        it("get one folder - success", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/1/children")
                .respond(HttpStatusCode.Success, <AdminStoreModels.IInstanceItem[]>[
                    {id: 3, name: "Imported Projects", type: 0, description: "", parentFolderId: 1, hasChildren: false}
                ]);

            // Act
            let error: any;
            let data: AdminStoreModels.IInstanceItem[];
            projectService.getFolders().then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(data).toEqual(jasmine.any(Array), "incorrect type");
            expect(data.length).toBe(1, "incorrect data returned");
            expect(data[0].id).toBe(3, "incorrect id returned");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get one folder unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/5/children")
                .respond(HttpStatusCode.Unauthorized);

            // Act
            let error: any;
            let data: AdminStoreModels.IInstanceItem[];
            projectService.getFolders(5).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(401);
            expect(error.message).toEqual("Folder_NotFound");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("getProjects", () => {
        it("get one project", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET(`svc/adminstore/instance/projects/10`)
                .respond(HttpStatusCode.Success, [
                    {
                        id: 10, name: "Project 10", typeId: 0, hasChildren: true, description: "Description"
                    }
                ]);

            // Act
            let error: any;
            let data: AdminStoreModels.IInstanceItem;
            projectService.getProject(10).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data[0].id).toEqual(10);
            expect(data[0].name).toEqual("Project 10");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));


        it("get project - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/projects/10")
                .respond(HttpStatusCode.Unauthorized);

            // Act
            let error: any;
            let data: AdminStoreModels.IInstanceItem;
            projectService.getProject(10).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(401);
            expect(error.message).toEqual("Project_NotFound");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("getArtifacts", () => {
        it("get one project", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET(`svc/artifactstore/projects/10/children`)
                .respond(HttpStatusCode.Success, [
                    {
                        id: 10, name: "Project 10", typeId: 0, hasChildren: true,
                        artifacts: [
                            {
                                id: 11,
                                name: "Artifact 11",
                                typeId: 10,
                                projectId: 10,
                                predefinedType: 100,
                                parentId: 10,
                                prefix: "AT",
                                hasChildren: false
                            }
                        ]
                    }
                ]);

            // Act
            let error: any;
            let data: Models.IArtifact[];
            projectService.getArtifacts(10).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data.length).toEqual(1);
            expect(data[0].id).toEqual(10);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get project children", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/10/artifacts/111/children")
                .respond(HttpStatusCode.Success, [
                        {
                            id: 13,
                            name: "Artifact 13",
                            typeId: 14,
                            projectId: 10,
                            predefinedType: 100,
                            parentId: 10,
                            prefix: "AT",
                            hasChildren: false
                        },
                        {
                            id: 14,
                            name: "Artifact 14",
                            typeId: 14,
                            projectId: 10,
                            predefinedType: 100,
                            parentId: 10,
                            prefix: "AT",
                            hasChildren: true
                        }
                    ]

                );

            // Act
            let error: any;
            let data: Models.IArtifact[];
            projectService.getArtifacts(10, 111).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data.length).toEqual(2);
            expect(data.length).toEqual(2);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get project children - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/10/artifacts/111/children")
                .respond(HttpStatusCode.Unauthorized);

            // Act
            let error: any;
            let data: Models.IArtifact[];
            projectService.getArtifacts(10, 111).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(401);
            expect(error.message).toEqual("Artifact_NotFound");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("getProjectMeta", () => {
        it("get - successful", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET(`svc/artifactstore/projects/10/meta/customtypes`)
                .respond(HttpStatusCode.Success, ProjectServiceMock.populateMetaData());

            // Act
            let error: any;
            let data: Models.IProjectMeta;
            projectService.getProjectMeta(10).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toBeDefined();
            expect(data.propertyTypes).toEqual(jasmine.any(Array));
            expect(data.artifactTypes).toEqual(jasmine.any(Array));
            expect(data.subArtifactTypes).toEqual(jasmine.any(Array));
            expect(data.propertyTypes.length).toEqual(3);
            expect(data.artifactTypes.length).toEqual(3);
            expect(data.artifactTypes.length).toEqual(3);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET(`svc/artifactstore/projects/10/meta/customtypes`)
                .respond(HttpStatusCode.Unauthorized);

            // Act
            let error: any;
            let data: Models.IProjectMeta;
            projectService.getProjectMeta(10).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(401);
            expect(error.message).toEqual("Project_NotFound");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("searchProjects", () => {
        it("post - successful", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            const searchCriteria: SearchServiceModels.ISearchCriteria = {query: "new"};
            const searchResult: SearchServiceModels.IProjectSearchResultSet = {items: [
                {itemId: 1, name: "New project 1", path: "Blueprint"},
                {itemId: 2, name: "New project 2", path: "Blueprint"}
            ]};
            $httpBackend.expectPOST("/svc/searchservice/projectsearch/name?resultCount=100&separatorString=+%3E+", searchCriteria)
                .respond(HttpStatusCode.Success, searchResult);

            // Act
            let data: SearchServiceModels.IProjectSearchResultSet;
            let error: any;
            projectService.searchProjects(searchCriteria).then(response => data = response, err => error = err);

            // Assert
            $httpBackend.flush();
            expect(error).toBeUndefined();
            expect(data).toEqual(searchResult);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("post - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            const searchCriteria: SearchServiceModels.ISearchCriteria = {query: "new"};
            $httpBackend.expectPOST("/svc/searchservice/projectsearch/name?resultCount=100&separatorString=+%3E+", searchCriteria)
                .respond(HttpStatusCode.Unauthorized);

            // Act
            let data: SearchServiceModels.IProjectSearchResultSet;
            let error: any;
            projectService.searchProjects(searchCriteria).then(response => data = response, err => error = err);

            // Assert
            $httpBackend.flush();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(401);
            expect(error.message).toEqual("");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("searchItemNames", () => {
        it("post - successful", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            const searchCriteria: SearchServiceModels.IItemNameSearchCriteria = {query: "new", projectIds: [1]};
            const searchResult: SearchServiceModels.IItemNameSearchResultSet = {
                items: [
                    {id: 2, itemId: 2, name: "New Actor 1", path: ""},
                    {id: 2, itemId: 3, name: "New Actor 2", path: ""}
                ],
                pageItemCount: 2
            };
            $httpBackend.expectPOST("/svc/searchservice/itemsearch/name?pageSize=100&startOffset=0", searchCriteria)
                .respond(HttpStatusCode.Success, searchResult);

            // Act
            let data: SearchServiceModels.IItemNameSearchResultSet;
            let error: any;
            projectService.searchItemNames(searchCriteria).then(response => data = response, err => error = err);

            // Assert
            $httpBackend.flush();
            expect(error).toBeUndefined();
            expect(data).toEqual(searchResult);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("post - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            const searchCriteria: SearchServiceModels.IItemNameSearchCriteria = {query: "new", projectIds: [1]};
            $httpBackend.expectPOST("/svc/searchservice/itemsearch/name?pageSize=100&startOffset=0", searchCriteria)
                .respond(HttpStatusCode.Unauthorized);

            // Act
            let data: SearchServiceModels.IItemNameSearchResultSet;
            let error: any;
            projectService.searchItemNames(searchCriteria).then(response => data = response, err => error = err);

            // Assert
            $httpBackend.flush();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(401);
            expect(error.message).toEqual("");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });
});
