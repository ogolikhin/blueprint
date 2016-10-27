import * as angular from "angular";
import "angular-mocks";
require("script!mxClient");
import {IUserStoryService, UserStoryService} from "./user-story.svc";
import {MessageServiceMock} from "../../../core/messages/message.mock";
import {IUserStory} from "../models/process-models";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {CommunicationManager} from "../";

interface IHttpError {
    message: string;
    statusCode: number; // client side only
    errorCode: number;
}

describe("userStoryService", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("userStoryService", UserStoryService);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
    }));

    afterEach(inject(($httpBackend: ng.IHttpBackendService) => {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("generateUserStories for all tasks in process returns result",
        inject(($httpBackend: ng.IHttpBackendService, userStoryService: IUserStoryService) => {
            // arrange
            let projectId = 1;
            let processId = 2;
            let userStories: IUserStory[] = <any>[{ processTaskId: 1 }, { processTaskId: 2 }];

            $httpBackend.whenPOST(`/svc/components/storyteller/projects/${projectId}/processes/${processId}/userstories`).respond(userStories);
            let result: IUserStory[];

            // act
            userStoryService.generateUserStories(projectId, processId).then((response: IUserStory[]) => {
                result = response;
            });
            $httpBackend.flush();

            // assert
            expect(result).not.toBeNull();
            expect(result).toEqual(userStories);
        })
    );

    it("generateUserStories for specific task returns result",
        inject(($httpBackend: ng.IHttpBackendService, userStoryService: IUserStoryService) => {
            // arrange
            let projectId = 1;
            let processId = 2;
            let taskId = 3;
            let userStories: IUserStory[] = <any>[{ processTaskId: taskId }];

            $httpBackend.whenPOST(`/svc/components/storyteller/projects/${projectId}/processes/${processId}/userstories?taskId=${taskId}`).respond(userStories);
            let result: IUserStory[];

            // act
            userStoryService.generateUserStories(projectId, processId, taskId).then((response: IUserStory[]) => {
                result = response;
            });
            $httpBackend.flush();

            // assert
            expect(result).not.toBeNull();
            expect(result).toEqual(userStories);
            expect(result[0].processTaskId).toBe(taskId);
        })
    );

    it("generateUserStories returns an error",
        inject(($httpBackend: ng.IHttpBackendService, userStoryService: IUserStoryService) => {
            // arrange
            let projectId = 1;
            let processId = 2;
            let taskId = 3;
            let userStories: IUserStory[] = <any>[{ processTaskId: taskId }];

            $httpBackend.whenPOST(`/svc/components/storyteller/projects/${projectId}/processes/${processId}/userstories?taskId=${taskId}`)
                        .respond(401, <IHttpError>{ message: "error", statusCode: 401 });
            let result: IUserStory[];
            let error: IHttpError;

            // act
            userStoryService.generateUserStories(projectId, processId, taskId).then((response: IUserStory[]) => {
                result = response;
            }, (response: IHttpError) => {
                error = response;
            });
            $httpBackend.flush();

            // assert
            expect(result).toBeUndefined();
            expect(error).toBeDefined();
            expect(error.statusCode).toBe(401);
        })
    );
});