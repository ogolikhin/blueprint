import "angular";
import {ISession} from "../login/session.svc";
import {SessionTokenHelper} from "../login/session.token.helper";
import {IMessageService, IHttpInterceptorConfig, HttpStatusCode, IApplicationError, ApplicationError} from "../../core";

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

        const deferred: ng.IDeferred<any> = $q.defer();

        let error: ApplicationError = this.createApplicationError(response);
        
        if (config.ignoreInterceptor) {
            response.data = this.createApplicationError(response);
            deferred.reject(response);
        } else if (response.status === HttpStatusCode.Unavailable) {
            if (!this.canceledByUser(config)) {
                $message.addError("HttpError_ServiceUnavailable"); // Service is unavailable
                response.data = this.createApplicationError(response, { handled: true});
                deferred.reject(response);
            } else {
                deferred.reject();
            }
        } else if (response.status === HttpStatusCode.Unauthorized) {
            $session.onExpired().then(
                () => {
                    if (!config.dontRetry) {
                        const $http = <ng.IHttpService>this.$injector.get("$http");
                        HttpErrorInterceptor.applyNewSessionToken(config);

                        config.dontRetry = true;

                        $http(config).then(
                            retryResponse => deferred.resolve(retryResponse), 
                            retryResponse => {
                                retryResponse.data = this.createApplicationError(retryResponse);
                                deferred.reject(retryResponse); });
                    } else {
                        response.data = this.createApplicationError(response); 
                        deferred.reject(response);
                    }
                },
                () => {
                    response.data = this.createApplicationError(response);
                    deferred.reject(response);
                }
            );
        } else if (response.status === HttpStatusCode.Forbidden) {
            $message.addError("HttpError_Forbidden"); //Forbidden. The user does not have permissions for the artifact
            response.data = this.createApplicationError(response, {
                message: "HttpError_Forbidden",
                handled: true
            });
            //here we need to reject with none object passed in, means that the error has been handled
            deferred.reject(response);
            

        } else if (response.status === HttpStatusCode.ServerError) {
            $message.addError("HttpError_InternalServer"); //Internal Server Error. An error occurred.
            //here we need to reject with none object passed in, means that the error has been handled
            response.data = this.createApplicationError(response, {
                message: "HttpError_InternalServer",
                handled: true
            });
            deferred.reject(response);

        } else {
            $log.error(response.data);
            response.data = this.createApplicationError(response);
            deferred.reject(response);
        }

        return deferred.promise;
    };

    private createApplicationError(response: ng.IHttpPromiseCallbackArg<any>, data?: IApplicationError): ApplicationError {
        let error = new ApplicationError(data);
        if (!error.message) {
            error.message = response.data ? response.data.message : response.statusText;
        }
        if (!error.statusCode) {
            error.statusCode = response.status;
        }
        if (!error.errorCode) {
            error.errorCode = response.data ? response.data.errorCode : undefined;
        }
        if (!error.errorContent) {
            error.errorContent = response.data ? response.data.errorContent : undefined;
        }
        return error;         
    }
    

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
        const token = SessionTokenHelper.getSessionToken();
        if (token) {
            if (!config.headers) {
                config.headers = {};
            }
            config.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = token;
        }
    }
}
