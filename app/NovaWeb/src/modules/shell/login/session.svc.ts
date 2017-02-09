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

    static $inject: [string] = ["$q", "auth", "$uibModal", "localization", "dialogService"];

    constructor(private $q: ng.IQService,
                private auth: IAuth,
                private $uibModal: ng.ui.bootstrap.IModalService,
                private localization: ILocalizationService,
                private dialogService: IDialogService) {
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

    public forceUsername(): string {
        if (this._currentUser) {
            return this._currentUser.login;
        } else {
            return undefined;
        }
    }

    public forceDisplayname(): string {
        if (this._currentUser) {
            return this._currentUser.displayName;
        } else {
            return undefined;
        }
    }

    public getLoginMessage(): string {
        return this._loginMsg;
    }

    public logout(): ng.IPromise<any> {
        const defer = this.$q.defer();
        this.auth.logout(this._currentUser, false).then(() => defer.resolve());
        if (this._currentUser) {
            this._prevLogin = "";
        }
        this._currentUser = null;

        return defer.promise;
    }

    public login(username: string, password: string, overrideSession: boolean): ng.IPromise<any> {
        const defer = this.$q.defer();

        this.auth.login(username, password, overrideSession).then(
            (user) => {
                this._currentUser = user;
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
                this._currentUser = user;
                defer.resolve();

            },
            (error) => {
                defer.reject(error);
            });

        return defer.promise;
    }

    private onExpiredDefer: ng.IDeferred<any>;

    public onExpired(): ng.IPromise<any> {

        if (!this._isExpired && !this._loginDialogPromise) {
            this._isExpired = true;
            this._loginMsg = this.localization.get("Login_Session_Timeout");
            this._isForceSameUsername = true;
            return this.showLogin();
        }

        if (this._loginDialogPromise) {
            return this._loginDialogPromise;
        } else {
            this._loginDialogPromise = this.$q.resolve();
            return this._loginDialogPromise;
        }
    }

    public ensureAuthenticated(): ng.IPromise<any> {
        if (this._currentUser) {
            return this.$q.resolve();
        } else if (this._loginDialogPromise) {
            return this._loginDialogPromise;
        }

        this._loginMsg = this.localization.get("Login_Session_EnterCredentials");
        this._isForceSameUsername = false;
        if (SessionTokenHelper.hasSessionToken()) {
            return this.auth.getCurrentUser().then(user => {
                    this._currentUser = user;
                }
            ).finally(() => {
                if (this._currentUser) {
                    return this.$q.resolve();
                } else {
                    return this.showLogin();
                }
            });
        } else {
            return this.showLogin();
        }
    }

    private showLogin = (error?: Error): ng.IPromise<any> => {
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
                } else if (result.samlLogin) {
                    this._loginDialogPromise = this.dialogService
                        .confirm(this.localization.get("Login_Session_DuplicateSession_Verbose"), null, "nova-messaging nova-login-confirm")
                        .then(() => {
                            return this.loginWithSaml(true).then(
                                () => {
                                    this._isExpired = false;
                                    return this.$q.resolve();
                                },
                                (err) => {
                                    return this.showLogin(err);
                                });
                        })
                        .catch(() => {
                            return this.showLogin();
                        })
                        .finally(() => {
                            confirmationDialog = null;
                        });
                        return this._loginDialogPromise;
                } else if (result.userName && result.password) {
                    this._loginDialogPromise = this.dialogService
                        .confirm(this.localization.get("Login_Session_DuplicateSession_Verbose"), null, "nova-messaging nova-login-confirm")
                        .then(() => {
                            return this.login(result.userName, result.password, true).then(
                                () => {
                                    this._isExpired = false;
                                    return this.$q.resolve();
                                },
                                (err) => {
                                    return this.showLogin(err);
                                });
                        })
                        .catch(() => {
                            return this.showLogin();
                        })
                        .finally(() => {
                            confirmationDialog = null;
                        });
                        return this._loginDialogPromise;
                } else {
                    return this.showLogin();
                }
            } else {
                return this.showLogin();
            }
        }).finally(() => {
            this._loginDialogPromise = null;
        });
        return this._loginDialogPromise;
    };

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
