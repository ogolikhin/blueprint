import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../../core/localization.mock";
import { IArtifactHistory, IArtifactHistoryVersion, ArtifactHistory} from "./artifact-history.svc";

describe("Artifact History Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistory);
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("get artifact histories with default values", inject(($httpBackend: ng.IHttpBackendService, artifactHistory: IArtifactHistory) => {
        // Arrange
        $httpBackend.expectGET(`/svc/ArtifactStore/artifacts/306/version?asc=false&limit=10&offset=0`)
            .respond(200, {
                "artifactId": 306,
                "artifactHistoryVersions": [
                    {
                        "versionId": 52,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 51,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 50,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 49,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 48,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 47,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 46,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 45,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 44,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 43,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                ]
            });

        // Act
        let error: any;
        let data: IArtifactHistoryVersion[];
        artifactHistory.getArtifactHistory(306).then( (response) => {
            data = response;
        }, (err) => {
            error = err; 
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).toEqual(jasmine.any(Array));
        expect(data.length).toEqual(10);
        expect(data[0].versionId).toEqual(52);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("get first artifact history", inject(($httpBackend: ng.IHttpBackendService, artifactHistory: IArtifactHistory) => {
        // Arrange
        $httpBackend.expectGET(`/svc/ArtifactStore/artifacts/306/version?asc=false&limit=1&offset=0`)
            .respond(200, {
                "artifactId": 306,
                "artifactHistoryVersions": [
                    {
                    "versionId": 52,
                    "userId": 1,
                    "displayName": "admin",
                    "hasUserIcon": false,
                    "timestamp": "2016-06-06T13:58:24.557"
                    }
                ]
            });

        // Act
        let error: any;
        let data: IArtifactHistoryVersion[];
        artifactHistory.getArtifactHistory(306, 1, 0, null, false).then( (response) => {
            data = response;
        }, (err) => {
            error = err; 
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).toEqual(jasmine.any(Array));
        expect(data.length).toEqual(1);
        expect(data[0].versionId).toEqual(52);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactHistory: IArtifactHistory) => {
        // Arrange
        $httpBackend.expectGET(`/svc/ArtifactStore/artifacts/0/version?asc=false&limit=1&offset=0`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IArtifactHistoryVersion[];
        artifactHistory.getArtifactHistory(0, 1, 0, null, false).then( (response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(data).toBeUndefined();
        expect(error.statusCode).toEqual(404);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("get first 2 artifact histories in descending order", inject(($httpBackend: ng.IHttpBackendService, artifactHistory: IArtifactHistory) => {
        // Arrange
        $httpBackend.expectGET(`/svc/ArtifactStore/artifacts/306/version?asc=false&limit=2&offset=0`)
            .respond(200, {
                "artifactId": 306,
                "artifactHistoryVersions": [
                    {
                        "versionId": 52,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 51,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    }
                ]
            });

        // Act
        let error: any;
        let data: IArtifactHistoryVersion[];
        artifactHistory.getArtifactHistory(306, 2, 0, null, false).then( (response) => {
            data = response;
        }, (err) => {
            error = err; 
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).toEqual(jasmine.any(Array));
        expect(data.length).toEqual(2);
        expect(data[0].versionId).toEqual(52);
        expect(data[1].versionId).toEqual(51);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("get first 2 artifact histories in ascending order", inject(($httpBackend: ng.IHttpBackendService, artifactHistory: IArtifactHistory) => {
        // Arrange
        $httpBackend.expectGET(`/svc/ArtifactStore/artifacts/306/version?asc=true&limit=2&offset=0`)
            .respond(200, {
                "artifactId": 306,
                "artifactHistoryVersions": [
                    {
                        "versionId": 51,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    },
                    {
                        "versionId": 52,
                        "userId": 1,
                        "displayName": "admin",
                        "hasUserIcon": false,
                        "timestamp": "2016-06-06T13:58:24.557"
                    }
                ]
            });

        // Act
        let error: any;
        let data: IArtifactHistoryVersion[];
        artifactHistory.getArtifactHistory(306, 2, 0, null, true).then( (response) => {
            data = response;
        }, (err) => {
            error = err; 
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).toEqual(jasmine.any(Array));
        expect(data.length).toEqual(2);
        expect(data[0].versionId).toEqual(51);
        expect(data[1].versionId).toEqual(52);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));
});
