﻿import "angular-mocks";
import {SessionSvc} from "./session.svc";
import {LoginCtrl, LoginState} from "./login.ctrl";
import {LocalizationServiceMock} from "../../commonModule/localization/localization.service.mock";
import {SettingsMock, ModalServiceMock, ModalServiceInstanceMock} from "./mocks.spec";
import {HttpStatusCode} from "../../commonModule/httpInterceptor/http-status-code";
import {DialogSettingsMock, DataMock} from "./login.ctrl.mock";
import {SessionSvcMock} from "./session.svc.mock";

describe("LoginCtrl", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("loginCtrl", LoginCtrl);
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("settings", SettingsMock);
        $provide.service("dialogSettings", DialogSettingsMock);
        $provide.service("dialogData", DataMock);
    }));

    describe("login", () => {
        it("complete login successfully", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(false);
        }));

        it("return incorrect username or password error",
            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                spyOn(session, "login").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: HttpStatusCode.Unauthorized,
                        errorCode: 2000
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaPassword = "changeme";
                loginCtrl.login();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(true);
                expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
                expect(loginCtrl.errorMessage).toBe("Login_Session_CredentialsInvalid", "error message is incorrect");
            }));

        it("return empty username or password error",
            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                spyOn(session, "login").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: HttpStatusCode.Unauthorized,
                        errorCode: 2003
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaPassword = "";
                loginCtrl.login();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(true);
                expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
                expect(loginCtrl.errorMessage).toBe("Login_Session_CredentialsCannotBeEmpty", "error message is incorrect");
            }));

        it("return account disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    errorCode: 2001,
                    statusCode: HttpStatusCode.Unauthorized
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("Login_Session_AccountDisabled", "error message is incorrect");
        }));

        it("return password expired error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    errorCode: 2002,
                    statusCode: HttpStatusCode.Unauthorized
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("Login_Session_PasswordHasExpired", "error message is incorrect");
        }));

        it("return password expired error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    errorCode: 1001,
                    statusCode: HttpStatusCode.Unauthorized
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("Login_Auth_FederatedFallbackDisabled", "error message is incorrect");
        }));

        it("return unexpected error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: HttpStatusCode.Unauthorized,
                    errorCode: 2010,
                    message: "unexpected error"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(true);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("unexpected error");
        }));

        it("return license limit reached", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: HttpStatusCode.Forbidden,
                    message: "Login_Auth_LicenseLimitReached"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("Login_Auth_LicenseLimitReached");
        }));

        it("return license server not found", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: HttpStatusCode.NotFound,
                    message: "Login_Auth_LicenseNotFound_Verbose"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("Login_Auth_LicenseNotFound_Verbose");
        }));

        it("return session override error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: 409
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(false);
        }));

        it("return unexpected status code error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: 411,
                    message: "unexpected status code"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaPassword = "changeme";
            loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(true);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("unexpected status code");
        }));
    });

    describe("goToForgetPasswordScreen", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            loginCtrl.goToForgetPasswordScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasForgetPasswordScreenError).toBe(false);
            expect(loginCtrl.forgetPasswordScreenUsername).toBe(loginCtrl.novaUserName, "forgetPasswordScreenUsername");
            expect(loginCtrl.isInForgetPasswordScreen).toBe(true);
        }));
    });

    describe("goToChangePasswordScreenBecauseExpired", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            loginCtrl.goToChangePasswordScreenBecauseExpired();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.isInChangePasswordScreen).toBe(true);
        }));
    });

    describe("goToUpdatePasswordScreen", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            loginCtrl.goToChangePasswordScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(false);
            expect(loginCtrl.isInChangePasswordScreen).toBe(true);
        }));
    });

    describe("goToSAMLScreen", () => {
        it("complete login successfully", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(false);
            expect(loginCtrl.isInSAMLScreen).toBe(true);
        }));

        it("return account disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    errorCode: 2001,
                    statusCode: HttpStatusCode.Unauthorized
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("Login_Session_AccountDisabled", "error message is incorrect");
        }));

        it("return account in AD but not in BP", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    errorCode: 2000,
                    statusCode: HttpStatusCode.Unauthorized
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.currentFormState).toBe(LoginState.LoginForm, "form is not back at login");
            expect(loginCtrl.errorMessage).toBe("Login_Session_ADUserNotInDB", "error message is incorrect");
        }));

        it("return session override error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: 409
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(false);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(false);
        }));

        it("return unexpected error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: HttpStatusCode.Unauthorized,
                    errorCode: 2010,
                    message: "unexpected error"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(true);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("unexpected error");
        }));

        it("return unexpected status code error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: 411,
                    message: "unexpected status code"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isTextFieldErrorStyleShowing).toBe(true);
            expect(loginCtrl.isLabelErrorStyleShowing).toBe(true);
            expect(loginCtrl.errorMessage).toBe("unexpected status code");
        }));
    });

    describe("goToLoginScreen", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            loginCtrl.goToLoginScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isInLoginForm).toBe(true, "isInLoginForm");
        }));
    });

    describe("changePassword", () => {
        it("complete successfully", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(false);
        }));

        it("respond with password confirm mismatch error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_PasswordConfirmMismatch");
        }));

        it("respond with password equals to username error (case insensitive)", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUserName = "username<1>";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "UserName<1>";
            loginCtrl.novaConfirmNewPassword = "UserName<1>";

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCannotBeUsername");
        }));

        it("respond with password equals to display name error (case insensitive)", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUserName = "Username<1>";
            loginCtrl.novaDisplayName = "displayname<1>";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "DisplayName<1>";
            loginCtrl.novaConfirmNewPassword = "DisplayName<1>";

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCannotBeDisplayname");
        }));

        it("respond with password min length error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123E";
            loginCtrl.novaConfirmNewPassword = "123E";

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordMinLength");
        }));

        it("respond with password max length error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            /* tslint:disable:max-line-length */
            loginCtrl.novaNewPassword = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
            loginCtrl.novaConfirmNewPassword = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
            /* tslint:enable:max-line-length */

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordMaxLength");
        }));

        it("respond with incorrect current password error",
            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaCurrentPassword = "changeme";
                loginCtrl.novaNewPassword = "123EWQ!@#";
                loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
                spyOn(session, "resetPassword").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: HttpStatusCode.Unauthorized,
                        errorCode: 2000
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.changePassword();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
                expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_EnterCurrentPassword");
            }));

        it("respond with login disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: HttpStatusCode.Unauthorized,
                    errorCode: 2001
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_AccountDisabled");
        }));

        it("respond with empty password error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: HttpStatusCode.Unauthorized,
                    errorCode: 2003
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_CurrentPasswordCannotBeEmpty");
        }));

        it("respond with new password empty error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: 400,
                    errorCode: 4000
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCannotBeEmpty");
        }));

        it("respond with new password same as old error",
            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaCurrentPassword = "changeme";
                loginCtrl.novaNewPassword = "123EWQ!@#";
                loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
                spyOn(session, "resetPassword").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: 400,
                        errorCode: 4001
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.changePassword();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
                expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordSameAsOld");
            }));

        it("respond with new password invalid error",
            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaCurrentPassword = "changeme";
                loginCtrl.novaNewPassword = "123EWQ!@#";
                loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
                spyOn(session, "resetPassword").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: 400,
                        errorCode: 4002
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.changePassword();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
                expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCriteria");
            }));

        it("responds with new password cannot equal login name",
            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaCurrentPassword = "changeme";
                loginCtrl.novaNewPassword = "123EWQ!@#";
                loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
                spyOn(session, "resetPassword").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: 400,
                        errorCode: 4005
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.changePassword();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
                expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCannotBeLoginname");
            }));

        it("responds with new password cannot equal display name (serverside fallback)",
            //Note: We test this above for the client-side, but client-side data may be out of date, so it's possible to get this from the server.

            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaCurrentPassword = "changeme";
                loginCtrl.novaNewPassword = "123EWQ!@#";
                loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
                spyOn(session, "resetPassword").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: 400,
                        errorCode: 4006
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.changePassword();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
                expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCannotBeDisplayname");
            }));

        it("respond with password cooldown in effect",
            inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
                // Arrange
                loginCtrl.novaUserName = "admin";
                loginCtrl.novaCurrentPassword = "changeme";
                loginCtrl.novaNewPassword = "123EWQ!@#";
                loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
                spyOn(session, "resetPassword").and.callFake(function () {
                    const deferred = $q.defer();
                    const error = {
                        statusCode: 400,
                        errorCode: 4003
                    };
                    deferred.reject(error);
                    return deferred.promise;
                });

                // Act
                loginCtrl.changePassword();
                $rootScope.$digest();

                // Assert
                expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
                expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_PasswordChangeCooldown");
            }));

        it("respond with unknown error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUserName = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                const deferred = $q.defer();
                const error = {
                    statusCode: HttpStatusCode.ServerError
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            loginCtrl.changePassword();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.hasChangePasswordScreenError).toBe(true);
        }));
    });
});
