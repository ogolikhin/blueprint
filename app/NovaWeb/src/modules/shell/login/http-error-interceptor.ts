import "angular";
import {ISession} from "./session.svc";
import {IHttpInterceptorConfig} from "./auth.svc";
import {SessionTokenHelper} from "./session.token.helper";

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
                    response.status = 1401;
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
