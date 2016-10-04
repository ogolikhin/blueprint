import * as angular from "angular";
import "angular-mocks";
import { HttpStatusCode } from "../http";
import { UserOrGroupInfo } from "./users-and-groups.svc.mock";
import { IUserOrGroupInfo, UsersAndGroupsService } from "./users-and-groups.svc";

describe("Users And Groups Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("usersAndGroupsService", UsersAndGroupsService);
        //$provide.service("localization", LocalizationServiceMock);
    }));

    it("Search for users, user returned", inject(($httpBackend: ng.IHttpBackendService, usersAndGroupsService: UsersAndGroupsService) => {
        const searchValue = "test";
        let emailDiscussions = true;
        let testUser = new UserOrGroupInfo("test name", "test@test.com", false, false, false);
        $httpBackend.expectGET(`/svc/shared/users/search?emailDiscussions=true&search=test`).respond(HttpStatusCode.Success, [ testUser ]);
        let userResponse: IUserOrGroupInfo;
        usersAndGroupsService.search(searchValue, emailDiscussions).then((response) => {
            userResponse = response[0];
        });
        $httpBackend.flush();

        expect(userResponse).toBeDefined();
        expect(userResponse.name).toEqual("test name");
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("Search for users, user was not found", inject(($httpBackend: ng.IHttpBackendService, usersAndGroupsService: UsersAndGroupsService) => {
        const searchValue = "test";
        let emailDiscussions = true;
        $httpBackend.expectGET(`/svc/shared/users/search?emailDiscussions=true&search=test`).respond(HttpStatusCode.NotFound, {
            statusCode: HttpStatusCode.NotFound,
            message: "Couldn't find the user"
        });
        let userResponse: IUserOrGroupInfo;
        let error: any;
        usersAndGroupsService.search(searchValue, emailDiscussions).then((response) => {
            userResponse = response[0];
        }, (err) => {
                error = err;
            });
        $httpBackend.flush();

        expect(userResponse).toBeUndefined();
        expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("Search for users with limit N, N users returned", inject(($httpBackend: ng.IHttpBackendService, usersAndGroupsService: UsersAndGroupsService) => {
        const searchValue = "test";
        let emailDiscussions = true;
        let testUser1 = new UserOrGroupInfo("test name 1", "test1@test.com", false, false, false);
        let testUser2 = new UserOrGroupInfo("test name 2", "test2@test.com", false, false, false);
        $httpBackend.expectGET(`/svc/shared/users/search?emailDiscussions=true&search=test`).respond(HttpStatusCode.Success, [ testUser1, testUser2 ]);
        let userResponse: IUserOrGroupInfo[];
        usersAndGroupsService.search(searchValue, emailDiscussions).then((response) => {
            userResponse = response;
        });
        $httpBackend.flush();

        expect(userResponse).toBeDefined();
        expect(userResponse.length).toEqual(2);
        expect(userResponse[0].name).toEqual("test name 1");
        expect(userResponse[1].name).toEqual("test name 2");
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));
});
