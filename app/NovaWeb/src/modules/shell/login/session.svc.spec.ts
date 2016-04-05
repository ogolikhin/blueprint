import "angular";
import "angular-mocks"
import {ISession, SessionSvc} from "./session.svc";
import {ILoginInfo, LoginCtrl} from "./login.ctrl";
import {IAuth} from "./auth.svc";
import {LocalizationServiceMock, ConfigValueHelperMock, WindowMock, AuthSvcMock, ModalServiceMock, ModalServiceInstanceMock} from "./mocks.spec";

describe("SessionSvc", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("session", SessionSvc);
        $provide.service("auth", AuthSvcMock);
        $provide.service("$uibModal", ModalServiceMock);
    
    }));

    describe("ensureAuthenticated", () => {
        it("return current user from auth service", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange

            // Act
            var error: any;
            var result = session.ensureAuthenticated().then(() => {}, (err) => error = err);
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
            expect(session.currentUser).toBeDefined();
            expect(session.currentUser.Login).toBe("admin", "current user is not admin");
        }))

        it("return current user after logging in with session override", inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
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
            var result = session.ensureAuthenticated().then(() => { }, (err) => error = err);
            (<ModalServiceMock>$uibModal).instanceMock.close(loginInfo); //simulate user input in login dialog
            
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
            expect(session.currentUser).toBeDefined();
            expect(session.currentUser.Login).toBe("admin", "current user is not admin");
        }))

        it("return current user after logging in without session override", inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
            // Arrange
            spyOn(auth, "getCurrentUser").and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            });
            var loginInfo: ILoginInfo = new ILoginInfo();
            loginInfo.loginSuccessful = true;

            // Act
            var error: any;
            var result = session.ensureAuthenticated().then(() => { }, (err) => error = err);
            (<ModalServiceMock>$uibModal).instanceMock.close(loginInfo); //simulate user input in login dialog
            
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
        }))
    });

    describe("loginWithSaml", () => {
        it("return current user from auth service", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange

            // Act
            var error: any;
            var result = session.loginWithSaml(true).then(() => { }, (err) => error = err);
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
            expect(session.currentUser).toBeDefined();
            expect(session.currentUser.Login).toBe("admin", "current user is not admin");
        }))

        it("return current user after logging in without session override", inject(($rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
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
            var result = session.ensureAuthenticated().then(() => { }, (err) => error = err);
            (<ModalServiceMock>$uibModal).instanceMock.close(loginInfo); //simulate user input in login dialog
            
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
        }))
    });

    describe("logout", () => {
        it("complete logout successfully", inject(($rootScope: ng.IRootScopeService, session: ISession) => {
            // Arrange

            // Act
            var error: any;
            var result = session.logout().then(() => { }, (err) => { error = err; });
            $rootScope.$digest();

            // Assert
            expect(error).toBe(undefined, "error is set");
        }));
    });
});
