import "angular";
import "angular-mocks"
import {ISession, SessionSvc} from "./session.svc";
import {IUser, IAuth} from "./auth.svc";

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

    public logout(userInfo: IUser, skipSamlLogout: boolean): ng.IPromise<any> {
        var deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}

export class ModalServiceMock implements ng.ui.bootstrap.IModalService {
    public open(options: ng.ui.bootstrap.IModalSettings): ng.ui.bootstrap.IModalServiceInstance {
        return null;
    }
}

describe("SessionSvc", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("session", SessionSvc);
        $provide.service("auth", AuthSvcMock);
        $provide.service("$uibModal", ModalServiceMock);
    }));

    describe("getCurrentUser", () => {
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