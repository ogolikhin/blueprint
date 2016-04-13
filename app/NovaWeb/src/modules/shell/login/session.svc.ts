﻿import "angular";
import {ILocalizationService} from "../../core/localization";
import {IAuth, IUser} from "./auth.svc";
import {IConfigValueHelper} from "../../core/config.value.helper";
import {SimpleDialogCtrl, LoginCtrl, ILoginInfo} from "./login.ctrl";

export interface ISession {
    ensureAuthenticated(): ng.IPromise<any>;

    currentUser: IUser;
    lastError: Error;

    logout(): ng.IPromise<any>;

    login(username: string, password: string, overrideSession: boolean): ng.IPromise<any>;

    loginWithSaml(overrideSession: boolean): ng.IPromise<any>;

    resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any>;
}

export class SessionSvc implements ISession {

    static $inject: [string] = ["$q", "auth", "$uibModal", "localization"];
    constructor(private $q: ng.IQService, private auth: IAuth, private $uibModal: ng.ui.bootstrap.IModalService, private localization: ILocalizationService) {
    }

    private _modalInstance: ng.ui.bootstrap.IModalServiceInstance;

    private _currentUser: IUser;
    private _lastError: Error;
    //TODO investigate neccessity to save previous login (session expiration for saml)
    private _prevLogin: string;

    public get currentUser(): IUser {
        return this._currentUser;
    }

    public get lastError(): Error {
        return this._lastError;
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

    public ensureAuthenticated(): ng.IPromise<any> {
        if (this._currentUser || this._modalInstance) {
            return this.$q.resolve();
        }
        var defer = this.$q.defer();
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

    private createConfirmationDialog(): ng.ui.bootstrap.IModalServiceInstance {
        return this.$uibModal.open(<ng.ui.bootstrap.IModalSettings>{
            template: require("./../messaging/confirmation.dialog.html"),
            windowClass: "nova-messaging",
            controller: SimpleDialogCtrl,
            controllerAs: "ctrl",
            keyboard: false, // cannot Escape ))
            backdrop: false,
            bindToController: true
        });
    }

    private showLogin(done: ng.IDeferred<any>, error?: Error): void {
        if (error) {
            this._lastError = error;
        } else {
            this._lastError = undefined;
        }
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
                    if (result.loginSuccessful) {
                        done.resolve();
                    } else if (result.samlLogin) {
                        var confirmationDialog: ng.ui.bootstrap.IModalServiceInstance = this.createConfirmationDialog();
                        confirmationDialog.result.then((confirmed: boolean) => {
                            if (confirmed) {
                                this.loginWithSaml(true).then(
                                    () => {
                                        done.resolve();
                                    },
                                    (error) => {
                                        this.showLogin(done, error);
                                    });
                            } else {
                                this.showLogin(done);
                            }
                        }).finally(() => {
                            confirmationDialog = null;
                        });
                    } else if (result.userName && result.password) {
                        var confirmationDialog: ng.ui.bootstrap.IModalServiceInstance = this.createConfirmationDialog();
                        confirmationDialog.result.then((confirmed: boolean) => {
                            if (confirmed) {
                                this.login(result.userName, result.password, true).then(
                                    () => {
                                        done.resolve();
                                    },
                                    (error) => {
                                        this.showLogin(done, error);
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


