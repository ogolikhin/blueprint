import "angular";
import "angular-mocks";
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
        $httpBackend.expectGET(`/svc/shared/users/search?emailDiscussions=true&search=test`).respond(200, [ testUser ]);
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
        $httpBackend.expectGET(`/svc/shared/users/search?emailDiscussions=true&search=test`).respond(404, {
            statusCode: 404,
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
        expect(error.statusCode).toEqual(404);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

});