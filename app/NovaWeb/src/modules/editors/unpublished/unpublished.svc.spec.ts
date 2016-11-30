import * as angular from "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {Models} from "../../main/models";
import {HttpStatusCode} from "../../core/http/http-status-code";
import {UnpublishedArtifactsService, IUnpublishedArtifactsService} from "./unpublished.svc";

describe("Publish Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("publishService", UnpublishedArtifactsService);
    }));

    describe("Get Unpublished Changes", () => {
        it("get unpublished changes successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            $httpBackend.expectGET("/svc/bpartifactstore/artifacts/unpublished")
                .respond(HttpStatusCode.Success, <Models.IPublishResultSet>{
                    artifacts: [<Models.IArtifact>{
                        id: 2,
                        projectId: 1
                    }],
                    projects: [{
                        id: 1
                    }]
                });

            // Act
            let error: any;
            let data: Models.IPublishResultSet;
            publishService.getUnpublishedArtifacts().then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).not.toBeUndefined();
            expect(data.artifacts.length).toEqual(1);
            expect(data.projects.length).toEqual(1);
            expect(data.artifacts[0].id).toEqual(2);
            expect(data.artifacts[0].projectId).toEqual(1);
            expect(data.projects[0].id).toEqual(1);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get unpublished changes error", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            $httpBackend.expectGET("/svc/bpartifactstore/artifacts/unpublished")
                .respond(HttpStatusCode.NotFound, {
                    statusCode: HttpStatusCode.NotFound
                });

            // Act
            let error: any;
            let data: Models.IPublishResultSet;
            publishService.getUnpublishedArtifacts().then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).not.toBeUndefined();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("Post Publish All", () => {
        it("post publish all successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=true")
                .respond(HttpStatusCode.Success, <Models.IPublishResultSet>{
                    artifacts: [],
                    projects: []
                });

            // Act
            let error: any;
            let data: Models.IPublishResultSet;
            publishService.publishAll().then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).not.toBeUndefined();
            expect(data.artifacts.length).toEqual(0);
            expect(data.projects.length).toEqual(0);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("post publish all error", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=true")
                .respond(HttpStatusCode.NotFound, {
                    statusCode: HttpStatusCode.NotFound
                });

            // Act
            let error: any;
            let data: Models.IPublishResultSet;
            publishService.publishAll().then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).not.toBeUndefined();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("Post Publish Specific Artifacts", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            let Ids: number[] = [1, 2, 4];
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=false", Ids)
                .respond(HttpStatusCode.Success, <Models.IPublishResultSet>{
                    artifacts: [],
                    projects: []
                });

            // Act
            let error: any;
            let data: Models.IPublishResultSet;
            publishService.publishArtifacts(Ids).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).not.toBeUndefined();
            expect(data.artifacts.length).toEqual(0);
            expect(data.projects.length).toEqual(0);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("error", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            let Ids: number[] = [1, 2, 4];
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=false", Ids)
                .respond(HttpStatusCode.Conflict, {
                    statusCode: HttpStatusCode.Conflict,
                    errorContent: <Models.IPublishResultSet>{
                        artifacts: [<Models.IArtifact>{
                            id: 2,
                            projectId: 1
                        }],
                        projects: [{
                            id: 1
                        }]
                    }
                });

            // Act
            let error: any;
            let data: Models.IPublishResultSet;
            let errorContent: Models.IPublishResultSet;
            publishService.publishArtifacts(Ids).then((responce) => {
                data = responce;
            }, (err) => {
                error = err;
                errorContent = err.errorContent;
            });
            $httpBackend.flush();

            // Assert
            expect(error).not.toBeUndefined();
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(HttpStatusCode.Conflict);
            expect(errorContent.artifacts.length).toEqual(1);
            expect(errorContent.projects.length).toEqual(1);
            expect(errorContent.artifacts[0].id).toEqual(2);
            expect(errorContent.artifacts[0].projectId).toEqual(1);
            expect(errorContent.projects[0].id).toEqual(1);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

});
