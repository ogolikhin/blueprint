import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {IConfigValueHelper} from "../../core";
import {IUser, IAuth} from "./auth.svc";
import {ISession} from "./session.svc";


export class ConfigValueHelperMock implements IConfigValueHelper {
    getBooleanValue(setting: string, fallBack?: boolean) {
        if (setting === "DisableWindowsIntegratedSignIn") {
            return false;
        } else {
            return undefined;
        }
    }

    getStringValue(setting: string, fallBack?: string) {
        return undefined;
    }
}

export class WindowMock {
    public location = { origin: "http://localhost:9876" };
    public open() { }
}

export class SessionSvcMock implements ISession {

    public static $inject = ["$q"];
    public currentUser: IUser;

    constructor(private $q: ng.IQService) {
    }

    public getLoginMessage(): string {
        return "";
    }

    public forceUsername(): string {
        return "";
    }

    public ensureAuthenticated() {
        var deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{ displayName: "Default Instance Admin", login: "admin" };
        deferred.resolve();
        return deferred.promise;
    }

    public logout() {
        var deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public onExpired() {
        var deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public login(username: string, password: string, overrideSession: boolean) {
        var deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{ displayName: "Default Instance Admin", login: "admin" };
        deferred.resolve();
        return deferred.promise;
    }

    public loginWithSaml(overrideSession: boolean) {
        var deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{ displayName: "Default Instance Admin", login: "admin" };
        deferred.resolve();
        return deferred.promise;
    }

    public resetPassword(login: string, oldPassword: string, newPassword: string) {
        var deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}

export class AuthSvcMock implements IAuth {

    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) {
    }

    public getCurrentUser(): ng.IPromise<IUser> {
        var deferred = this.$q.defer<IUser>();
        var user: IUser = <IUser>{ displayName: "Default Instance Admin", login: "admin" };
        deferred.resolve(user);
        return deferred.promise;
    }

    public login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser> {
        var deferred = this.$q.defer<IUser>();
        var user: IUser = <IUser>{ displayName: "Default Instance Admin", login: "admin" };
        deferred.resolve(user);
        return deferred.promise;
    }

    public loginWithSaml(overrideSession: boolean = false, prevLogin: string): ng.IPromise<IUser> {
        var deferred = this.$q.defer<IUser>();
        var user: IUser = <IUser>{ displayName: "Default Instance Admin", login: "admin" };
        deferred.resolve(user);
        return deferred.promise;
    }

    public logout(userInfo: IUser, skipSamlLogout: boolean): ng.IPromise<any> {
        var deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any> {
        var deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}

export class ModalServiceMock implements ng.ui.bootstrap.IModalService {
    public static $inject = ["$q", "$timeout", "$rootScope"];
    constructor(private $q: ng.IQService, private $timeout: ng.ITimeoutService, private $rootScope: ng.IRootScopeService) {
        this.instanceMock = new ModalServiceInstanceMock(this.$q);
    }

    public instanceMock: ModalServiceInstanceMock;
    public loginCtrl;

    public open(options: ng.ui.bootstrap.IModalSettings): ng.ui.bootstrap.IModalServiceInstance {
        // typescript trick
        var controller: any = <any>options.controller;
        
        /* tslint:disable:no-unused-variable */
        var ctrl = new controller(
            new LocalizationServiceMock(this.$rootScope),
            this.instanceMock,
            new SessionSvcMock(this.$q),
            this.$timeout,
            new ConfigValueHelperMock()
        );
        return this.instanceMock;
        /* tslint:enable:no-unused-variable */
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

    public closed: angular.IPromise<any>;
}