﻿import "angular";
import {IDialogService, IDialogSettings, BaseDialogController} from "../../shared/";
import {IAuth, IUser} from "./auth.svc";
import {SessionTokenHelper} from "./session.token.helper";
import {LoginCtrl, ILoginInfo, ILoginModalDialogData} from "./login.ctrl";
import {ILocalizationService} from "../../core/localization/localizationService";

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
    private _isLoginModalOpen: boolean;
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

        if (!this._isExpired) {
            this._isExpired = true;
            this.onExpiredDefer = this.$q.defer();
            this._loginMsg = this.localization.get("Login_Session_Timeout");
            this._isForceSameUsername = true;
            this.showLogin(this.onExpiredDefer);
        }

        return this.onExpiredDefer.promise;
    }

    public ensureAuthenticated(): ng.IPromise<any> {
        if (this._currentUser || this._isLoginModalOpen) {
            return this.$q.resolve();
        }
        const defer = this.$q.defer();
        this._loginMsg = this.localization.get("Login_Session_EnterCredentials");
        this._isForceSameUsername = false;
        if (SessionTokenHelper.hasSessionToken()) {
            this.auth.getCurrentUser().then(user => {
                    this._currentUser = user;
                }
            ).finally(() => {
                if (this._currentUser) {
                    defer.resolve();
                } else {
                    this.showLogin(defer);
                }
            });
        } else {
            this.showLogin(defer);
        }
        return defer.promise;
    }

    private showLogin = (done: ng.IDeferred<any>, error?: Error): void => {
        this.dialogService.open(<IDialogSettings>{
            template: require("./login.html"),
            css: "nova-login",
            controller: LoginCtrl,
            controllerAs: "ctrl",
            keyboard: false, // cannot Escape ))
            backdrop: false,
            bindToController: true
        })
            .then((result: ILoginInfo) => {
                this._isLoginModalOpen = true;
                if (result) {
                    let confirmationDialog: ng.ui.bootstrap.IModalServiceInstance;
                    if (result.loginSuccessful) {
                        this._isExpired = false;
                        done.resolve();
                    } else if (result.samlLogin) {
                        this.dialogService
                            .confirm(this.localization.get("Login_Session_DuplicateSession_Verbose"))
                            .then(() => {
                                this.loginWithSaml(true).then(
                                    () => {
                                        this._isExpired = false;
                                        done.resolve();
                                    },
                                    (err) => {
                                        this.showLogin(done, err);
                                    });
                            })
                            .catch(() => {
                                this.showLogin(done);
                            })
                            .finally(() => {
                                confirmationDialog = null;
                            });
                    } else if (result.userName && result.password) {
                        this.dialogService
                            .confirm(this.localization.get("Login_Session_DuplicateSession_Verbose"), null, "nova-messaging nova-login-confirm")
                            .then(() => {
                                this.login(result.userName, result.password, true).then(
                                    () => {
                                        this._isExpired = false;
                                        done.resolve();
                                    },
                                    (err) => {
                                        this.showLogin(done, err);
                                    });
                            })
                            .catch(() => {
                                this.showLogin(done);
                            })
                            .finally(() => {
                                confirmationDialog = null;
                            });
                    } else {
                        this.showLogin(done);
                    }
                } else {
                    this.showLogin(done);
                }
            })
            .finally(() => {
                this._isLoginModalOpen = false;
            });
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
