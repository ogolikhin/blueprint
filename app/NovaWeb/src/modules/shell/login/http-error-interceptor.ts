import "angular";
import {ISession} from "./session.svc";
import {IHttpInterceptorConfig} from "./auth.svc";
import {SessionTokenHelper} from "./session.token.helper";

export class HttpErrorInterceptor {
    //constructor(private $q: ng.IQService, private session: ISession) { }

    static $inject: [string] = ["$injector"];
    constructor(private $injector: ng.auto.IInjectorService) {
    }

    //static $inject: [string] = ["$q", "session"];

    public responseError = (response: ng.IHttpPromiseCallbackArg<any>) => {
        var $q: ng.IQService = <ng.IQService>this.$injector.get("$q");
        var session: ISession = <ISession>this.$injector.get("session");
        //var $http: ng.IHttpService = <ng.IHttpService>this.$injector.get("$http");


        var config = <IHttpInterceptorConfig>response.config;

        var deferred: ng.IDeferred<any> = $q.defer();

        if (config && config.ignoreInterceptor) {
            deferred.reject(response);
        } else if (response.status === 401) {
            session.onExpired().then(
                () => {
                    response.status = 1401;
                    deferred.reject(response);
                },
                () => deferred.reject(response)
            );
            
            //appendToRetryBuffer(response.config, deferred);
            //$rootScope.$broadcast("inactivityEvent");
        } else {
            //var errorMsg = (response.data ? response.data.message : null) || response.statusText || "An error occurred.";
            //var msg = messageService.getMessages();
            //if (msg.length === 0 || msg[msg.length - 1].messageText !== errorMsg) {
                //messageService.addError(errorMsg);
            //}
            deferred.reject(response);
        }

        return deferred.promise;

    };

    /*public static factory: any[] = [
        "$q",
        "$rootScope",
        "$injector",
        "messageService",
        (
            $q: ng.IQService,
            //$rootScope: ng.IRootScopeService,
            $injector: ng.auto.IInjectorService
            //messageService: IMessageService
        ) => {
*/
            /*var state: ng.ui.IStateService;

            var loginService: ILoginService;
            var isLoggedId = () => {
                loginService = loginService || $injector.get("loginService");
                return loginService.isLoggedin();
            };

            var retryHttpBuffer: IRetryHttpBuffer;
            var appendToRetryBuffer = (config: ng.IRequestConfig, deferred: ng.IDeferred<any>) => {
                retryHttpBuffer = retryHttpBuffer || $injector.get("retryHttpBuffer");
                retryHttpBuffer.append(config, deferred);
            };

            return new HttpErrorInterceptor($q,
                $rootScope,
                isLoggedId,
                appendToRetryBuffer,
                messageService);
       */ //}];

    /*constructor(
        $q: ng.IQService,
        $rootScope: ng.IRootScopeService,
        isLoggedIn: () => boolean,
        appendToRetryBuffer: (config: ng.IRequestConfig, deferred: ng.IDeferred<any>) => void,
        messageService: IMessageService) {

        this.responseError = (response: ng.IHttpPromiseCallbackArg<any>) => {
            var config = <IHttpInterceptorConfig>response.config;

            var deferred: ng.IDeferred<any> = $q.defer();

            if (config && config.ignoreInterceptor) {
                deferred.reject(response);
            } else if (response.status === 401) {
                appendToRetryBuffer(response.config, deferred);
                $rootScope.$broadcast("inactivityEvent");
            } else {
                var errorMsg = (response.data ? response.data.message : null) || response.statusText || "An error occurred.";
                var msg = messageService.getMessages();
                if (msg.length === 0 || msg[msg.length - 1].messageText !== errorMsg) {
                    messageService.addError(errorMsg);
                }
                deferred.reject(response);
            }

            return deferred.promise;
        };
    }*/
}

//var app = angular.module("Shell");
//app.factory("httpErrorInterceptor", HttpErrorInterceptor.factory);