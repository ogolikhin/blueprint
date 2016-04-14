import "angular";
import "angular-mocks"
import {SessionSvc} from "./session.svc";
import {LoginCtrl} from "./login.ctrl";
import {LocalizationServiceMock, ConfigValueHelperMock, AuthSvcMock, ModalServiceMock, ModalServiceInstanceMock, SessionSvcMock} from "./mocks.spec";


describe("LoginCtrl", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("loginCtrl", LoginCtrl);
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("configValueHelper", ConfigValueHelperMock);
    }));

    describe("login", () => {
        it("complete login successfully", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(false, "label error is true");
        }));

        it("return incorrect username or password error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 401,
                    errorCode: 2000,
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(true, "field error is false");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Session_CredentialsInvalid", "error message is incorrect");
        }));

        it("return empty username or password error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 401,
                    errorCode: 2003
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(true, "field error is false");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Session_CredentialsCannotBeEmpty", "error message is incorrect");
        }));

        it("return account disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    errorCode: 2001,
                    statusCode: 401,
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Session_AccountDisabled");
        }));

        it("return password expired error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    errorCode: 2002,
                    statusCode: 401,
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Session_PasswordHasExpired");
        }));

        it("return unexpected error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 401,
                    errorCode: 2010,
                    message: "unexpected error"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(true, "field error is false");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("unexpected error");
        }));

        it("return license limit reached", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 403,
                    message: "Login_Auth_LicenseLimitReached"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Auth_LicenseLimitReached");
        }));

        it("return license server not found", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 404,
                    message: "Login_Auth_LicenseNotFound_Verbose"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Auth_LicenseNotFound_Verbose");
        }));

        it("return license verification error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 500,
                    message: "Login_Auth_LicenseVerificationFailed"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Auth_LicenseVerificationFailed");
        }));

        it("return session override error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 409
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(false, "label error is true");
        }));

        it("return unexpected status code error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "login").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 411,
                    message: "unexpected status code"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaPassword = "changeme";
            var result = loginCtrl.login();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(true, "field error is false");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("unexpected status code");
        }));
    });

    describe("goToForgetPasswordScreen", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            var result = loginCtrl.goToForgetPasswordScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.forgetPasswordScreenError).toBe(false, "forgetPasswordScreenError");
            expect(loginCtrl.forgetPasswordScreenUsername).toBe(loginCtrl.novaUsername, "forgetPasswordScreenUsername");
            expect(loginCtrl.isInForgetPasswordScreen).toBe(true, "isInForgetPasswordScreen");
        }));
    });

    describe("goToChangePasswordScreenBecauseExpired", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            var result = loginCtrl.goToChangePasswordScreenBecauseExpired();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "changePasswordScreenError");
            expect(loginCtrl.isInChangePasswordScreen).toBe(true, "isInChangePasswordScreen");
        }));
    });

    describe("goToUpdatePasswordScreen", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            var result = loginCtrl.goToChangePasswordScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(false, "changePasswordScreenError");
            expect(loginCtrl.isInChangePasswordScreen).toBe(true, "isInChangePasswordScreen");
        }));
    });

    describe("goToSAMLScreen", () => {
        it("complete login successfully", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            var result = loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(false, "label error is true");
            expect(loginCtrl.isInSAMLScreen).toBe(true, "isInSAMLScreen");
        }));

        it("return account disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    errorCode: 2001,
                    statusCode: 401,
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            var result = loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("Login_Session_AccountDisabled");
        }));

        it("return session override error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 409
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            var result = loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(false, "field error is true");
            expect(loginCtrl.labelError).toBe(false, "label error is true");
        }));

        it("return unexpected error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 401,
                    errorCode: 2010,
                    message: "unexpected error"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            var result = loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(true, "field error is false");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("unexpected error");
        }));

        it("return unexpected status code error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            spyOn(session, "loginWithSaml").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 411,
                    message: "unexpected status code"
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var error: any;
            var result = loginCtrl.goToSAMLScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.fieldError).toBe(true, "field error is false");
            expect(loginCtrl.labelError).toBe(true, "label error is false");
            expect(loginCtrl.errorMsg).toBe("unexpected status code");
        }));
    });

    describe("goToLoginScreen", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange

            // Act
            var result = loginCtrl.goToLoginScreen();
            $rootScope.$digest();

            // Assert
            expect(loginCtrl.isInLoginForm).toBe(true, "isInLoginForm");
        }));
    });

    describe("changePassword", () => {
        it("complete successfully", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(false, "change password error is true");
        }));

        it("respond with password confirm missmatch error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_PasswordConfirmMismatch");
        }));

        it("respond with password min length error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123E";
            loginCtrl.novaConfirmNewPassword = "123E";

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordMinLength");
        }));

        it("respond with password max length error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
            loginCtrl.novaConfirmNewPassword = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordMaxLength");
        }));

        it("respond with incorrect current password error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 401,
                    errorCode: 2000
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_EnterCurrentPassword");
        }));

        it("respond with login disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 401,
                    errorCode: 2001
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_AccountDisabled");
        }));

        it("respond with empty password error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 401,
                    errorCode: 2003
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_CurrentPasswordCannotBeEmpty");
        }));

        it("respond with new password empty error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 400,
                    errorCode: 4000
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCannotBeEmpty");
        }));

        it("respond with new password same as old error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 400,
                    errorCode: 4001
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordSameAsOld");
        }));

        it("respond with new password invalid error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 400,
                    errorCode: 4002
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
            expect(loginCtrl.changePasswordScreenMessage).toBe("Login_Session_NewPasswordCriteria");
        }));

        it("respond with unknown error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, session: SessionSvc, $q: ng.IQService) => {
            // Arrange
            loginCtrl.novaUsername = "admin";
            loginCtrl.novaCurrentPassword = "changeme";
            loginCtrl.novaNewPassword = "123EWQ!@#";
            loginCtrl.novaConfirmNewPassword = "123EWQ!@#";
            spyOn(session, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                var error = {
                    statusCode: 500,
                };
                deferred.reject(error);
                return deferred.promise;
            });

            // Act
            var result = loginCtrl.changePassword();
            $rootScope.$digest();
            
            // Assert
            expect(loginCtrl.changePasswordScreenError).toBe(true, "change password error is false");
        }));
    });
});