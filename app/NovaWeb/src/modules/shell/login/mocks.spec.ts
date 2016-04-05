import "angular";
import "angular-mocks"
import {ILocalizationService} from "../../core/localization";
import {IConfigValueHelper} from "../../core/config.value.helper";
import {IUser, IAuth} from "./auth.svc";

export class LocalizationServiceMock implements ILocalizationService {
    public get(name: string): string {
        return name;
    }
}

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