import "angular";
import {IAuth, IUser} from "./auth.svc";
import {ConfirmationDialogCtrl} from "./../messaging/confirmation.dialog.ctrl";

export interface ISession {
    ensureAuthenticated(): ng.IPromise<any>;

    currentUser: IUser;

    logout(): ng.IPromise<any>;
}

export class SessionSvc implements ISession {

    static $inject: [string] = ["$q", "auth", "$uibModal"];
    constructor(private $q: ng.IQService, private auth: IAuth, private $uibModal: ng.ui.bootstrap.IModalService) {
    }

    private _modalInstance: ng.ui.bootstrap.IModalServiceInstance;

    private _currentUser: IUser;
    public get currentUser(): IUser {
        return this._currentUser;
    }

    public logout(): ng.IPromise<any> {
        var defer = this.$q.defer();
        this.auth.logout(this._currentUser, false).then(() => defer.resolve());
        this._currentUser = null;

        return defer.promise;
    }

    public ensureAuthenticated(): ng.IPromise<any> {
        if (this._currentUser) {
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

    private showLogin(done: ng.IDeferred<any>): void {
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
                    if (result.userInfo) {
                        this._currentUser = result.userInfo;
                        done.resolve();
                    } else if (result.userName && result.password) {
                        var confirmationDialog: ng.ui.bootstrap.IModalServiceInstance;
                        confirmationDialog = this.$uibModal.open(<ng.ui.bootstrap.IModalSettings>{
                            template: require("./../messaging/confirmation.dialog.html"),
                            windowClass: "nova-messaging",
                            controller: SimpleDialogCtrl,
                            controllerAs: "ctrl",
                            keyboard: false, // cannot Escape ))
                            backdrop: false,
                            bindToController: true
                        });
                        confirmationDialog.result.then((confirmed: boolean) => {
                            if (confirmed) {
                                this.auth.login(result.userName, result.password, true).then(
                                    (user) => {
                                        this._currentUser = user;
                                        done.resolve();
                                    },
                                    (error) => {
                                        this.showLogin(done);
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
                }
                else {
                    this.showLogin(done);
                }
            }).finally(() => {
                this._modalInstance = null;
            });
        }
    }
}

export class SimpleDialogCtrl extends ConfirmationDialogCtrl{
    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        super($uibModalInstance);
        this.acceptButtonName = "Yes";
        this.cancelButtonName = "No";
        this.msg = "This user is already logged into Blueprint in another browser/session.<br><br>Do you want to override the previous session?";
    }
}

export class ILoginInfo {
    public userName: string;
    public password: string;
    public userInfo: IUser;
}

export class LoginCtrl {
    public labelError: boolean;
    public fieldError: boolean;

    public isInLoginForm: boolean;
    public errorMsg: string;
    public novaUsername: string;
    public novaPassword: string;

    public enableForgetPasswordScreen: boolean;
    public isInForgetPasswordScreen: boolean;
    public forgetPasswordScreenError: boolean;
    public forgetPasswordScreenMessage: string;
    public forgetPasswordScreenUsername: string;

    public enableChangePasswordScreen: boolean;
    public isInChangePasswordScreen: boolean;
    public changePasswordScreenError: boolean;
    public changePasswordScreenMessage: string;
    public changePasswordCurrentPasswordError: boolean; //if the user doesn't put the correct current password
    public changePasswordNewPasswordError: boolean; //if the new password doesn't satisfy the security criteria
    public changePasswordConfirmPasswordError: boolean; //if new password and confirm password don't match

    public enableSAMLScreen: boolean;
    public isInSAMLScreen: boolean;
    public SAMLScreenMessage: string;

    static $inject: [string] = ["$uibModalInstance", "auth", "$timeout"];
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private auth: IAuth, private $timeout: ng.ITimeoutService) {
        this.isInLoginForm = true;
        this.errorMsg = "Please enter your Username and Password";

        this.enableForgetPasswordScreen = false;
        this.isInForgetPasswordScreen = this.enableForgetPasswordScreen;
        this.forgetPasswordScreenMessage = "Please enter your Username";

        this.enableChangePasswordScreen = false;
        this.isInChangePasswordScreen = this.enableChangePasswordScreen;
        this.changePasswordScreenMessage = "Your Password has expired. Please change your Password below.";

        this.enableSAMLScreen = true;
        this.isInSAMLScreen = this.enableSAMLScreen;
        this.SAMLScreenMessage = "Please authenticate using your corporate credentials in the popup window that has opened. If you do not see the window, please ensure your popup blocker is disabled and then click the Retry button.<br><br>You will be automatically logged in after you are authenticated.";
    }

    public goToForgetPasswordScreen(): void {
        this.isInLoginForm = false;
        if(this.enableSAMLScreen) this.isInSAMLScreen = false;
        if(this.enableChangePasswordScreen) this.isInChangePasswordScreen = false;

        this.forgetPasswordScreenError = false;
        this.forgetPasswordScreenUsername = this.novaUsername;
        this.isInForgetPasswordScreen = true;
    }

    public goToChangePasswordScreen(): void {
        this.isInLoginForm = false;
        if(this.enableForgetPasswordScreen) this.isInForgetPasswordScreen = false;
        if(this.enableSAMLScreen) this.isInSAMLScreen = false;

        this.changePasswordScreenError = false;
        this.isInChangePasswordScreen = true;
    }

    public goToSAMLScreen(): void {
        this.isInLoginForm = false;
        if(this.enableForgetPasswordScreen) this.isInForgetPasswordScreen = false;
        if(this.enableChangePasswordScreen) this.isInChangePasswordScreen = false;

        this.isInSAMLScreen = true;
    }

    public goToLoginScreen(): void {
        this.isInLoginForm = true;
        this.$timeout(()=> {
            this.isInForgetPasswordScreen = this.enableForgetPasswordScreen;
            this.isInChangePasswordScreen = this.enableChangePasswordScreen;
            this.isInSAMLScreen = this.enableSAMLScreen;
        }, 500); // I need to reset the other panels after transitioning back to the login form
    }

    public changePassword(): void {
        // TODO: back-end not ready yet
    }

    public resetPassword(): void {
        // TODO: back-end not ready yet
    }

    public login(): void {
        this.auth.login(this.novaUsername, this.novaPassword, false).then(
            (user: IUser) => {
                this.labelError = false;
                this.fieldError = false;
                var result: ILoginInfo = new ILoginInfo();
                result.userInfo = user;

                this.$uibModalInstance.close(result);
            },
            (error) => {
                if (error.statusCode === 401) {
                    if (error.errorCode === 2000) {
                        this.errorMsg = "Please enter a correct Username and Password";
                        this.labelError = true;
                        this.fieldError = true;
                    } else if (error.errorCode === 2001) {
                        this.errorMsg = "Your account has been disabled. <br>Please contact your Administrator.";
                        this.labelError = true;
                        this.fieldError = false;
                    } else if (error.errorCode === 2002) {
                        this.errorMsg = "Your Password has expired.";
                        this.labelError = true;
                        this.fieldError = false;
                        this.enableChangePasswordScreen = true;
                        this.isInChangePasswordScreen = this.enableChangePasswordScreen;
                    } else {
                        this.errorMsg = error.message;
                        this.labelError = true;
                        this.fieldError = true;
                    }
                } else if (error.statusCode === 409) {
                    this.labelError = false;
                    this.fieldError = false;
                    var result: ILoginInfo = new ILoginInfo();
                    result.userName = this.novaUsername;
                    result.password = this.novaPassword;

                    this.$uibModalInstance.close(result);
                } else {
                    this.errorMsg = error.message;
                    this.labelError = true;
                    this.fieldError = true;
                }
            });
    }
}
