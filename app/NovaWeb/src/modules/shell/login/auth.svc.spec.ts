import * as angular from "angular";
import "angular-mocks";
import {IAuth, IUser, AuthSvc} from "./auth.svc";
import {HttpStatusCode} from "../../core/http";
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
                .respond(HttpStatusCode.Unauthorized, {statusCode: HttpStatusCode.Unauthorized} );
            $httpBackend.expectPOST("/Login/WinLogin.aspx", "")
                .respond(HttpStatusCode.Unauthorized, {statusCode: HttpStatusCode.Unauthorized});
            $rootScope["config"] = {
                settings: {DisableWindowsIntegratedSignIn: "false"}
            };

            // Act

            let error: any;
            auth.getCurrentUser().catch(err => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(HttpStatusCode.Unauthorized, "error.statusCode is not Unauthorized");
            expect(error.message).toBe("Login_Auth_CannotGetUser", "error.message does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("succeed with SSO login after failing to get user info",
            inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $rootScope: ng.IRootScopeService) => {
                // Arrange
                $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                    .respond(HttpStatusCode.Unauthorized, {statusCode: HttpStatusCode.Unauthorized});
                $httpBackend.expectPOST("/Login/WinLogin.aspx", "")
                    .respond(HttpStatusCode.Success, "6be473a999a140d894805746bf54c129");
                $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                    .respond(HttpStatusCode.Success);
                $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                    .respond(HttpStatusCode.Success, <IUser>{
                            displayName: "Default Instance Admin", login: "admin"
                        }
                    );
                $rootScope["config"] = {
                    settings: {DisableWindowsIntegratedSignIn: "false"}
                };

                // Act
                let error: any;
                let user: IUser;
                auth.getCurrentUser().then((response) => {
                    user = response;
                }, (err) => error = err);
                $httpBackend.flush();

                // Assert
                expect(error).toBe(undefined, "response got error");
                expect(user.login).toBe("admin", "user login does not match");
                $httpBackend.verifyNoOutstandingExpectation();
                $httpBackend.verifyNoOutstandingRequest();
            }));

        it("resolve successfully", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(HttpStatusCode.Success, <IUser>{
                        displayName: "Default Instance Admin", login: "admin"
                    }
                );

            // Act
            let error: any;
            let user: IUser;
            auth.getCurrentUser().then((response) => {
                user = response;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "response got error");
            expect(user.login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("login", () => {
        it("reject on error with default error message", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            const status: number = HttpStatusCode.Unauthorized;
            const message: string = "Login Failed";
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(status, {message: message, errorCode: 2000, statusCode: HttpStatusCode.Unauthorized});

            // Act
            let error: any;
            auth.login("admin", "changeme", false).catch(err => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(HttpStatusCode.Unauthorized, "error.statusCode is not Unauthorized");
            expect(error.message).toBe("Login Failed", "error.message does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with success", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(HttpStatusCode.Success, "6be473a999a140d894805746bf54c129"
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(HttpStatusCode.Success);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(HttpStatusCode.Success, <IUser>{
                        displayName: "Default Instance Admin", login: "admin"
                    }
                );

            // Act
            let error: any;
            let user: IUser;
            auth.login("admin", "changeme", false).then((response) => {
                user = response;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "response got error");
            expect(user.login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with error from missing token", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(HttpStatusCode.Success);

            // Act
            let error: any;
            let user: IUser;
            auth.login("admin", "changeme", false).then((response) => {
                user = response;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(HttpStatusCode.ServerError);
            expect(error.message).toBe("Login_Auth_SessionTokenRetrievalFailed");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with loginuser error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            //unicode to test encode function
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("Карл") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(HttpStatusCode.Success, <IUser>{
                        displayName: "Default Instance Admin", login: "admin"
                    }
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(HttpStatusCode.Success);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(HttpStatusCode.Unauthorized, {message: "Unauthorized error", statusCode: HttpStatusCode.Unauthorized});

            // Act
            let error: any;
            let user: IUser;
            auth.login("Карл", "changeme", false).then((response) => {
                user = response;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.statusCode).toBe(HttpStatusCode.Unauthorized, "error.statusCode is not HttpStatusCode.Unauthorized");
            expect(error.message).toBe("Unauthorized error");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with no licenses error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            //exotic unicode to test encode function
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("𐊇𐊈𐊉") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(HttpStatusCode.Success, <IUser>{
                        displayName: "Default Instance Admin", login: "admin"
                    }
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(HttpStatusCode.NotFound, {statusCode: HttpStatusCode.NotFound});
            $httpBackend.expectDELETE("/svc/adminstore/sessions")
                .respond(HttpStatusCode.Success);

            // Act
            let error: any;
            let user: IUser;
            auth.login("𐊇𐊈𐊉", "changeme", false).then((response) => {
                user = response;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.message).toBe("Login_Auth_LicenseNotFound_Verbose", "error.message does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with license limit error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/?login=" + AuthSvc.encode("admin") + "&force=false", angular.toJson(AuthSvc.encode("changeme")))
                .respond(HttpStatusCode.Success, <IUser>{
                        displayName: "Default Instance Admin", login: "admin"
                    }
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(HttpStatusCode.Forbidden, {statusCode: HttpStatusCode.Forbidden});
            $httpBackend.expectDELETE("/svc/adminstore/sessions")
                .respond(HttpStatusCode.Success);

            // Act
            let error: any;
            let user: IUser;
            auth.login("admin", "changeme", false).then((response) => {
                user = response;
            }, (err) => error = err);
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
                .respond(HttpStatusCode.Success);

            // Act
            let error: any;
            let user: IUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
            auth.logout(user, true).catch(err => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "response got error");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("loginWithSaml", () => {
        it("respond with success", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/sso?force=true", angular.toJson("PHNhbWx"))
                .respond(HttpStatusCode.Success, "6be473a999a140d894805746bf54c129"
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(HttpStatusCode.Success);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(HttpStatusCode.Success, <IUser>{
                        displayName: "Default Instance Admin", login: "admin"
                    }
                );

            // Act
            let error: any;
            let user: IUser;
            auth.loginWithSaml(true, "").then((response) => {
                user = response;
            }, (err) => error = err);
            $window["notifyAuthenticationResult"]("1", "PHNhbWx");
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "response got error");
            expect(user.login).toBe("admin", "user login does not match");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with saml error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/adminstore/sessions/sso?force=false", angular.toJson("PHNhbWx"))
                .respond(HttpStatusCode.Unauthorized, {message: "saml login error", statusCode: HttpStatusCode.Unauthorized});

            // Act
            let error: any;
            let user: IUser;
            auth.loginWithSaml(false, "").then((response) => {
                user = response;
            }, (err) => error = err);
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
                .respond(HttpStatusCode.Success, "6be473a999a140d894805746bf54c129"
                );
            $httpBackend.expectPOST("/svc/shared/licenses/verify", "")
                .respond(HttpStatusCode.Success);
            $httpBackend.expectGET("/svc/adminstore/users/loginuser")
                .respond(HttpStatusCode.Success, <IUser>{
                        displayName: "Default Instance Admin", login: "admin"
                    }
                );
            $httpBackend.expectDELETE("/svc/adminstore/sessions")
                .respond(HttpStatusCode.Success);

            // Act
            let error: any;
            let user: IUser;
            auth.loginWithSaml(true, "notAdmin").then((response) => {
                user = response;
            }, (err) => error = err);
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
            const login = "admin";
            const oldPassword = "changeme";
            const newPassword = "123EWQ!@#";

            const encUserName = AuthSvc.encode(login);
            const encOldPassword = AuthSvc.encode(oldPassword);
            const encNewPassword = AuthSvc.encode(newPassword);
            $httpBackend.expectPOST("/svc/adminstore/users/reset?login=" + encUserName, angular.toJson({
                OldPass: encOldPassword,
                NewPass: encNewPassword
            }))
                .respond(HttpStatusCode.Success);

            // Act
            let error: any;
            auth.resetPassword(login, oldPassword, newPassword).catch(err => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "response got error");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("respond with error", inject(($httpBackend: ng.IHttpBackendService, auth: IAuth, $window: ng.IWindowService) => {
            // Arrange
            const login = "admin";
            const oldPassword = "changeme";
            const newPassword = "123EWQ!@#";
            const errorMsg = "unauthorized error";

            const encUserName = AuthSvc.encode(login);
            const encOldPassword = AuthSvc.encode(oldPassword);
            const encNewPassword = AuthSvc.encode(newPassword);
            $httpBackend.expectPOST("/svc/adminstore/users/reset?login=" + encUserName, angular.toJson({
                OldPass: encOldPassword,
                NewPass: encNewPassword
            }))
                .respond(HttpStatusCode.Unauthorized, {message: errorMsg, statusCode: HttpStatusCode.Unauthorized});

            // Act
            let error: any;
            auth.resetPassword(login, oldPassword, newPassword).catch(err => error = err);
            $httpBackend.flush();

            // Assert
            expect(error.message).toBe(errorMsg);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });
});
