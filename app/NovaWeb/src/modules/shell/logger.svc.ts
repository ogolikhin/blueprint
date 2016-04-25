import "angular";
import {SessionTokenHelper}  from "./login/session.token.helper";

export interface ILogger {
    log(message: any, level: number): ng.IPromise<any>;
}

interface IServiceLogModel {
    Source: string;
    LogLevel: number;
    Message: string;
    //System.DateTime OccurredAt  //set on server
    SessionId: string;
    UserName: string;
    MethodName: string; 
    FilePath: string;
    LineNumber: number;
    StackTrace: string;
}

export class LoggerSvc implements ILogger {
    static $inject: [string] = ["$injector"];
    constructor(private $injector: ng.auto.IInjectorService) {
    }

    public log(message: any, level: number): ng.IPromise<any> {

        var $q: ng.IQService = <ng.IQService>this.$injector.get("$q");
        var $http: ng.IHttpService = <ng.IHttpService>this.$injector.get("$http");

        var deferred: ng.IDeferred<any> = $q.defer();

        var config = <ng.IRequestConfig>{};
        config.headers = {};
        config.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = SessionTokenHelper.getSessionToken();

        var sessionId: string = SessionTokenHelper.getSessionToken();
        if (!sessionId) {
            sessionId = "";
        } else {
            sessionId = sessionId.substr(0, 8);
        }

        var logMessage: IServiceLogModel = <IServiceLogModel>{
            Source: "NovaClient",
            LogLevel: level,
            Message: message.message,
            SessionId: sessionId,
            UserName: "",
            MethodName: "",
            FilePath: "",
            LineNumber: 0,
            StackTrace: message.stack ? message.stack : ""
        };

        $http.post("/svc/adminstore/log", angular.toJson(logMessage), config)
            .success(() => {
                deferred.resolve()
            })
            .error(() => {
                deferred.reject()
            });

        return deferred.promise;
    };
};