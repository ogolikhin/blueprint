import "angular";
import { ISession } from "../login/session.svc";
import { SessionTokenHelper } from "../login/session.token.helper";
import { IMessageService, IHttpInterceptorConfig } from "../../core";


export enum HttpErrorStatusCodes {
    Unavailable = -1,
    Succsess = 200,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    ServerError = 500
}

export class HttpErrorInterceptor {

    static $inject: [string] = ["$injector"];
    constructor(private $injector: ng.auto.IInjectorService) {
    }

    public responseError = (response: ng.IHttpPromiseCallbackArg<any>) => {
        let $q = this.$injector.get("$q") as ng.IQService;
        let $session = this.$injector.get("session") as ISession;
        let $message = this.$injector.get("messageService") as IMessageService;
        let $log = this.$injector.get("$log") as ng.ILogService;

        let config = (response.config || {}) as IHttpInterceptorConfig;

        var deferred: ng.IDeferred<any> = $q.defer();

        if (config.ignoreInterceptor) {
            deferred.reject(response);
        } else if (response.status === HttpErrorStatusCodes.Unavailable) {
            if (!this.canceledByUser(config)) {
                $message.addError("HttpError_ServiceUnavailable"); // Service is unavailable
            }
            deferred.reject();
        } else if (response.status === HttpErrorStatusCodes.Unauthorized) {            
            $session.onExpired().then(
                () => {
                    if (!config.dontRetry) {
                        var $http = <ng.IHttpService>this.$injector.get("$http");
                        HttpErrorInterceptor.applyNewSessionToken(config);                    

                        config.dontRetry = true;

                        $http(config).then(retryResponse => deferred.resolve(retryResponse), retryResponse => deferred.reject(retryResponse));
                    } else {
                        response.status = HttpErrorStatusCodes.Unauthorized;
                        deferred.reject(response);
                    }
                },
                () => deferred.reject(response)
            );
        } else if (response.status === HttpErrorStatusCodes.Forbidden && !config.dontHandle) {
            $message.addError("HttpError_Forbidden"); //Forbidden. The user does not have permissions for the artifact
            //here we need to reject with none object passed in, means that the error has been handled
            deferred.reject();

        } else if (response.status === HttpErrorStatusCodes.ServerError && !config.dontHandle) {
            $message.addError("HttpError_InternalServer"); //Internal Server Error. An error occurred.
            //here we need to reject with none object passed in, means that the error has been handled
            deferred.reject();
        
        } else {
            $log.error(response.data);
            deferred.reject(response);
        }

        return deferred.promise;
    };

    private canceledByUser(config: IHttpInterceptorConfig): boolean {
        //handle edge-case for cancelled request
        if (!config) {
            return false;
        }
        let promise = config.timeout as ng.IPromise<any>;
        if (promise) {
            if (promise["$$state"] && promise["$$state"].status === 1) {
                //request canceled by user
                return true;
            }
        }
        return false;
    }

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
