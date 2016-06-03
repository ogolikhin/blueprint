import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "../core";
import {AppComponent} from "./app.component";
import {AuthSvc} from "./login/auth.svc";
import {ISession, SessionSvc} from "./login/session.svc";
import {HttpErrorInterceptor} from "./login/http-error-interceptor";
import {ServerLoggerSvc} from "./log/server-logger.svc";
import {Logger} from "./log/logger.ts";
import {SessionTokenInterceptor} from "./login/session-token-interceptor";

angular.module("app.shell",
    [
        "app.core",
        "ui.router",
        "ui.bootstrap",
        "ngSanitize"
    ])
    .component("app", new AppComponent())
    .service("auth", AuthSvc)
    .service("session", SessionSvc)
    .service("sessionTokenInterceptor", SessionTokenInterceptor)
    .service("httpErrorInterceptor", HttpErrorInterceptor)
    .service("serverLogger", ServerLoggerSvc)
    .config(Logger)
    .config(initializeInterceptors);

function initializeInterceptors($httpProvider: ng.IHttpProvider) {
    $httpProvider.interceptors.push("sessionTokenInterceptor");
    $httpProvider.interceptors.push("httpErrorInterceptor");
}
initializeInterceptors.$inject = ["$httpProvider"];

//TODO: move to other file
export class AuthenticationRequired {
    private static key = "authenticated";
    public resolve = {};

    constructor() {
        this.resolve[AuthenticationRequired.key] = [
            "$log", "session", ($log: ng.ILogService, session: ISession): ng.IPromise<any> => {
                $log.debug("AuthenticationRequired...called");
                return session.ensureAuthenticated();
            }
        ];
    }
}

export { IServerLogger } from "./log/server-logger.svc";