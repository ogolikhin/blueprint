import "angular";
import {SessionTokenHelper} from "./session.token.helper";

export interface IUser {
    DisplayName: string;
    Login: string;
    IsFallbackAllowed: boolean;
    IsSso: boolean;
}

export class UserInfo implements IUser {
    DisplayName: string;
    Login: string;
    IsFallbackAllowed: boolean;
    IsSso: boolean;

    constructor(public displayName: string, public login: string, public IsSSo: boolean, public allowFallback: boolean) {
        this.DisplayName = displayName;
        this.Login = login;
        this.IsFallbackAllowed = allowFallback;
        this.IsSso = IsSSo;
    }
}

export interface IAuth {
    authenticated: ng.IPromise<boolean>;

    getCurrentUser(): ng.IPromise<IUser>;

    login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser>;

    logout(skipSamlLogout: boolean): ng.IPromise<UserInfo>;
}

export interface IHttpInterceptorConfig extends ng.IRequestConfig {
    ignoreInterceptor: boolean;
}

export class AuthSvc implements IAuth {

    static $inject: [string] = ["$q", "$log", "$http"];
    constructor(private $q: ng.IQService, private $log: ng.ILogService, private $http: ng.IHttpService) {
    }

    public userInfo: IUser;

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

        var token = SessionTokenHelper.getSessionToken();
        config.headers["Session-Token"] = token;

        this.$http.get<IUser>("/svc/adminstore/users/loginuser", config)
            .success((result: IUser) => {
                this.onLogin(result);
      
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

    public login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser> {
        var deferred = this.$q.defer<IUser>();

        this.$http.post<any>("/svc/adminstore/sessions/?login=" + AuthSvc.encode(userName) + "&force=" + overrideSession, angular.toJson(AuthSvc.encode(password)), this.createRequestConfig())
            .success((token: string) => {
                this.onTokenSuccess(token, deferred, false);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: this.getLoginErrorMessage(err)
                };
                deferred.reject(error);

            });

        return deferred.promise;
    }

    private pendingLogout: ng.IPromise<any>;

    public logout(skipSamlLogout: boolean = false): ng.IPromise<UserInfo> {
        if (!this.pendingLogout) {
            var logoutFinilizer: () => boolean = (!skipSamlLogout && this.userInfo && this.userInfo.IsSso)
                ? () => {
                    var url = "/Login/SAMLHandler.ashx?action=logout";
                    //this.$window.location.replace(url);
                    return true;
                }
                : () => false;

            var deferred: ng.IDeferred<any> = this.$q.defer();
            this.pendingLogout = deferred.promise;
            console.debug(SessionTokenHelper.getSessionToken());
            this.$http.delete("/svc/adminstore/sessions", this.createRequestConfig())
                .finally(() => {
                    if (logoutFinilizer()) {
                        return;
                    }
                    var userInfo = this.userInfo;
                    this.pendingLogout = null;
                    this.onLogout();
                    SessionTokenHelper.setToken(null);
                    deferred.resolve(userInfo);
                });
        }

        return this.pendingLogout;
    }

    public getLoginErrorMessage(err: any): string {
        if (!err)
            return "";

        return err.Message ? err.Message : "Login Failed"; // TODO: generic message
    }

    private onTokenSuccess(token: string, deferred: any, isSaml: boolean) {
        if (token) {
            this.verifyLicense(token)
                .then(() => {
                    SessionTokenHelper.setToken(token);
                    this.$http.get<IUser>("/svc/adminstore/users/loginuser", this.createRequestConfig())
                        .success((user: IUser) => {
                            /*if (isSaml && this.prevLogin && this.prevLogin !== user.Login) {
                                this.internalLogout(token).finally(() => {
                                    deferred.reject(<IHttpError>{ message: "To continue your session, please login with the same user that the session was started with" });
                                });
                            } else {*/
                                this.onLogin(user);
                                deferred.resolve(this.userInfo);
                            //}
                        }).error((err: any, statusCode: number) => {
                            var error = {
                                statusCode: statusCode,
                                message: err ? err.Message : ""
                            };
                            deferred.reject(error);
                        });
                }, (msg: string) => {
                    if (msg) {
                        deferred.reject({ message: msg });
                    } else {
                        deferred.reject({ message: "Cannot verify license" });
                    }
                });
        } else {
            deferred.reject({ statusCode: 500, message: "Cannot get Sessin Token" });
        }
    }

    private createRequestConfig(): ng.IRequestConfig {
        var config = <IHttpInterceptorConfig>{ ignoreInterceptor: true };
        config.headers = {};
        config.headers["Session-Token"] = SessionTokenHelper.getSessionToken();
        return config;
    }

    private verifyLicense(token: string): ng.IPromise<any> {
        var deferred: ng.IDeferred<any> = this.$q.defer();
        let requestConfig = this.createRequestConfig();
        if (!requestConfig.headers) {
            requestConfig.headers = {};
        }
        requestConfig.headers["Session-Token"] = token;

        this.$http.post("/svc/shared/licenses/verify", "", requestConfig)
            .success(() => deferred.resolve())
            .error((err: any, statusCode: number) => {
                var msg = null;
                /*if (statusCode === 404) { // NotFound
                    msg = LabelsUtil.getValue(this.$rootScope, "ZeroLicenseMessage",
                        "No licenses found or Blueprint is using an invalid server license. Please contact your Blueprint administrator");

                } else if (statusCode === 403) { // Forbidden
                    msg = LabelsUtil.getValue(this.$rootScope, "LicenseLimitReachedMessage",
                        "The maximum concurrent license limit has been reached. Please contact your Blueprint Administrator.");
                }*/

                deferred.reject(msg);
            });

        return deferred.promise;
    }

    private static key = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

    public static encode(input: string): string {
        var output = "";
        var chr1, chr2, chr3, enc1, enc2, enc3, enc4;
        var i = 0;

        input = AuthSvc.utf8Encode(input);

        while (i < input.length) {
            chr1 = input.charCodeAt(i++);
            chr2 = input.charCodeAt(i++);
            chr3 = input.charCodeAt(i++);

            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;

            if (isNaN(chr2)) {
                enc3 = enc4 = 64;
            } else if (isNaN(chr3)) {
                enc4 = 64;
            }

            output = output +
                AuthSvc.key.charAt(enc1) + AuthSvc.key.charAt(enc2) +
                AuthSvc.key.charAt(enc3) + AuthSvc.key.charAt(enc4);
        }

        return output;
    }

    private static utf8Encode(input: string): string {
        input = input.replace(/\r\n/g, "\n");
        var output = "";

        for (var n = 0; n < input.length; n++) {

            var c = input.charCodeAt(n);

            if (c < 128) {
                output += String.fromCharCode(c);
            }
            else if ((c > 127) && (c < 2048)) {
                output += String.fromCharCode((c >> 6) | 192);
                output += String.fromCharCode((c & 63) | 128);
            }
            else {
                output += String.fromCharCode((c >> 12) | 224);
                output += String.fromCharCode(((c >> 6) & 63) | 128);
                output += String.fromCharCode((c & 63) | 128);
            }

        }

        return output;
    }

    private onLogin(result: IUser): void {
        this.userInfo = new UserInfo(result.DisplayName, result.Login, result.IsSso, result.IsFallbackAllowed);

        /*
        this.$rootScope.$broadcast("serviceLoginEvent");

        this.retryHttpBuffer.retryAll((config: ng.IRequestConfig) => {
            if (config.headers && config.headers["Session-Token"]) {
                var token = SessionTokenHelper.getSessionToken();
                config.headers["Session-Token"] = token;
            }
            return config;
        });*/
    }

    private onLogout(): void {
        /*if (this.userInfo) {
            this.prevLogin = this.userInfo.Login;
        }*/

        this.userInfo = null;
        //this.$rootScope.$broadcast("serviceLogoutEvent");
    }

    private skipCommonInterceptor(): ng.IRequestShortcutConfig {
        //TODO: ignore session token and unauthorized response processing for some Auth service calls
        return {};
    }
}