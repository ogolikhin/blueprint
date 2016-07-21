import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../../core/localization.mock";
import {IArtifactDiscussions, ArtifactDiscussions, IDiscussionResultSet, IReply} from "./artifact-discussions.svc";

describe("Artifact Discussion Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactDiscussions", ArtifactDiscussions);
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("get artifact discussions with default values", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/discussions`)
            .respond(200, {
                "canCreate": true,
                "canDelete": true,
                "discussions": [
                    {
                        "isClosed": false,
                        "status": "",
                        "itemId": 1,
                        "repliesCount": 1,
                        "discussionId": 1,
                        "version": 3,
                        "userId": 1,
                        "lastEditedOn": "2016-05-31T17:19:53.07",
                        "userName": "Mehdi",
                        "isGuest": false,
                        "comment": "This is a test."
                    },
                    {
                        "isClosed": true,
                        "status": "",
                        "itemId": 2,
                        "repliesCount": 0,
                        "discussionId": 1,
                        "version": 4,
                        "userId": 2,
                        "lastEditedOn": "",
                        "userName": "Mehdi",
                        "isGuest": false,
                        "comment": "flakdj alkdjf lajd f."
                    }
                ]
            });

        // Act
        let error: any;
        let data: IDiscussionResultSet;
        artifactDiscussions.getArtifactDiscussions(5).then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.discussions.length).toEqual(2);
        expect(data.discussions[0].version).toEqual(3);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/discussions`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IDiscussionResultSet;
        artifactDiscussions.getArtifactDiscussions(5).then((response) => {
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

    it("get artifact replies with default values", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/discussions/2/replies`)
            .respond(200,
            [
                {
                    "replyId": 1,
                    "itemId": 1,
                    "discussionId": 1,
                    "version": 3,
                    "userId": 1,
                    "lastEditedOn": "",
                    "userName": "Mehdi",
                    "isGuest": false,
                    "comment": "This is a test."
                }
            ]);


        // Act
        let error: any;
        let data: IReply[];
        artifactDiscussions.getReplies(5, 2).then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.length).toEqual(1);
        expect(data[0].version).toEqual(3);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/discussions/2/replies`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IReply[];
        artifactDiscussions.getReplies(5, 2).then((response) => {
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
});
