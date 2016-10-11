import * as angular from "angular";

export interface IServerLogger {
    log(message: any, level: number): ng.IPromise<any>;
}

interface IServerLogModel {
    Source: string;
    LogLevel: number;
    Message: string;
    //System.DateTime OccurredAt  //set on server
    //SessionId: string;  //set on server
    //UserName: string;  //set on server
    //MethodName: string;  //set on server
    //FilePath: string;  //set on server
    //LineNumber: number;  //set on server
    StackTrace: string;
}

export class ServerLoggerSvc implements IServerLogger {
    static $inject: [string] = ["$injector"];

    constructor(private $injector: ng.auto.IInjectorService) {
    }

    public log(message: any, level: number): ng.IPromise<any> {

        var $q: ng.IQService = <ng.IQService>this.$injector.get("$q");
        var $http: ng.IHttpService = <ng.IHttpService>this.$injector.get("$http");

        var deferred: ng.IDeferred<any> = $q.defer();

        var logMessage: IServerLogModel = <IServerLogModel>{
            Source: "NovaClient",
            LogLevel: level,
            Message: message.message,
            StackTrace: message.stack ? message.stack : ""
        };

        $http.post("/svc/adminstore/log", angular.toJson(logMessage))
            .then(() => {
                deferred.resolve();
            }, () => {
                deferred.reject();
            });

        return deferred.promise;
    };
}
;
