import "angular";
import { ISession } from "../login/session.svc";

export class HttpHandledErrorStatusCodes {
    public static get handledUnauthorizedStatus() {
        return 1401;
    }
}

export interface IHttpInterceptorConfig extends ng.IRequestConfig {
    ignoreInterceptor: boolean;
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

        if (config && config.ignoreInterceptor) {
            deferred.reject(response);
        } else if (response.status === 401) {
            session.onExpired().then(
                () => {
                    response.status = HttpHandledErrorStatusCodes.handledUnauthorizedStatus;
                    deferred.reject(response);
                },
                () => deferred.reject(response)
            );
        } else {
            deferred.reject(response);
        }

        return deferred.promise;
    };
}
