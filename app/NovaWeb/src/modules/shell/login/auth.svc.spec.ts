import "angular";
import "angular-mocks";
import {IAuth, IUser, AuthSvc} from "./auth.svc";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {SettingsMock, WindowMock} from "./mocks.spec";

describe("AuthSvc", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("auth", AuthSvc);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("$window", WindowMock);
        $provide.service("settings", SettingsMock);
    }));

    describe("getCurrentUser", () => {
        it("reject on error with default error message", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $rootScope: ng.IRootScopeService) => {
            // Arrange
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(401);
            $httpBackend.expectPOST("/Login/WinLogin.aspx", "")
                .respond(401);
            $rootScope["config"] = {
                settings: { DisableWindowsIntegratedSignIn: "false" }
            };

            // Act

            var error: any;
            auth.getCurrentUser().then(() => { }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(401, "error.statusCode is not 401");
            expect(error.message).toBe("Login_Auth_CannotGetUser", "error.message does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("succeed with SSO login after failing to get user info",
            inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $rootScope: ng.IRootScopeService) => {
            // Arrange
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(401);
            $httpBackend.expectPOST("/Login/WinLogin.aspx", "")
                .respond(200, "6be473a999a140d894805746bf54c129");
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(200);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );
            $rootScope["config"] = {
                settings: { DisableWindowsIntegratedSignIn: "false" }
            };

            // Act
            var error: any;
            var user: IUser;
            auth.getCurrentUser().then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(user.login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("resolve successfully", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );

            // Act
            var error: any;
            var user: IUser;
            auth.getCurrentUser().then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(user.login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("login", () => {
        it("reject on error with default error message", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            var status: number = 401;
            var message: string = "Login Failed";
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(status, { message: message, errorCode: 2000 });

            // Act
            var error: any;
            auth.login("admin", "changeme", false).then(() => { }, (err) => error = err);
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
                .respond(200, "6be473a999a140d894805746bf54c129"
            );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(200);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );

            // Act
            var error: any;
            var user: IUser;
            auth.login("admin", "changeme", false).then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(user.login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with error from missing token", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(200);

            // Act
            var error: any;
            var user: IUser;
            auth.login("admin", "changeme", false).then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(500);
            expect(error.message).toBe("Login_Auth_SessionTokenRetrievalFailed");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with loginuser error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            //unicode to test encode function
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("Карл") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(200);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(401, { message: "401 error" });

            // Act
            var error: any;
            var user: IUser;
            auth.login("Карл", "changeme", false).then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(401, "error.statusCode is not 401");
            expect(error.message).toBe("401 error");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with no licenses error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            //exotic unicode to test encode function
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("𐊇𐊈𐊉") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(404);
            $httpBackend.expectDELETE("/svc/adminstore/sessions")
                .respond(200);

            // Act
            var error: any;
            var user: IUser;
            auth.login("𐊇𐊈𐊉", "changeme", false).then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.message).toBe("Login_Auth_LicenseNotFound_Verbose", "error.message does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with license limit error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(403);
            $httpBackend.expectDELETE("/svc/adminstore/sessions")
                .respond(200);

            // Act
            var error: any;
            var user: IUser;
            auth.login("admin", "changeme", false).then((responce) => { user = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.message).toBe("Login_Auth_LicenseLimitReached");
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
            var user: IUser = <IUser>{ displayName: "Default Instance Admin", login: "admin" };
            auth.logout(user, true).then(() => {}, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("loginWithSaml", () => {
        it("respond with success", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/sso?force=true", angular.toJson("PHNhbWx"))
                .respond(200, "6be473a999a140d894805746bf54c129"
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(200);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );

            // Act
            var error: any;
            var user: IUser;
            auth.loginWithSaml(true, "").then((responce) => { user = responce; }, (err) => error = err);
            $window["notifyAuthenticationResult"]("1", "PHNhbWx");
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(user.login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with saml error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/sso?force=false", angular.toJson("PHNhbWx"))
                .respond(401, {message: "saml login error"});

            // Act
            var error: any;
            var user: IUser;
            auth.loginWithSaml(false, "").then((responce) => { user = responce; }, (err) => error = err);
            $window["notifyAuthenticationResult"]("1", "PHNhbWx");
            $httpBackend.flush();

            // Assert
            expect(error.message).toBe("Login_Auth_LoginFailed");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("reject with wrong user error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/sso?force=true", angular.toJson("PHNhbWx"))
                .respond(200, "6be473a999a140d894805746bf54c129"
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(200);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(200, <IUser>{
                    displayName: "Default Instance Admin", login: "admin"
                }
            );
            $httpBackend.expectDELETE("/svc/adminstore/sessions")
                .respond(200);

            // Act
            var error: any;
            var user: IUser;
            auth.loginWithSaml(true, "notAdmin").then((responce) => { user = responce; }, (err) => error = err);
            $window["notifyAuthenticationResult"]("1", "PHNhbWx");
            $httpBackend.flush();

            // Assert
            expect(error.message).toBe("Login_Auth_SamlContinueSessionWithOriginalUser");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

    });

    describe("resetPassword", () => {
        it("respond with success", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            var login = "admin";
            var oldPassword = "changeme";
            var newPassword = "123EWQ!@#";

            var encUserName = AuthSvc.encode(login);
            var encOldPassword = AuthSvc.encode(oldPassword);
            var encNewPassword = AuthSvc.encode(newPassword);
            $httpBackend.expectPOST("/svc/adminstore/users/reset?login=" + encUserName, angular.toJson({ OldPass: encOldPassword, NewPass: encNewPassword }))
                .respond(200);

            // Act
            var error: any;
            auth.resetPassword(login, oldPassword, newPassword).then(() => {}, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            var login = "admin";
            var oldPassword = "changeme";
            var newPassword = "123EWQ!@#";
            var errorMsg = "unauthorized error";

            var encUserName = AuthSvc.encode(login);
            var encOldPassword = AuthSvc.encode(oldPassword);
            var encNewPassword = AuthSvc.encode(newPassword);
            $httpBackend.expectPOST("/svc/adminstore/users/reset?login=" + encUserName, angular.toJson({ OldPass: encOldPassword, NewPass: encNewPassword }))
                .respond(401, { message: errorMsg});

            // Act
            var error: any;
            auth.resetPassword(login, oldPassword, newPassword).then(() => { }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.message).toBe(errorMsg);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });
});