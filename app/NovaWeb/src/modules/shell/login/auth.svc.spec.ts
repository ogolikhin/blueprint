import "angular";
import "angular-mocks"
import {IAuth, IUser, AuthSvc} from "./auth.svc";

describe("AuthSvc", () => {
    var $httpBackend: ng.IHttpBackendService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("auth", AuthSvc);
    }));

    describe("getCurrentUser", () => {
        it("reject on error with default error message", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(401);

            // Act
            var error: any;
            var result = auth.getCurrentUser().then(() => { }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(401, "error.statusCode is not 401");
            expect(error.message).toBe("Cannot get current user", "error.message does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

    });
});