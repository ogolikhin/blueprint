import "angular";
import {IDialogService, IDialogSettings, BaseDialogController} from "../../shared/";
import {IAuth, IUser} from "./auth.svc";
import {SessionTokenHelper} from "./session.token.helper";
import {LoginCtrl, ILoginInfo, ILoginModalDialogData} from "./login.ctrl";
import {ILocalizationService} from "../../commonModule/localization/localization.service";

export interface ISession {
    ensureAuthenticated(): ng.IPromise<any>;

    currentUser: IUser;

    logout(): ng.IPromise<any>;

    login(username: string, password: string, overrideSession: boolean): ng.IPromise<any>;

    loginWithSaml(overrideSession: boolean): ng.IPromise<any>;

    resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any>;

    onExpired(): ng.IPromise<any>;

    getLoginMessage(): string;

    forceUsername(): string;

    forceDisplayname(): string;
}

export class SessionSvc implements ISession {

    static $inject: [string] = ["$q", "auth", "$uibModal", "localization", "dialogService", "Analytics"];

    constructor(private $q: ng.IQService,
                private auth: IAuth,
                private $uibModal: ng.ui.bootstrap.IModalService,
                private localization: ILocalizationService,
                private dialogService: IDialogService,
                private analytics: ng.google.analytics.AnalyticsService) {
    }

    private _currentUser: IUser;
    private _loginMsg: string;
    private _prevLogin: string;
    private _isExpired: boolean;
    private _loginDialogPromise: ng.IPromise<any>;
    private _isForceSameUsername: boolean;

    public get currentUser(): IUser {
        return this._currentUser;
    }

    public set currentUser(user: IUser) {
        this._currentUser = user;
        this.setUserForAnalytics();
    }

    public forceUsername(): string {
        if (this.currentUser) {
            return this.currentUser.login;
        } else {
            return undefined;
        }
    }

    public forceDisplayname(): string {
        if (this.currentUser) {
            return this.currentUser.displayName;
        } else {
            return undefined;
        }
    }

    public getLoginMessage(): string {
        return this._loginMsg;
    }

    public logout(): ng.IPromise<any> {
        const defer = this.$q.defer();
        this.auth.logout(this.currentUser, false).then(() => defer.resolve());
        if (this.currentUser) {
            this._prevLogin = "";
        }
        this.currentUser = null;

        return defer.promise;
    }

    public login(username: string, password: string, overrideSession: boolean): ng.IPromise<any> {
        const defer = this.$q.defer();

        this.auth.login(username, password, overrideSession).then(
            (user) => {
                this.currentUser = user;
                defer.resolve();
            },
            (error) => {
                defer.reject(error);
            });
        return defer.promise;
    }

    public loginWithSaml(overrideSession: boolean): ng.IPromise<any> {
        const defer = this.$q.defer();

        this.auth.loginWithSaml(overrideSession, this._prevLogin).then(
            (user) => {
                this.currentUser = user;
                defer.resolve();

            },
            (error) => {
                defer.reject(error);
            });

        return defer.promise;
    }

    public onExpired(): ng.IPromise<any> {

        if (!this._isExpired && !this._loginDialogPromise) {
            this._isExpired = true;
            this._loginMsg = this.localization.get("Login_Session_Timeout");
            this._isForceSameUsername = true;
            return this.showLogin();
        }

        if (!this._loginDialogPromise) {
            this._loginDialogPromise = this.$q.resolve();
        }
        return this._loginDialogPromise;
    }

    public ensureAuthenticated(): ng.IPromise<any> {
        if (this.currentUser) {
            return this.$q.resolve();
        } else if (this._loginDialogPromise) {
            return this._loginDialogPromise;
        }

        this._loginMsg = this.localization.get("Login_Session_EnterCredentials");
        this._isForceSameUsername = false;
        if (SessionTokenHelper.hasSessionToken()) {
            return this.auth.getCurrentUser()
                .then(user => this.currentUser = user)
                .catch(() => this.showLogin());
        } else {
            return this.showLogin();
        }
    }

    private showLogin = (): ng.IPromise<any> => {
        this._loginDialogPromise = this.dialogService.open(<IDialogSettings>{
            template: require("./login.html"),
            css: "nova-login",
            controller: LoginCtrl,
            controllerAs: "ctrl",
            keyboard: false, // cannot Escape ))
            backdrop: false,
            bindToController: true
        }).then((result: ILoginInfo) => {
            if (result) {
                let confirmationDialog: ng.ui.bootstrap.IModalServiceInstance;
                if (result.loginSuccessful) {
                    this._isExpired = false;
                    this._loginDialogPromise = null;
                    return this.$q.resolve();
                }

                if (result.samlLogin) {
                    this._loginDialogPromise = this.dialogService
                        .confirm(this.localization.get("Login_Session_DuplicateSession_Verbose"), null, "nova-messaging nova-login-confirm")
                        .then(() => this.loginWithSaml(true))
                        .then(() => this._isExpired = false)
                        .catch(() => this.showLogin())
                        .finally(() => confirmationDialog = null);
                    return this._loginDialogPromise;
                }

                if (result.userName && result.password) {
                    this._loginDialogPromise = this.dialogService
                        .confirm(this.localization.get("Login_Session_DuplicateSession_Verbose"), null, "nova-messaging nova-login-confirm")
                        .then(() => this.login(result.userName, result.password, true))
                        .then(() => this._isExpired = false)
                        .catch(() => this.showLogin())
                        .finally(() => confirmationDialog = null);
                    return this._loginDialogPromise;
                }
                return this.showLogin();

            } else {
                return this.showLogin();
            }
        }).finally(() => {
            this._loginDialogPromise = null;
        });
        return this._loginDialogPromise;
    };

    private setUserForAnalytics() {
        if (this.currentUser && this.currentUser.id) {
            this.analytics.set("&uid", this.currentUser.id);
            this.analytics.set("dimension2", this.currentUser.id);
        }
    }

    public resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any> {
        const defer = this.$q.defer();

        this.auth.resetPassword(login, oldPassword, newPassword).then(
            () => {
                defer.resolve();
            },
            (error) => {
                defer.reject(error);
            });
        return defer.promise;
    }
}
