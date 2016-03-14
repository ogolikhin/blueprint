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
        this._currentUser = null;

        return this.$q.resolve();
    }

    public ensureAuthenticated(): ng.IPromise<any> {        
        if (this._currentUser) {
            return this.$q.resolve();
        }

        var defer = this.$q.defer();

        this.auth.authenticated.then(
            (authenticated) => authenticated ? defer.resolve() : this.showLogin(defer),
            () => this.showLogin(defer)
        );

        return defer.promise;
    }

    private showLogin(done: ng.IDeferred<any>): void {
        if (!this._modalInstance) {
            this._modalInstance = this.$uibModal.open(<ng.ui.bootstrap.IModalSettings>{
                template: require("./login.html"),
                controller: LoginCtrl,
                controllerAs: "ctrl",
                keyboard: false, // cannot Escape ))
                backdrop: false,
                bindToController: true
            });

            this._modalInstance.result.then((result) => {
                if (result) {
                    //TEMP: just to test
                    this._currentUser = <IUser>{
                        DisplayName: "Default Instance Admin"
                    };

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

    static $inject: [string] = ["$uibModalInstance", "auth"];
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private auth: IAuth) {
    }

    public login(): void {
        this.$uibModalInstance.close(true);
    }
}
