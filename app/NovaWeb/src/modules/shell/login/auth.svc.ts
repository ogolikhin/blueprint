import "angular";

export interface IUser {
    DisplayName: string;
    Login: string;
    IsFallbackAllowed: boolean;
    IsSso: boolean;
}

export interface IAuth {
    authenticated: ng.IPromise<boolean>;

    getCurrentUser(): ng.IPromise<IUser>;
}

export class AuthSvc implements IAuth {

    static $inject: [string] = ["$q", "$log", "$http"];
    constructor(private $q: ng.IQService, private $log: ng.ILogService, private $http: ng.IHttpService) {
    }

    public get authenticated(): ng.IPromise<boolean> {
        var defer = this.$q.defer<boolean>();

        this.getCurrentUser().then(
            (user: IUser) => {
                //TODO: remember current user
                defer.resolve(!!user);
            },
            (err) => {
                defer.resolve(false);
            }
        );

        return defer.promise;
    }

    public getCurrentUser(): ng.IPromise<IUser> {
        var defer = this.$q.defer<IUser>();

        var config = this.skipCommonInterceptor();
        config.headers = config.headers || {};
        config.headers["Session-Token"] = "AFD56943990C43B490F5B54C35AE0DBF"; //GET real token from localStorage
        this.$http.get<IUser>("/svc/adminstore/users/loginuser", config)
            .success((result: IUser) => {
                //SessionTokenHelper.updateTokenCookie();
                //this.onLogin(result);

                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: err ? err.Message : "Cannot get current user"
                };

                this.$log.error(`AuthSvc.getCurrentUser: ErrorCode:${error.statusCode} Msg:${error.message}`);

                defer.reject(error);
            });

        return defer.promise;
    }

    private skipCommonInterceptor(): ng.IRequestShortcutConfig {
        //TODO: ignore session token and unauthorized response processing for some Auth service calls
        return {};
    }
}