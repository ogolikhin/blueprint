import "angular";
import { ISession } from "../login/session.svc";
import { SessionTokenHelper } from "../login/session.token.helper";

export class HttpHandledErrorStatusCodes {
    public static get handledUnauthorizedStatus() {
        return 1401;
    }
}

export interface IHttpInterceptorConfig extends ng.IRequestConfig {
    ignoreInterceptor: boolean;
    dontRetry: boolean;
}

export class HttpErrorInterceptor {

    static $inject: [string] = ["$injector"];
    constructor(private $injector: ng.auto.IInjectorService) {
    }

    public responseError = (response: ng.IHttpPromiseCallbackArg<any>) => {
        var $q: ng.IQService = <ng.IQService>this.$injector.get("$q");
        var session: ISession = <ISession>this.$injector.get("session");

        var config = <IHttpInterceptorConfig>response.config;

        var deferred: ng.IDeferred<any> = $q.defer();

        if (config && (config.ignoreInterceptor)) {
            deferred.reject(response);
        } else if (response.status === 401) {            
            session.onExpired().then(
                () => {
                    if (config &&  !config.dontRetry) {
                        var $http = <ng.IHttpService>this.$injector.get("$http");
                        HttpErrorInterceptor.applyNewSessionToken(config);                    

                        config.dontRetry = true;

                        $http(config).then(retryResponse => deferred.resolve(retryResponse), retryResponse => deferred.reject(retryResponse));
                    } else {
                        response.status = HttpHandledErrorStatusCodes.handledUnauthorizedStatus;
                        deferred.reject(response);
                    }
                },
                () => deferred.reject(response)
            );
        } else {
            deferred.reject(response);
        }

        return deferred.promise;
    };

    private static applyNewSessionToken(config: ng.IRequestConfig) {
        var token = SessionTokenHelper.getSessionToken();
        if (token) {
            if (!config.headers) {
                config.headers = {};
            }
            config.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = token;
        }
    }
}
