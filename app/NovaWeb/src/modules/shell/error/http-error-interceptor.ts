import "angular";
import { ISession } from "../login/session.svc";
import { SessionTokenHelper } from "../login/session.token.helper";
import { IMessageService,} from "../../core";
import { } from "../../shared";

export class HttpHandledErrorStatusCodes {
    public static get handledUnauthorizedStatus() {
        return 1401;
    }
}

export enum HttpErrorStatusCodes {
    Inaccessible = -1,
    Unauthorized = 401,
    UnauthorizedHandeled = 1401,
    Forbidden = 403,
    NotFound = 404,
    ServerError = 500
}

export interface IHttpInterceptorConfig extends ng.IRequestConfig {
    ignoreInterceptor: boolean;
    dontRetry: boolean;
}

export class HttpErrorInterceptor {

    static $inject: [string] = ["$injector"];
    constructor(private $injector: ng.auto.IInjectorService) {
    }

    //private $q: ng.IQService;
    //private session: ISession;
    //private messageService: IMessageService;
    //private $log: ng.ILogService;
    public responseError = (response: ng.IHttpPromiseCallbackArg<any>) => {
        let $q = this.$injector.get("$q") as ng.IQService;
        let $session = this.$injector.get("session") as ISession;
        let $message = this.$injector.get("messageService") as IMessageService;
        let $log = this.$injector.get("$log") as ng.ILogService;

        let config = <IHttpInterceptorConfig>response.config;

        var deferred: ng.IDeferred<any> = $q.defer();

        if (config && (config.ignoreInterceptor || config.dontRetry)) {
            deferred.reject(response);

        } else if (response.status === HttpErrorStatusCodes.Inaccessible) {
            if (!this.canceledByUser(config)) {
                $message.addError("App_Error_Inaccessible"); // Service is inaccessible
            }
            deferred.reject();

        } else if (response.status === HttpErrorStatusCodes.Unauthorized) {
            $session.onExpired().then(() => {
                    if (config) {
                        var $http = <ng.IHttpService>this.$injector.get("$http");
                        HttpErrorInterceptor.applyNewSessionToken(config);

                        config.dontRetry = true;

                        $http(config).then(retryResponse => deferred.resolve(retryResponse), retryResponse => deferred.reject(retryResponse));
                    } else {
                        response.status = HttpErrorStatusCodes.UnauthorizedHandeled;
                        deferred.reject(response);
                    }
                },
                () => deferred.reject(response)
            );
        } else if (response.status === HttpErrorStatusCodes.Forbidden) {
            $message.addError("App_Error_Forbidden"); //Forbidden. The user does not have permissions for the artifact
            //here we need to reject with none object passed in, means that the error has been handled
            //TODO: change to deferred.reject();
            deferred.reject();

        } else if (response.status === HttpErrorStatusCodes.ServerError) {
            $message.addError("App_Error_InternalServer"); //Internal Server Error. An error occurred.
            //here we need to reject with none object passed in, means that the error has been handled
            //TODO: change to deferred.reject();
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
        if (config.timeout) {
            if (promise && promise["$$state"] && promise["$$state"].status === 1) {
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
