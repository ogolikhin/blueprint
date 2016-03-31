import "angular";
import "angular-mocks"
import {ILocalizationService} from "../../core/localization";
import {ISession, SessionSvc, ILoginInfo, LoginCtrl, SimpleDialogCtrl} from "./session.svc";
import {IUser, IAuth} from "./auth.svc";


export class LocalizationServiceMock implements ILocalizationService {
    public get(name: string): string {
        return name;
    }
}

export class AuthSvcMock implements IAuth {

    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) {
    }

    public getCurrentUser(): ng.IPromise<IUser> {
        var deferred = this.$q.defer<IUser>();
        var user: IUser = <IUser>{ DisplayName: "Default Instance Admin", Login: "admin" };
        deferred.resolve(user);
        return deferred.promise;
    }

    public login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser> {
        var deferred = this.$q.defer<IUser>();
        var user: IUser = <IUser>{ DisplayName: "Default Instance Admin", Login: "admin" };
        deferred.resolve(user);
        return deferred.promise;
    }

    public loginWithSaml(overrideSession: boolean = false, prevLogin: string): ng.IPromise<IUser> {
        var deferred = this.$q.defer<IUser>();
        var user: IUser = <IUser>{ DisplayName: "Default Instance Admin", Login: "admin" };
        deferred.resolve(user);
        return deferred.promise;
    }

    public logout(userInfo: IUser, skipSamlLogout: boolean): ng.IPromise<any> {
        var deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}

export class ModalServiceMock implements ng.ui.bootstrap.IModalService {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) {
        this.instanceMock = new ModalServiceInstanceMock(this.$q);
    }

    public instanceMock: ModalServiceInstanceMock;
    public loginCtrl;

    public open(options: ng.ui.bootstrap.IModalSettings): ng.ui.bootstrap.IModalServiceInstance {
        var ctrl = new options.controller(new LocalizationServiceMock());
        return this.instanceMock;
    }
}

export class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {
    public static $inject = ["$q"];
    private resultDeffered = this.$q.defer<any>();
    private openedDeffered = this.$q.defer<any>();

    constructor(private $q: ng.IQService) {
        this.opened = this.openedDeffered.promise;
        this.rendered = this.openedDeffered.promise;
        this.result = this.resultDeffered.promise;
    }

    public close(result?: any): void {
        this.resultDeffered.resolve(result);
    }

    public dismiss(reason?: any): void {
        this.resultDeffered.reject();
    }

    public result: angular.IPromise<any>;

    public opened: angular.IPromise<any>;

    public rendered: angular.IPromise<any>;
}

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

        it("return current user after logging in with session override", inject((localization: ILocalizationService, $rootScope: ng.IRootScopeService, session: ISession, auth: IAuth, $q: ng.IQService, $uibModal: ng.ui.bootstrap.IModalService) => {
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

describe("LoginCtrl", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("loginCtrl", LoginCtrl);
        $provide.service("auth", AuthSvcMock);
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("session", SessionSvc);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("localization", LocalizationServiceMock);
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
            expect(loginCtrl.errorMsg).toBe("Username and Password cannot be empty", "error message is incorrect");
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