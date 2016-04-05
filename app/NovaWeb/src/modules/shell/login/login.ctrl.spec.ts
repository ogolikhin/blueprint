import "angular";
import "angular-mocks"
import {SessionSvc} from "./session.svc";
import {LoginCtrl} from "./login.ctrl";
import {IAuth} from "./auth.svc";
import {LocalizationServiceMock, ConfigValueHelperMock, AuthSvcMock, ModalServiceMock, ModalServiceInstanceMock} from "./mocks.spec";


describe("LoginCtrl", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("loginCtrl", LoginCtrl);
        $provide.service("auth", AuthSvcMock);
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("session", SessionSvc);
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

        it("return incorrect username or password error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "login").and.callFake(function () {
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

        it("return empty username or password error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "login").and.callFake(function () {
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

        it("return account disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "login").and.callFake(function () {
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

        it("return password expired error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "login").and.callFake(function () {
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

        it("return unexpected error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "login").and.callFake(function () {
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

        it("return session override error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "login").and.callFake(function () {
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

        it("return unexpected status code error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "login").and.callFake(function () {
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

    describe("goToChangePasswordScreen", () => {
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

        it("return account disabled error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "loginWithSaml").and.callFake(function () {
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

        it("return session override error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "loginWithSaml").and.callFake(function () {
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

        it("return unexpected error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "loginWithSaml").and.callFake(function () {
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

        it("return unexpected status code error", inject(($rootScope: ng.IRootScopeService, loginCtrl: LoginCtrl, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            spyOn(auth, "loginWithSaml").and.callFake(function () {
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
});