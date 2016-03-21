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

        it("resolve successfully", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(200, <IUser>{
                    DisplayName: "Default Instance Admin", Login: "admin"
                }
            );

            // Act
            var error: any;
            var user: IUser;
            var result = auth.getCurrentUser().then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(user.Login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("login", () => {
        it("reject on error with default error message", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            var status: number = 401;
            var message: string = "Login Failed";
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin")+"&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(status, { Message: message, ErrorCode: 2000 });

            // Act
            var error: any;
            var result = auth.login("admin", "changeme", false).then(() => { }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(401, "error.statusCode is not 401");
            expect(error.message).toBe("Login Failed", "error.message does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with success", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(200, <IUser>{
                    DisplayName: "Default Instance Admin", Login: "admin"
                }
            );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(200);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(200, <IUser>{
                    DisplayName: "Default Instance Admin", Login: "admin"
                }
            );

            // Act
            var error: any;
            var user: IUser;
            var result = auth.login("admin", "changeme",false).then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(user.Login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("logout", () => {
        it("complete logout", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectDELETE("/svc/adminstore/sessions")
                .respond(200);

            // Act
            var error: any;
            var user: IUser = <IUser>{ DisplayName: "Default Instance Admin", Login: "admin" };
            var result = auth.logout(user, true).then(() => {}, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });
});