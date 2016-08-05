﻿import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {IArtifactDiscussions, ArtifactDiscussions, IDiscussionResultSet, IReply, IDiscussion} from "./artifact-discussions.svc";

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

    it("add discussion returns default discussion", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectPOST(`/svc/components/RapidReview/artifacts/5/discussions`)
            .respond(200,
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
            });


        // Act
        let error: any;
        let data: IDiscussion;
        artifactDiscussions.addDiscussion(5, "").then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.version).toEqual(3);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("add discussion gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectPOST(`/svc/components/RapidReview/artifacts/5/discussions`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IDiscussion;
        artifactDiscussions.addDiscussion(5, "").then((response) => {
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

    it("add discussion reply returns default reply", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectPOST(`/svc/components/RapidReview/artifacts/5/discussions/1/reply`)
            .respond(200,
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
            });


        // Act
        let error: any;
        let data: IReply;
        artifactDiscussions.addDiscussionReply(5, 1, "").then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.version).toEqual(3);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("add discussion reply gets an error if artifact id is invalid",
        inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
            // Arrange
            $httpBackend.expectPOST(`/svc/components/RapidReview/artifacts/5/discussions/1/reply`)
                .respond(404, {
                    statusCode: 404,
                    message: "Couldn't find the artifact"
                });

            // Act
            let error: any;
            let data: IReply;
            artifactDiscussions.addDiscussionReply(5, 1, "").then((response) => {
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

    it("edit discussion returns default discussion", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectPATCH(`/svc/components/RapidReview/artifacts/5/discussions/1`)
            .respond(200,
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
            });


        // Act
        let error: any;
        let data: IDiscussion;
        artifactDiscussions.editDiscussion(5, 1, "").then((response) => {
            data = (<any>response).data;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.version).toEqual(3);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("edit discussion gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectPATCH(`/svc/components/RapidReview/artifacts/5/discussions/1`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IDiscussion;
        artifactDiscussions.editDiscussion(5, 1, "").then((response) => {
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

    it("edit discussion reply returns default reply", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectPATCH(`/svc/components/RapidReview/artifacts/5/discussions/1/reply/1`)
            .respond(200,
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
            });


        // Act
        let error: any;
        let data: IReply;
        artifactDiscussions.editDiscussionReply(5, 1, 1, "").then((response) => {
            data = (<any>response).data;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.version).toEqual(3);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("edit discussion reply gets an error if artifact id is invalid",
        inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
            // Arrange
            $httpBackend.expectPATCH(`/svc/components/RapidReview/artifacts/5/discussions/1/reply/1`)
                .respond(404, {
                    statusCode: 404,
                    message: "Couldn't find the artifact"
                });

            // Act
            let error: any;
            let data: IReply;
            artifactDiscussions.editDiscussionReply(5, 1, 1, "").then((response) => {
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
    it("successfully deletes reply", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectDELETE(`/svc/components/RapidReview/artifacts/5/deletecomment/1`)
            .respond(200, {
                statusCode: 200,
                message: "Success"
            });

        // Act
        let success: boolean;
        artifactDiscussions.deleteReply(5, 1).then((response) => {
            success = true;
        }, () => {
            success = false;
        });

        $httpBackend.flush();

        // Assert
        expect(success).toBe(true);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("delete reply failes with 404", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectDELETE(`/svc/components/RapidReview/artifacts/5/deletecomment/1`)
            .respond(404, {
                statusCode: 404,
                message: "Comment Not Found"
            });
        let success: boolean;
        let errorStatusCode: number;
        
        // Act
        artifactDiscussions.deleteReply(5, 1).then((response) => {
            success = true;
        }, (error) => {
            success = false;
            errorStatusCode = error.statusCode;
        });
        $httpBackend.flush();

        // Assert
        expect(success).toBe(false);
        expect(errorStatusCode).toBe(404);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("successfully deletes thread", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectDELETE(`/svc/components/RapidReview/artifacts/5/deletethread/1`)
            .respond(200, {
                statusCode: 200,
                message: "Success"
            });

        // Act
        let success: boolean;
        artifactDiscussions.deleteCommentThread(5, 1).then((response) => {
            success = true;
        }, () => {
            success = false;
        });
        $httpBackend.flush();

        // Assert
        expect(success).toBe(true);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("delete thread failes with 404", inject(($httpBackend: ng.IHttpBackendService, artifactDiscussions: IArtifactDiscussions) => {
        // Arrange
        $httpBackend.expectDELETE(`/svc/components/RapidReview/artifacts/5/deletethread/1`)
            .respond(404, {
                statusCode: 404,
                message: "Comment Not Found"
            });
        let success: boolean;
        let errorStatusCode: number;
        // Act
        artifactDiscussions.deleteCommentThread(5, 1).then((response) => {
            success = true;
        }, (error) => {
            success = false;
            errorStatusCode = error.statusCode;
        });
        $httpBackend.flush();
        // Assert
        expect(success).toBe(false);
        expect(errorStatusCode).toBe(404);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

});
