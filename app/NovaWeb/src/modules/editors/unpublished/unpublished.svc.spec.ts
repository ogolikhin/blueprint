import * as angular from "angular";
import "angular-mocks";
import "Rx";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {HttpStatusCode} from "../../core/http/http-status-code";
import {UnpublishedArtifactsService, IUnpublishedArtifactsService} from "./unpublished.svc";
import {IArtifact, IPublishResultSet} from "../../main/models/models";
import {ApplicationError} from "../../core/error/applicationError";

describe("Publish Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("publishService", UnpublishedArtifactsService);
    }));

    describe("Get Unpublished Changes", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            const observableHandlerSpy = jasmine.createSpy("unpublishedArtifactHandler");
            const observable = publishService.unpublishedArtifactsObservable.subscribeOnNext(observableHandlerSpy);
            $httpBackend.expectGET("/svc/bpartifactstore/artifacts/unpublished")
                .respond(HttpStatusCode.Success, <IPublishResultSet>{
                    artifacts: [<IArtifact>{
                        id: 2,
                        projectId: 1
                    }],
                    projects: [{
                        id: 1
                    }]
                });

            // Act
            let error: any;
            let result: IPublishResultSet;
            publishService.getUnpublishedArtifacts()
                .then(response => {
                    result = response;
                })
                .catch(err => {
                    error = err;
                });
            $httpBackend.flush();
            observable.dispose();

            // Assert
            expect(error).toBeUndefined();
            expect(result).not.toBeUndefined();
            expect(result.artifacts.length).toEqual(1);
            expect(result.projects.length).toEqual(1);
            expect(result.artifacts[0].id).toEqual(2);
            expect(result.artifacts[0].projectId).toEqual(1);
            expect(result.projects[0].id).toEqual(1);
            expect(observableHandlerSpy).toHaveBeenCalled();
            expect(observableHandlerSpy).toHaveBeenCalledWith(result);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("to get the same promise if called twice in a row", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            $httpBackend.expectGET("/svc/bpartifactstore/artifacts/unpublished")
                .respond(HttpStatusCode.Success);

            // Act
            const promise1 = publishService.getUnpublishedArtifacts();
            const promise2 = publishService.getUnpublishedArtifacts();
            $httpBackend.flush();

            // Assert
            expect(promise1).toBe(promise2);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("error", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            $httpBackend.expectGET("/svc/bpartifactstore/artifacts/unpublished")
                .respond(HttpStatusCode.NotFound, {
                    statusCode: HttpStatusCode.NotFound
                });

            // Act
            let error: any;
            let data: IPublishResultSet;
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

    describe("post Publish All", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            const observableHandlerSpy = jasmine.createSpy("processedArtifacts");
            const observable = publishService.processedArtifactsObservable.subscribeOnNext(observableHandlerSpy);
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=true")
                .respond(HttpStatusCode.Success, <IPublishResultSet>{
                    artifacts: [],
                    projects: []
                });

            // Act
            let error: any;
            let result: IPublishResultSet;
            publishService.publishAll()
                .then(response => {
                    result = response;
                })
                .catch(err => {
                    error = err;
                });
            $httpBackend.flush();
            observable.dispose();

            // Assert
            expect(error).toBeUndefined();
            expect(result).not.toBeUndefined();
            expect(result.artifacts.length).toEqual(0);
            expect(result.projects.length).toEqual(0);
            expect(observableHandlerSpy).toHaveBeenCalled();
            expect(observableHandlerSpy).toHaveBeenCalledWith(result);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("error", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=true")
                .respond(HttpStatusCode.NotFound, {
                    statusCode: HttpStatusCode.NotFound
                });

            // Act
            let error: any;
            let data: IPublishResultSet;
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

    describe("post Publish Specific Artifacts", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            const observableHandlerSpy = jasmine.createSpy("processedArtifacts");
            const observable = publishService.processedArtifactsObservable.subscribeOnNext(observableHandlerSpy);
            const ids: number[] = [1, 2, 4];
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=false", ids)
                .respond(HttpStatusCode.Success, <IPublishResultSet>{
                    artifacts: [],
                    projects: []
                });

            // Act
            let error: any;
            let result: IPublishResultSet;
            publishService.publishArtifacts(ids)
                .then(response => {
                    result = response;
                })
                .catch(err => {
                    error = err;
                });
            $httpBackend.flush();
            observable.dispose();

            // Assert
            expect(error).toBeUndefined();
            expect(result).not.toBeUndefined();
            expect(result.artifacts.length).toEqual(0);
            expect(result.projects.length).toEqual(0);
            expect(observableHandlerSpy).toHaveBeenCalled();
            expect(observableHandlerSpy).toHaveBeenCalledWith(result);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("error", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            let Ids: number[] = [1, 2, 4];
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/publish?all=false", Ids)
                .respond(HttpStatusCode.Conflict, {
                    statusCode: HttpStatusCode.Conflict,
                    errorContent: <IPublishResultSet>{
                        artifacts: [<IArtifact>{
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
            let data: IPublishResultSet;
            let errorContent: IPublishResultSet;
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

    describe("post Discard All", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            const observableHandlerSpy = jasmine.createSpy("processedArtifacts");
            const observable = publishService.processedArtifactsObservable.subscribeOnNext(observableHandlerSpy);
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/discard?all=true")
                .respond(HttpStatusCode.Success, <IPublishResultSet>{
                    artifacts: [],
                    projects: []
                });

            // Act
            let error: ApplicationError;
            let result: IPublishResultSet;
            publishService.discardAll()
                .then(response => {
                    result = response;
                })
                .catch((err: ApplicationError) => {
                    error = err;
                });
            $httpBackend.flush();
            observable.dispose();

            // Assert
            expect(error).toBeUndefined();
            expect(result).toBeDefined();
            expect(result.artifacts.length).toEqual(0);
            expect(result.projects.length).toEqual(0);
            expect(observableHandlerSpy).toHaveBeenCalled();
            expect(observableHandlerSpy).toHaveBeenCalledWith(result);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("Discard Specific Artifacts", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            const observableHandlerSpy = jasmine.createSpy("processedArtifacts");
            const observable = publishService.processedArtifactsObservable.subscribeOnNext(observableHandlerSpy);
            const ids: number[] = [1, 2];
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/discard?all=false", ids)
                .respond(HttpStatusCode.Success, <IPublishResultSet>{
                    artifacts: [
                        {
                            id: 1,
                            projectId: 1
                        },
                        {
                            id: 2,
                            projectId: 1
                        }
                    ],
                    projects: [{
                        id: 1,
                        name: "test"
                    }]
                });

            // Act
            let error: ApplicationError;
            let result: IPublishResultSet;
            publishService.discardArtifacts(ids)
                .then(response => {
                    result = response;
                })
                .catch(err => {
                    error = err;
                });
            $httpBackend.flush();
            observable.dispose();

            // Assert
            expect(error).toBeUndefined();
            expect(result).toBeDefined();
            expect(result.artifacts.length).toEqual(2);
            expect(result.projects.length).toEqual(1);
            expect(observableHandlerSpy).toHaveBeenCalled();
            expect(observableHandlerSpy).toHaveBeenCalledWith(result);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("error", inject(($httpBackend: ng.IHttpBackendService, publishService: IUnpublishedArtifactsService) => {
            // Arrange
            const ids: number[] = [1, 2];
            $httpBackend.expectPOST("/svc/bpartifactstore/artifacts/discard?all=false", ids)
                .respond(HttpStatusCode.Conflict, <ApplicationError>{
                    statusCode: HttpStatusCode.Conflict,
                    errorCode: 129,
                    message: "Artifact with ID 1 has nothing to discard."
                });

            // Act
            let error: ApplicationError;
            let result: IPublishResultSet;
            publishService.discardArtifacts(ids)
                .then(response => {
                    result = response;
                })
                .catch((err: ApplicationError) => {
                    error = err;
                });
            $httpBackend.flush();

            // Assert
            expect(result).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(HttpStatusCode.Conflict);
            expect(error.message).toBeDefined();
            expect(error.errorCode).toBe(129);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });
});
