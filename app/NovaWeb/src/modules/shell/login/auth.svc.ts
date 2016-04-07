import "angular";
import {SessionTokenHelper} from "./session.token.helper";
import {ILocalizationService} from "../../core/localization";
import {IConfigValueHelper} from "../../core/config.value.helper";

export interface IUser {
    DisplayName: string;
    Login: string;
    IsFallbackAllowed: boolean;
    IsSso: boolean;
}

export interface IAuth {
    getCurrentUser(): ng.IPromise<IUser>;

    login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser>;

    loginWithSaml(overrideSession: boolean, prevLogin: string): ng.IPromise<IUser>;

    logout(userInfo: IUser, skipSamlLogout: boolean): ng.IPromise<any>;
}

export interface IHttpInterceptorConfig extends ng.IRequestConfig {
    ignoreInterceptor: boolean;
}

export class AuthSvc implements IAuth {

    private samlRequestId = 0;
    private _loggedOut: boolean = false;


    static $inject: [string] = ["$q", "$log", "$http", "$window", "localization", "configValueHelper"];
    constructor(private $q: ng.IQService, private $log: ng.ILogService, private $http: ng.IHttpService, private $window: ng.IWindowService, private localization: ILocalizationService, private configValueHelper: IConfigValueHelper) {
    }

    public getCurrentUser(): ng.IPromise<IUser> {
        var defer = this.$q.defer<IUser>();
        var config = this.createRequestConfig();

        this.$http.get<IUser>("/svc/adminstore/users/loginuser", config)
            .success((result: IUser) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: err ? err.Message : this.localization.get("Login_Auth_CannotGetUser")
                };
                if (this.configValueHelper.getBooleanValue("DisableWindowsIntegratedSignIn") === false && !this._loggedOut) {
                    this.$http.post<any>("/Login/WinLogin.aspx", "", config)
                        .success((token: string) => {
                            this.onTokenSuccess(token, defer, false, "");
                        }).error((err) => {
                            defer.reject(error);
                        });

                } else {
                    defer.reject(error);
                }
            });

        return defer.promise;
    }

    public login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser> {
        var encUserName: string = userName ? AuthSvc.encode(userName) : "";
        var encPassword: string = password ? AuthSvc.encode(password) : "";

        var deferred = this.$q.defer<IUser>();

        this.$http.post<any>("/svc/adminstore/sessions/?login=" + encUserName + "&force=" + overrideSession, angular.toJson(encPassword), this.createRequestConfig())
            .success((token: string) => {
                this.onTokenSuccess(token, deferred, false, "");
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: this.getLoginErrorMessage(err),
                    errorCode: err.ErrorCode
                };
                deferred.reject(error);

            });

        return deferred.promise;
    }

    private getAppBaseUrl(): string {
        const location = this.$window.location;

        let origin: string = (<any>location).origin;
        if (!origin) {
            origin = location.protocol + "//" + location.hostname + (location.port ? ":" + location.port : "");
        }

        return origin + "/";
    }


    private generateGuid(): string {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
            /* tslint:disable:no-bitwise */
            var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
            /* tslint:enable:no-bitwise */
            return v.toString(16);
        });
    }

    public loginWithSaml(overrideSession: boolean = false, prevLogin: string): ng.IPromise<any> {
        var deferred = this.$q.defer<IUser>();

        var guid: string = this.generateGuid();

        this.$window.name = guid;

        this.samlRequestId += 1;

        var absPath = this.getAppBaseUrl();

        var url = "/Login/SAMLHandler.ashx?action=relogin&id=" + this.samlRequestId + "&wname=" + guid + "&host=" + encodeURI(absPath);
        this.$window["notifyAuthenticationResult"] = (requestId: string, samlResponse: string): string => {
            if (requestId === this.samlRequestId.toString()) {
                this.$http.post("/svc/adminstore/sessions/sso?force=" + overrideSession, angular.toJson(samlResponse), this.createRequestConfig())
                    .success(
                    (token: string) => {
                        this.onTokenSuccess(token, deferred, true, prevLogin);
                    })
                    .error((err: any, statusCode: number) => {
                        var error = {
                            statusCode: statusCode,
                            message: this.getLoginErrorMessage(err)
                        };
                        deferred.reject(error);
                    });


                return null;
            } else {
                return this.localization.get("Login_Auth_IncorrectRequestId");
            }
        };

        this.$window.open(url, "_blank");

        return deferred.promise;
    }

    private pendingLogout: ng.IPromise<any>;

    public logout(userInfo: IUser, skipSamlLogout: boolean = false): ng.IPromise<any> {
        if (!this.pendingLogout) {
            var logoutFinilizer: () => boolean = (!skipSamlLogout && userInfo && userInfo.IsSso)
                ? () => {
                    var url = "/Login/SAMLHandler.ashx?action=logout";
                    this.$window.location.replace(url);
                    return true;
                }
                : () => false;

            var deferred: ng.IDeferred<any> = this.$q.defer();
            this.pendingLogout = deferred.promise;
            this.$http.delete("/svc/adminstore/sessions", this.createRequestConfig())
                .finally(() => {
                    if (logoutFinilizer()) {
                        return;
                    }
                    this.pendingLogout = null;
                    SessionTokenHelper.setToken(null);
                    this._loggedOut = true;
                    deferred.resolve();
                });
        }

        return this.pendingLogout;
    }

    public getLoginErrorMessage(err: any): string {
        if (!err) {
            return "";
        }

        return err.Message ? err.Message : this.localization.get("Login_Auth_LoginFailed"); // TODO: generic message
    }

    private internalLogout(token: string): ng.IPromise<any> {
        var deferred: ng.IDeferred<any> = this.$q.defer();

        let requestConfig = this.createRequestConfig();
        requestConfig.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = token;

        this.$http.delete("/svc/adminstore/sessions", requestConfig)
            .success(() => deferred.resolve())
            .error(() => deferred.reject());

        return deferred.promise;
    }

    private onTokenSuccess(token: string, deferred: any, isSaml: boolean, prevLogin: string) {
        if (token) {
            this.verifyLicense(token)
                .then(() => {
                    SessionTokenHelper.setToken(token);
                    this.$http.get<IUser>("/svc/adminstore/users/loginuser", this.createRequestConfig())
                        .success((user: IUser) => {
                            if (isSaml && prevLogin && prevLogin !== user.Login) {
                                this.internalLogout(token).finally(() => {
                                    deferred.reject({ message: this.localization.get("Login_Auth_SamlContinueSessionWithOriginalUser") });
                                });
                            } else {
                                //this.onLogin(user);
                            deferred.resolve(user);
                            }
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
                        deferred.reject({ message: this.localization.get("Login_Auth_LicenseVerificationFailed") });
                    }
                });
        } else {
            deferred.reject({ statusCode: 500, message: this.localization.get("Login_Auth_SessionTokenRetrievalFailed") });
        }
    }

    private createRequestConfig(): ng.IRequestConfig {
        var config = <IHttpInterceptorConfig>{ ignoreInterceptor: true };
        config.headers = {};
        //TODO: move the token injection somewhere more appropriate
        config.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = SessionTokenHelper.getSessionToken();
        return config;
    }

    private verifyLicense(token: string): ng.IPromise<any> {
        var deferred: ng.IDeferred<any> = this.$q.defer();
        let requestConfig = this.createRequestConfig();

        requestConfig.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = token;

        this.$http.post("/svc/shared/licenses/verify", "", requestConfig)
            .success(() => deferred.resolve())
            .error((err: any, statusCode: number) => {
                var msg = null;
                if (statusCode === 404) { // NotFound
                    msg = this.localization.get("Login_Auth_LicenseNotFound_Verbose");

                } else if (statusCode === 403) { // Forbidden
                    msg = this.localization.get("Login_Auth_LicenseLimitReached");
                }

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

            /* tslint:disable:no-bitwise */
            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;
            /* tslint:enable:no-bitwise */

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
            } else if ((c > 127) && (c < 2048)) {
                /* tslint:disable:no-bitwise */
                output += String.fromCharCode((c >> 6) | 192);
                output += String.fromCharCode((c & 63) | 128);
                /* tslint:enable:no-bitwise */
            } else {
                /* tslint:disable:no-bitwise */
                output += String.fromCharCode((c >> 12) | 224);
                output += String.fromCharCode(((c >> 6) & 63) | 128);
                output += String.fromCharCode((c & 63) | 128);
                /* tslint:enable:no-bitwise */
            }

        }

        return output;
    }
}