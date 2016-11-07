import {SessionTokenHelper} from "./session.token.helper";
import {Helper} from "../../shared";
import {ISettingsService} from "../../core/configuration/settings";
import {ApplicationError} from "../../core/error/applicationError";
import {HttpStatusCode} from "../../core/http/http-status-code";
import {IHttpInterceptorConfig} from "../../core/http/http-interceptor-config";
import {ILocalizationService} from "../../core/localization/localizationService";

export interface IUser {
    id: number;
    displayName: string;
    login: string;
    isFallbackAllowed: boolean;
    isSso: boolean;
    licenseType: LicenseType;
}

export enum LicenseType {
    Viewer = 1,
    Collaborator = 2,
    Author = 3
}

export interface IAuth {
    getCurrentUser(): ng.IPromise<IUser>;

    login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser>;

    loginWithSaml(overrideSession: boolean, prevLogin: string): ng.IPromise<IUser>;

    logout(userInfo: IUser, skipSamlLogout: boolean): ng.IPromise<any>;

    resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any>;
}

export class AuthSvc implements IAuth {

    private samlRequestId = 0;
    private _loggedOut: boolean = false;


    static $inject: [string] = ["$q", "$log", "$http", "$window", "localization", "settings"];

    constructor(private $q: ng.IQService,
                private $log: ng.ILogService,
                private $http: ng.IHttpService,
                private $window: ng.IWindowService,
                private localization: ILocalizationService,
                private settings: ISettingsService) {
        // Nothing
    }

    public getCurrentUser(): ng.IPromise<IUser> {
        const defer = this.$q.defer<IUser>();
        const config = this.createRequestConfig();

        this.$http.get<IUser>("/svc/adminstore/users/loginuser", config)
            .then((result: ng.IHttpPromiseCallbackArg<IUser>) => {
                defer.resolve(result.data);
            }, (result: ng.IHttpPromiseCallbackArg<any>) => {

                result.data.message = result.data.message || this.localization.get("Login_Auth_CannotGetUser");
                if (this.settings.getBoolean("DisableWindowsIntegratedSignIn") === false && !this._loggedOut) {
                    this.$http.post<any>("/Login/WinLogin.aspx", "", config)
                        .then((winLoginResult: ng.IHttpPromiseCallbackArg<string>) => {
                            this.onTokenSuccess(winLoginResult.data, defer, false, "");
                        }, () => {
                            defer.reject(result.data);
                        });

                } else {
                    defer.reject(result.data);
                }
            });

        return defer.promise;
    }

    public login(userName: string, password: string, overrideSession: boolean): ng.IPromise<IUser> {
        const encUserName: string = userName ? AuthSvc.encode(userName) : "";
        const encPassword: string = password ? AuthSvc.encode(password) : "";

        const deferred = this.$q.defer<IUser>();


        this.$http.post<any>("/svc/adminstore/sessions/?login=" + encUserName + "&force=" + overrideSession,
                angular.toJson(encPassword), this.createRequestConfig())
            .then((result: ng.IHttpPromiseCallbackArg<string>) => {
                this.onTokenSuccess(result.data, deferred, false, "");
            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                result.data.message = this.getLoginErrorMessage(result.data);
                deferred.reject(result.data);

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

    public loginWithSaml(overrideSession: boolean = false, prevLogin: string): ng.IPromise<any> {
        const deferred = this.$q.defer<IUser>();

        const guid: string = Helper.UID;

        this.$window.name = guid;

        this.samlRequestId += 1;

        const absPath = this.getAppBaseUrl();

        const url = "/Login/SAMLHandler.ashx?action=relogin&id=" + this.samlRequestId + "&wname=" + guid + "&host=" + encodeURI(absPath);
        this.$window["notifyAuthenticationResult"] = (requestId: string, samlResponse: string): string => {
            if (requestId === this.samlRequestId.toString()) {
                this.$http.post("/svc/adminstore/sessions/sso?force=" + overrideSession, angular.toJson(samlResponse), this.createRequestConfig())
                    .then(
                        (result: ng.IHttpPromiseCallbackArg<string>) => {
                            this.onTokenSuccess(result.data, deferred, true, prevLogin);
                        }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                            result.data.message = this.localization.get("Login_Auth_LoginFailed");
                            deferred.reject(result.data);
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
            const logoutFinilizer: () => boolean = (!skipSamlLogout && userInfo && userInfo.isSso)
                ? () => {
                const url = "/Login/SAMLHandler.ashx?action=logout";
                this.$window.location.replace(url);
                return true;
            }
                : () => false;

            const deferred: ng.IDeferred<any> = this.$q.defer();
            this.pendingLogout = deferred.promise;
            this.$http.delete("/svc/adminstore/sessions", this.createRequestConfig())
                .finally(() => {
                    if (logoutFinilizer()) {
                        return;
                    }
                    this.pendingLogout = null;
                    SessionTokenHelper.clearSessionToken();
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

        return err.message ? err.message : this.localization.get("Login_Auth_LoginFailed");
    }

    private internalLogout(token: string): ng.IPromise<any> {
        const deferred: ng.IDeferred<any> = this.$q.defer();

        let requestConfig = this.createRequestConfig();
        requestConfig.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = token;

        this.$http.delete("/svc/adminstore/sessions", requestConfig)
            .then(() => deferred.resolve(), () => deferred.reject());

        return deferred.promise;
    }

    private onTokenSuccess(token: string, deferred: any, isSaml: boolean, prevLogin: string) {
        if (token) {
            this.verifyLicense(token)
                .then(() => {
                    SessionTokenHelper.setToken(token);
                    this.$http.get<IUser>("/svc/adminstore/users/loginuser", this.createRequestConfig())
                        .then((result: ng.IHttpPromiseCallbackArg<IUser>) => {
                            let user = result.data;

                            if (user.licenseType === LicenseType.Viewer) {
                                this.internalLogout(token).finally(() => {
                                    deferred.reject({message: this.localization.get("Login_Session_InvalidLicense")}); //TODO: Localize
                                });
                            } else if (isSaml && prevLogin && prevLogin !== user.login) {
                                this.internalLogout(token).finally(() => {
                                    deferred.reject({message: this.localization.get("Login_Auth_SamlContinueSessionWithOriginalUser")});
                                });
                            } else {
                                deferred.resolve(user);
                            }
                        }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                            deferred.reject(result.data);
                        });
                }, (err: any) => {
                    this.internalLogout(token);
                    deferred.reject(err);
                });
        } else {
            deferred.reject(new ApplicationError({
                statusCode: HttpStatusCode.ServerError,
                message: this.localization.get("Login_Auth_SessionTokenRetrievalFailed")
            }));
        }
    }

    private createRequestConfig(): ng.IRequestConfig {
        const config = <IHttpInterceptorConfig>{ignoreInterceptor: true};
        config.headers = {};
        //TODO: move the token injection somewhere more appropriate
        config.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = SessionTokenHelper.getSessionToken();
        return config;
    }

    private verifyLicense(token: string): ng.IPromise<any> {
        const deferred: ng.IDeferred<any> = this.$q.defer();
        let requestConfig = this.createRequestConfig();

        requestConfig.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = token;

        this.$http.post("/svc/shared/licenses/verify", "", requestConfig)
            .then(
                () => deferred.resolve(),
                (result: ng.IHttpPromiseCallbackArg<any>) => {
                    let error = {};
                    let statusCode = result.status;

                    if (statusCode === HttpStatusCode.NotFound) {
                        result.data.message = this.localization.get("Login_Auth_LicenseNotFound_Verbose");
                    } else if (statusCode === HttpStatusCode.Forbidden) {
                        result.data.message = this.localization.get("Login_Auth_LicenseLimitReached");
                    }

                    deferred.reject(result.data);
                });

        return deferred.promise;
    }

    public resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any> {
        const encUserName: string = login ? AuthSvc.encode(login) : "";
        const encOldPassword: string = oldPassword ? AuthSvc.encode(oldPassword) : "";
        const encNewPassword: string = newPassword ? AuthSvc.encode(newPassword) : "";

        const deferred = this.$q.defer<any>();

        this.$http.post<any>("/svc/adminstore/users/reset?login=" + encUserName,
            angular.toJson({OldPass: encOldPassword, NewPass: encNewPassword}), this.createRequestConfig())
            .then(
                () => deferred.resolve(),
                (result: ng.IHttpPromiseCallbackArg<any>) => {
                    result.data.message = this.getLoginErrorMessage(result.data);
                    deferred.reject(result.data);
                }
            );

        return deferred.promise;
    }

    private static key = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

    public static encode(input: string): string {
        let output = "";
        let chr1, chr2, chr3, enc1, enc2, enc3, enc4;
        let i = 0;

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
        let output = "";

        for (let n = 0; n < input.length; n++) {

            const c = input.charCodeAt(n);

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
