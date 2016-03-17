import "angular";
import {IAuth, IUser} from "./auth.svc";

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

            this._modalInstance.result.then((result) => {
                if (result) {
                    this._currentUser = result;
                    done.resolve();
                }
                else {
                    done.reject();
                }
            }).finally(() => {
                this._modalInstance = null;
            });
        }
    }
}

export class LoginCtrl {



    static $inject: [string] = ["$uibModalInstance", "auth", '$scope'];
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private auth: IAuth, private $scope: ng.IScope) {
        this.$scope["errorMsg"] = "Please enter your Username and Password";
    }

    public login(): void {
        this.auth.login(this.$scope.$eval('novaUsername'), this.$scope.$eval('novaPassword'), true).then(
            (user) => {
                this.removeClass("error", document.getElementById("message-error"));
                this.removeClass("error", document.getElementById("nova-username-div"));
                this.removeClass("error", document.getElementById("nova-password-div"));
                this.$uibModalInstance.close(user);
            },
            (error) => {
                if (error.errorCode === 2000) {
                    this.$scope["errorMsg"] = "Please enter a correct Username and Password";
                    this.addClass("error",document.getElementById("message-error"));
                    this.addClass("error",document.getElementById("nova-username-div"));
                    this.addClass("error",document.getElementById("nova-password-div"));
                } else if (error.errorCode === 2001) {
                    this.$scope["errorMsg"] = "Your account has been disabled. <br>Please contact your administrator.";
                    this.addClass("error", document.getElementById("message-error"));
                    this.removeClass("error", document.getElementById("nova-username-div"));
                    this.removeClass("error", document.getElementById("nova-password-div"));
                } else {
                    this.$scope["errorMsg"] = error.message;
                    this.addClass("error", document.getElementById("message-error"));
                    this.addClass("error", document.getElementById("nova-username-div"));
                    this.addClass("error", document.getElementById("nova-password-div"));
                }
                
            });
     
    }

    private addClass(className: string, element: any) {
        element.className = element.className.replace(className, "");
        element.className += " " + className;
    }

    private removeClass(className: string, element: any) {
        element.className = element.className.replace(" " + className, "");
    }
}
