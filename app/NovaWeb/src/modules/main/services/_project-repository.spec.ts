import "angular";
import "angular-mocks";
import { Models } from "../../main/";
import { IProjectRepository, ProjectRepository } from "./";
import { ProjectRepositoryMock } from "./project-repository.mock";

describe("Project Repository", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectRepository", ProjectRepository);
    }));
    
    describe("getFolders", () => {

        it("get one folder - success", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
                // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/1/children")
                .respond(200, <Models.IProjectNode[]>[
                    { id: 3, name: "Imported Projects", type: 0, description: "", parentFolderId: 1, hasChildren: false }
                    ]);

                // Act
            var error: any;
            var data: Models.IProjectNode[];
            projectRepository.getFolders().then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(data).toEqual(jasmine.any(Array), "incorrect type");
            expect(data.length).toBe(1, "incorrect data returned");
            expect(data[0].id).toBe(3, "incorrect id returned");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
            }));

        it("get one folder unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/5/children")
                .respond(401);
                
            // Act
            var error: any;
            var data: Models.IProjectNode[];
            projectRepository.getFolders(5).then((responce) => { data = responce; }, (err) => error = err);
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

        it("get one project", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET(`svc/adminstore/instance/projects/10`)
                .respond(200, [
                    {
                        id: 10, name: "Project 10", typeId: 0, hasChildren: true, description: "Description"
                    }
                ]);

            // Act
            var error: any;
            var data: Models.IProjectNode;
            projectRepository.getProject(10).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data[0].id).toEqual(10);
            expect(data[0].name).toEqual("Project 10");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));


        it("get project - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/projects/10")
                .respond(401);

            // Act
            var error: any;
            var data: Models.IProjectNode;
            projectRepository.getProject(10).then((responce) => { data = responce; }, (err) => error = err);
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

        it("get one project", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET(`svc/artifactstore/projects/10/children`)
                .respond(200, [
                    {
                        id: 10, name: "Project 10", typeId: 0, hasChildren: true,
                        artifacts: [
                            {
                                id: 11, name: "Artifact 11", typeId: 10, projectId: 10, predefinedType: 100, parentId: 10, prefix: "AT", hasChildren: false,
                            }
                        ]
                    }
                ]);

            // Act
            var error: any;
            var data: Models.IArtifact[];
            projectRepository.getArtifacts(10).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data.length).toEqual(1);
            expect(data[0].id).toEqual(10);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get project children", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/10/artifacts/111/children")
                .respond(200, [
                    {
                        id: 13, name: "Artifact 13", typeId: 14, projectId: 10, predefinedType: 100, parentId: 10, prefix: "AT", hasChildren: false,
                    },
                    {
                        id: 14, name: "Artifact 14", typeId: 14, projectId: 10, predefinedType: 100, parentId: 10, prefix: "AT", hasChildren: true,
                    }
                ]

                );

            // Act
            var error: any;
            var data: Models.IArtifact[];
            projectRepository.getArtifacts(10, 111).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data.length).toEqual(2);
            expect(data.length).toEqual(2);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get project children - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/10/artifacts/111/children")
                .respond(401);

            // Act
            var error: any;
            var data: Models.IArtifact[];
            projectRepository.getArtifacts(10, 111).then((responce) => { data = responce; }, (err) => error = err);
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

        it("get - successful", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET(`svc/artifactstore/projects/10/meta/customtypes`)
                .respond(200, ProjectRepositoryMock.populateMetaData());

            // Act
            var error: any;
            var data: Models.IProjectMeta;
            projectRepository.getProjectMeta(10).then((responce) => { data = responce; }, (err) => error = err);
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


        it("get - unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET(`svc/artifactstore/projects/10/meta/customtypes`)
                .respond(401);

            // Act
            var error: any; 
            var data: Models.IProjectMeta;
            projectRepository.getProjectMeta(10).then((responce) => { data = responce; }, (err) => error = err);
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


});