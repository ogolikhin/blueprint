import * as angular from "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {ISettingsService} from "../../core";
import {IUser, IAuth} from "./auth.svc";
import {ISession} from "./session.svc";


export class SettingsMock implements ISettingsService {
    get(key: string, defaultValue?: string): string {
        return undefined;
    }

    getNumber(key: string, defaultValue?: number, minValue?: number, maxValue?: number): number {
        return undefined;
    }

    getBoolean(key: string, defaultValue?: boolean): boolean {
        if (key === "DisableWindowsIntegratedSignIn") {
            return false;
        }
        return undefined;
    }

    getObject(key: string, defaultValue?: any): any {
        return undefined;
    }
}

export class WindowMock {
    public location = {origin: "http://localhost:9876"};

    public open() {
        return;
    }
}

export class SessionSvcMock implements ISession {

    public static $inject = ["$q"];
    public currentUser: IUser = <IUser>{id: 1, displayName: "Default Instance Admin", login: "admin"};

    constructor(private $q: ng.IQService) {
    }

    public getLoginMessage(): string {
        return "";
    }

    public forceUsername(): string {
        return "";
    }

    public ensureAuthenticated() {
        const deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve();
        return deferred.promise;
    }

    public logout() {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public onExpired() {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public login(username: string, password: string, overrideSession: boolean) {
        const deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve();
        return deferred.promise;
    }

    public loginWithSaml(overrideSession: boolean) {
        const deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve();
        return deferred.promise;
    }

    public resetPassword(login: string, oldPassword: string, newPassword: string) {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}

export class AuthSvcMock implements IAuth {

    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public getCurrentUser(): ng.IPromise<IUser> {
        const deferred = this.$q.defer<IUser>();
        const user: IUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve(user);
        return deferred.promise;
    }

    public login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser> {
        const deferred = this.$q.defer<IUser>();
        const user: IUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve(user);
        return deferred.promise;
    }

    public loginWithSaml(overrideSession: boolean = false, prevLogin: string): ng.IPromise<IUser> {
        const deferred = this.$q.defer<IUser>();
        const user: IUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve(user);
        return deferred.promise;
    }

    public logout(userInfo: IUser, skipSamlLogout: boolean): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
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
        const controller: any = <any>options.controller;

        /* tslint:disable:no-unused-variable */
        const ctrl = new controller(
            new LocalizationServiceMock(this.$rootScope),
            this.instanceMock,
            new SessionSvcMock(this.$q),
            this.$timeout,
            new SettingsMock()
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
