import "angular";
import {IAuth} from "./auth.svc";

export interface ILogin {
    ensureAuthenticated(): ng.IPromise<any>;
}

export class LoginSvc implements ILogin {

    static $inject: [string] = ["$q", "auth", "$uibModal"];
    constructor(private $q: ng.IQService, private auth: IAuth, private $uibModal: ng.ui.bootstrap.IModalService) {
    }

    private _modalInstance: ng.ui.bootstrap.IModalServiceInstance;

    public ensureAuthenticated(): ng.IPromise<any> {
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
                    done.resolve();
                }
                else {
                    done.reject();
                }
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
