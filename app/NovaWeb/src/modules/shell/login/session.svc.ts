﻿import "angular";
import {ILocalizationService} from "../../core/localization";
import {IAuth, IUser} from "./auth.svc";
//import {IConfigValueHelper} from "../../core/config.value.helper";
import {LoginCtrl, ILoginInfo} from "./login.ctrl";
import {IDialogService} from "../../services/dialog.svc";
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
    constructor(
        private $q: ng.IQService,
        private auth: IAuth,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private localization: ILocalizationService,
        private dialogService: IDialogService) {
    }

    private _modalInstance: ng.ui.bootstrap.IModalServiceInstance;

    private _currentUser: IUser;
    private _loginMsg: string;
    private _prevLogin: string;
    private _isExpired: boolean;
    private _isForceSameUsername: boolean;

    public get currentUser(): IUser {
        return this._currentUser;
    }

    public forceUsername(): string {
        if (this._currentUser) {
            return this._currentUser.Login;
        } else {
            return undefined;
        }
    }

    public getLoginMessage(): string {
        return this._loginMsg;
    }

    public logout(): ng.IPromise<any> {
        var defer = this.$q.defer();
        this.auth.logout(this._currentUser, false).then(() => defer.resolve());
        if (this._currentUser) {
            this._prevLogin = "";
        }
        this._currentUser = null;

        return defer.promise;
    }

    public login(username: string, password: string, overrideSession: boolean): ng.IPromise<any> {
        var defer = this.$q.defer();

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
        var defer = this.$q.defer();

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

    public onExpired(): ng.IPromise<any> {
        var defer = this.$q.defer();
        if (!this._isExpired) {
            this._isExpired = true;
            this._loginMsg = this.localization.get("Login_Session_Timeout");
            this._isForceSameUsername = true;
            this.showLogin(defer);
        }
        defer.resolve();
        return defer.promise;
    }

    public ensureAuthenticated(): ng.IPromise<any> {
        if (this._currentUser || this._modalInstance) {
            return this.$q.resolve();
        }
        var defer = this.$q.defer();
        this._loginMsg = this.localization.get("Login_Session_EnterCredentials");
        this._isForceSameUsername = false;
        this.auth.getCurrentUser().then(
            (result: IUser) => {
                if (result) {
                    this._currentUser = result;
                    defer.resolve();
                } else {
                    this.showLogin(defer);
                }
            },
            () => this.showLogin(defer)
        );
        return defer.promise;
    }

    private showLogin(done: ng.IDeferred<any>, error?: Error): void {
        if (!this._modalInstance) {
            this._modalInstance = this.$uibModal.open(<ng.ui.bootstrap.IModalSettings>{
                template: require("./login.html"),
                windowClass: "nova-login",
                controller: LoginCtrl,
                controllerAs: "ctrl",
                keyboard: false, // cannot Escape ))
                backdrop: false,
                bindToController: true
            });

            this._modalInstance.result.then((result: ILoginInfo) => {

                if (result) {
                    var confirmationDialog: ng.ui.bootstrap.IModalServiceInstance;
                    if (result.loginSuccessful) {
                        this._isExpired = false;
                        done.resolve();
                    } else if (result.samlLogin) {
                        this.dialogService
                            .confirm(this.localization.get("Login_Session_DuplicateSession_Verbose"))
                            .then((confirmed: boolean) => {
                            if (confirmed) {
                                this.loginWithSaml(true).then(
                                    () => {
                                        this._isExpired = false;
                                        done.resolve();
                                    },
                                    (err) => {
                                        this.showLogin(done, err);
                                    });
                            } else {
                                this.showLogin(done);
                            }
                        }).finally(() => {
                            confirmationDialog = null;
                        });
                    } else if (result.userName && result.password) {
                        this.dialogService.confirm(this.localization.get("Login_Session_DuplicateSession_Verbose")).then((confirmed: boolean) => {
                            if (confirmed) {
                                this.login(result.userName, result.password, true).then(
                                    () => {
                                        this._isExpired = false;
                                        done.resolve();
                                    },
                                    (err) => {
                                        this.showLogin(done, err);
                                    });
                            } else {
                                this.showLogin(done);
                            }
                        }).finally(() => {
                            confirmationDialog = null;
                        });
                    } else {
                        this.showLogin(done);
                    }
                } else {
                    this.showLogin(done);
                }
            }).finally(() => {
                this._modalInstance = null;
            });
        }
    }

    public resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any> {
        var defer = this.$q.defer();

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


