import * as angular from "angular";
import "angular-mocks";
import { ISession, SessionSvc } from "./session.svc";
import { ILoginInfo } from "./login.ctrl";
import { IAuth} from "./auth.svc";
import { LocalizationServiceMock} from "../../core/localization/localization.mock";
import { AuthSvcMock, ModalServiceMock } from "./mocks.spec";
import { DialogService} from "../../shared/widgets/bp-dialog/bp-dialog";

describe("SessionSvc", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("session", SessionSvc);
        $provide.service("auth", AuthSvcMock);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
    }));

    describe("ensureAuthenticated", () => {
        it("return current user from auth service", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange

            // Act
            var error: any;
            session.ensureAuthenticated().then(() => {}, (err) => error = err);
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
            expect(session.currentUser).toBeDefined();
            expect(session.currentUser.login).toBe("admin", "current user is not admin");
        }));

        it("return current user after logging in with session override",
            inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
            // Arrange
            spyOn(auth, "getCurrentUser").and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            });
            var loginInfo: ILoginInfo = new ILoginInfo();
            loginInfo.userName = "admin";
            loginInfo.password = "changeme";

            // Act
            var error: any;
            session.ensureAuthenticated().then(() => { }, (err) => error = err);
            (<ModalServiceMock>$uibModal).instanceMock.close(loginInfo); //simulate user input in login dialog

            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
            expect(session.currentUser).toBeDefined();
            expect(session.currentUser.login).toBe("admin", "current user is not admin");
        }));

        it("return current user after logging in without session override",
            inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
            // Arrange
            spyOn(auth, "getCurrentUser").and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();  //simulate invalid response
                return deferred.promise;
            });
            var loginInfo: ILoginInfo = new ILoginInfo();
            loginInfo.loginSuccessful = true;

            // Act
            var error: any;
            session.ensureAuthenticated().then(() => { }, (err) => error = err);
            (<ModalServiceMock>$uibModal).instanceMock.close(loginInfo); //simulate user input in login dialog

            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
        }));
    });

    describe("loginWithSaml", () => {
        it("return current user from auth service", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange

            // Act
            var error: any;
            session.loginWithSaml(true).then(() => { }, (err) => error = err);
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
            expect(session.currentUser).toBeDefined();
            expect(session.currentUser.login).toBe("admin", "current user is not admin");
        }));

        it("return current user after logging in without session override",
            inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
            // Arrange
            spyOn(auth, "getCurrentUser").and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            });
            var loginInfo: ILoginInfo = new ILoginInfo();
            loginInfo.samlLogin = true;
            loginInfo.loginSuccessful = false;

            // Act
            var error: any;
            session.ensureAuthenticated().then(() => { }, (err) => error = err);
            (<ModalServiceMock>$uibModal).instanceMock.close(loginInfo); //simulate user input in login dialog

            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
        }));

        it("return error",
            inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
            // Arrange
            var errorMsg = "login error";
            spyOn(auth, "loginWithSaml").and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({ message: errorMsg });
                return deferred.promise;
            });

            // Act
            var error: any;
            session.loginWithSaml(true).then(() => { }, (err) => error = err);

            $rootScope.$digest();

            // Assert
            expect(error).toBeDefined();
            expect(error.message).toBe(errorMsg);
        }));
    });

    describe("login", () => {
        it("return error",
            inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
            // Arrange
            var errorMsg = "login error";
            var userName = "admin";
            var password = "changeme";
            spyOn(auth, "login").and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({ message: errorMsg});
                return deferred.promise;
            });

            // Act
            var error: any;
            session.login(userName, password, true).then(() => { }, (err) => error = err);

            $rootScope.$digest();

            // Assert
            expect(error).toBeDefined();
            expect(error.message).toBe(errorMsg);
        }));
    });

    describe("logout", () => {
        it("complete logout successfully", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange

            // Act
            var error: any;
            session.logout().then(() => { }, (err) => { error = err; });
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
        }));
    });

    describe("onExpired", () => {
        it("set correct login message", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange

            // Act
            var error: any;
            session.onExpired().then(() => { }, (err) => { error = err; });
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
            expect(session.getLoginMessage()).toBe("Login_Session_Timeout");
        }));
    });

    describe("resetPassword", () => {
        it("return success", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange
            var login = "admin";
            var oldPassword = "changeme";
            var newPassword = "123EWQ!@#";

            // Act
            var error: any;
            session.resetPassword(login, oldPassword, newPassword).then(() => { }, (err) => error = err);
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
        }));

        it("return error", inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService) => {
            // Arrange
            var login = "admin";
            var oldPassword = "changeme";
            var newPassword = "123EWQ!@#";
            spyOn(auth, "resetPassword").and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({message: "error"});
                return deferred.promise;
            });

            // Act
            var error: any;
            session.resetPassword(login, oldPassword, newPassword).then(() => { }, (err) => error = err);
            $rootScope.$digest();

            // Assert
            expect(error).toBeDefined();
        }));
    });
});
